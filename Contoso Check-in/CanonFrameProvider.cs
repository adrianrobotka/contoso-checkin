using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using EDSDKWrapper.Framework.Objects;
using EDSDKWrapper.Framework.Managers;
using System.Windows.Media.Imaging;

namespace ContosoCheckIn
{
    class CanonFrameProvider : FrameProvider
    {
        private TaskScheduler UITaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        private Task LiveViewCapturingTask { get; set; }
        private Camera Camera { get; set; }


        public CanonFrameProvider(Camera camera) : base(-1)
        {
            Camera = camera;
        }

        public override void Start()
        {
            LiveViewCapturingTask = Task.Factory.StartNew(() =>
            {
                var frameCount = 0;

                Camera.StartLiveView();

                while (Camera.LiveViewEnabled)
                {
                    int exceptionCount = 0;
                    try
                    {
                        var stream = Camera.GetLiveViewImage();

                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = stream;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();

                        var timestamp = DateTime.Now;
                        Mat image = bitmapImage.ToMat();
                        
                        // Package the image for submission.
                        VideoFrameMetadata meta;
                        meta.Index = frameCount;
                        meta.Timestamp = timestamp;
                        VideoFrame vframe = new VideoFrame(image, meta);

                        // Raise the new frame event
                        OnNewFrameProvided(vframe);

                        ++frameCount;

                        /*if (first)
                        {
                            first = false;
                            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                            Guid photoID = Guid.NewGuid();
                            String photolocation = photoID.ToString() + ".jpg";  //file name 

                            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                            using (var filestream = new FileStream(photolocation, FileMode.Create))
                                encoder.Save(filestream);
                        }*/

                        exceptionCount = 0;
                    }
                    catch
                    {
                        Thread.Sleep(100);
                        if (++exceptionCount > 10)
                        {
                            throw;
                        }
                    }
                }

            }).ContinueWith((previewsTask) =>
            {
                if (previewsTask.IsFaulted)
                {
                    throw previewsTask.Exception;
                }
            }, UITaskScheduler);
        }

        public override void Stop()
        {
            Camera.StopLiveView();
            LiveViewCapturingTask.Wait();
        }
    }
}
