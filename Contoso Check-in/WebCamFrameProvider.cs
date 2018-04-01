using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace ContosoCheckIn
{
    class WebCamFrameProvider : FrameProvider
    {
        protected VideoCapture reader = null;
        protected Task producerTask = null;
        protected bool stopping = false;
        protected Timer timer = null;
        protected SemaphoreSlim timerMutex = new SemaphoreSlim(1);
        protected AutoResetEvent _frameGrabTimer = new AutoResetEvent(false);

        public WebCamFrameProvider(int cameraNumber) : base(cameraNumber)
        {
        }

        public override void Start()
        {
            // Check to see if we're re-opening the same camera. 
            if (reader != null && reader.CaptureType == CaptureType.Camera)
            {
                return;
            }

            var timerIterations = 0;

            reader = new VideoCapture(CameraNumber);

            producerTask = Task.Factory.StartNew(() =>
            {
                var frameCount = 0;
                while (!stopping)
                {
                    var timestamp = DateTime.Now;
                    Mat image = new Mat();
                    bool success = reader.Read(image);

                    if (!success)
                    {
                        // If we've reached the end of the video, stop here. 
                        if (reader.CaptureType == CaptureType.File)
                        {
                            Stop();
                            break;
                        }
                        else
                        {
                            // If failed on live camera, try again. 
                            continue;
                        }
                    }

                    // Package the image for submission.
                    VideoFrameMetadata meta;
                    meta.Index = frameCount;
                    meta.Timestamp = timestamp;
                    VideoFrame vframe = new VideoFrame(image, meta);
                    int size = image.ToBytes().Length;

                    // Raise the new frame event
                    OnNewFrameProvided(vframe);

                    ++frameCount;
                }

            }, TaskCreationOptions.LongRunning);

            int fps = Properties.Settings.Default.FPS;

            TimeSpan frameGrabDelay = TimeSpan.FromSeconds(1 / fps);

            // Set up a timer object that will trigger the frame-grab at a regular interval.
            timer = new Timer(async s /* state */ =>
            {
                await timerMutex.WaitAsync();
                try
                {
                    // If the handle was not reset by the producer, then the frame-grab was missed.
                    _frameGrabTimer.WaitOne(0);

                    _frameGrabTimer.Set();

                    /* if (missed)
                     {
                         //LogMessage("Timer: missed frame-grab {0}", timerIterations - 1);
                     }*/

                    //LogMessage("Timer: grab frame num {0}", timerIterations);
                    ++timerIterations;
                }
                finally
                {
                    timerMutex.Release();
                }
            }, null, TimeSpan.Zero, frameGrabDelay);
        }

        public override void Stop()
        {
            stopping = true;
            if (producerTask != null)
            {
                producerTask = null;
            }
            stopping = false;
        }
    }
}
