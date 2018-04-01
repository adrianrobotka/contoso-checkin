using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.ProjectOxford.Face.Contract;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ContosoCheckIn
{
    public class EventHandler
    {
        private TimeSpan FaceDetectionTimeout = TimeSpan.FromMilliseconds(Properties.Settings.Default.FaceDetectionTimeout);
        private TimeSpan IdentifyTimeout = TimeSpan.FromMilliseconds(Properties.Settings.Default.IdentifyTimeout);

        public FrameProvider FrameProvider
        {
            get;
            private set;
        }

        /// <summary>
        /// Incoming result of the API face detection event handler
        /// </summary>
        public event EventHandler<NewRemoteDetectionEventArgs> ProcessingRemoteDetectResult;

        /// <summary>
        /// Incoming result of the face identify event handler
        /// </summary>
        public event EventHandler<NewIdentifyEventArgs> ProcessingIdentifyResult;

        /// <summary>
        /// Local face detection event handler
        /// </summary>
        public event EventHandler<NewLocalDetectionEventArgs> ProcessingLocalDetectionResult;

        private int ChainReactionCounter = 0;
        private int Infinity = Properties.Settings.Default.BlockInfinity;

        private LocalFaceDetector localFaceDetector = null;
        private float TimerMultiplier = 5;

        public EventHandler(FrameProvider frameProvider, float timermultiplier)
        {
            FrameProvider = frameProvider;
            TimerMultiplier = timermultiplier;
            localFaceDetector = new LocalFaceDetector();
            SetInternals();
        }

        #region EventHandling
        private void SetInternals()
        {
            FrameProvider.ProcessingLiveFrame += FrameProvider_ProcessingLiveFrameAsync;
            ProcessingLocalDetectionResult += EventHandler_ProcessingLocalDetectionResultAsync;
            ProcessingRemoteDetectResult += EventHandler_ProcessingRemoteDetectResultAsync;
            ProcessingIdentifyResult += EventHandler_ProcessingIdentifyResult;
        }

        private async void FrameProvider_ProcessingLiveFrameAsync(object sender, NewFrameEventArgs e)
        {
            // Run local face detection
            if (ChainReactionCounter < 0)
            {
                ChainReactionCounter = Infinity;

                bool localDetection = Properties.Settings.Default.LocalFaceDetection;

                if (localDetection)
                {
                    StartLocalFrameDetectionTask(e.Frame);
                }
                else
                {
                    NewRemoteDetectionEventArgs result = await RunRemoteFaceDetection(e.Frame);
                    OnRemoteDetectionResultProvided(result);
                }
            }

            ChainReactionCounter--;
        }

        private void StartLocalFrameDetectionTask(VideoFrame frame)
        {
            List<Rectangle> faces = localFaceDetector.Detect(frame.Image);

            NewLocalDetectionEventArgs args = new NewLocalDetectionEventArgs(frame, faces);
            OnLocalDetectionResultProvided(args);
        }

        private async void EventHandler_ProcessingLocalDetectionResultAsync(object sender, NewLocalDetectionEventArgs e)
        {
            if (e.Faces.Count == 0)
            {
                ChainReactionCounter = 1;
                return;
            }

            NewRemoteDetectionEventArgs result = await RunRemoteFaceDetection(e.Frame);
            OnRemoteDetectionResultProvided(result);
        }

        private async void EventHandler_ProcessingRemoteDetectResultAsync(object sender, NewRemoteDetectionEventArgs e)
        {
            if (e.TimedOut || e.Exception != null)
            {
                ChainReactionCounter = (int)(Properties.Settings.Default.BlockOnNoFace * TimerMultiplier);
                return;
            }

            if (e.Faces.Count() == 0)
            {
                ChainReactionCounter = (int)(Properties.Settings.Default.BlockOnNoFace * TimerMultiplier);
                return;
            }

            int biggestFaceIndex = Util.GetDominantFaceIndex(e.Faces);

            // There is a handsome face to identify XD
            if (e.Faces.Count() > 0 && biggestFaceIndex != -1)
            {
                Guid selectedFaceId = e.Faces[biggestFaceIndex].FaceId;
                NewIdentifyEventArgs result = await RunFaceIdentify(e.Frame, selectedFaceId);
                OnIdentifyResultProvided(result);
            }
            if (biggestFaceIndex == -1)
            {
                ChainReactionCounter = (int)(Properties.Settings.Default.BlockOnNoIdentity * TimerMultiplier);
            }
        }

        private void EventHandler_ProcessingIdentifyResult(object sender, NewIdentifyEventArgs e)
        {
            if (e.TimedOut || e.Exception != null)
            {
                // server error
                ChainReactionCounter = (int)(Properties.Settings.Default.BlockOnNoIdentity * TimerMultiplier);
                return;
            }

            switch (e.Faces.Count())
            {
                case 0:
                    ChainReactionCounter = (int)(Properties.Settings.Default.BlockOnNoIdentity * TimerMultiplier);
                    break;
                case 1:
                    ChainReactionCounter = (int)(Properties.Settings.Default.BlockOnPrimaryIdentity * TimerMultiplier);
                    break;
                default:
                    ChainReactionCounter = int.MaxValue;
                    break;
            }
        }
        #endregion

        public void resetBlock()
        {
            ChainReactionCounter = (int)(Properties.Settings.Default.BlockOnNoFace * TimerMultiplier);
        }

        protected async Task<NewRemoteDetectionEventArgs> RunRemoteFaceDetection(VideoFrame frame)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            NewRemoteDetectionEventArgs output = new NewRemoteDetectionEventArgs(frame);
            var task = ApiClient.DetectAsync(frame.Image);
            try
            {
                if (task == await Task.WhenAny(task, Task.Delay(FaceDetectionTimeout, source.Token)))
                {
                    output.Faces = (await task).Faces;
                    source.Cancel();
                }
                else
                {
                    output.TimedOut = true;
                    output.Faces = new Face[] { };
                }
            }
            catch (Exception ae)
            {
                output.Exception = ae;
                output.Faces = new Face[] { };
            }

            return output;
        }

        protected async Task<NewIdentifyEventArgs> RunFaceIdentify(VideoFrame frame, Guid faceId)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            NewIdentifyEventArgs output = new NewIdentifyEventArgs();
            var task = ApiClient.IdentifyPersonAsync(faceId);
            try
            {
                if (task == await Task.WhenAny(task, Task.Delay(IdentifyTimeout, source.Token)))
                {
                    output.Faces = await task;
                    source.Cancel();
                }
                else
                {
                    output.TimedOut = true;
                    output.Faces = new ParticipantIdentifyResult[] { };
                }
            }
            catch (Exception ae)
            {
                output.Exception = ae;
                output.Faces = new ParticipantIdentifyResult[] { };
            }

            return output;
        }

        protected void OnRemoteDetectionResultProvided(NewRemoteDetectionEventArgs args)
        {
            ProcessingRemoteDetectResult?.Invoke(this, args);
        }

        public void OnIdentifyResultProvided(NewIdentifyEventArgs args)
        {
            ProcessingIdentifyResult?.Invoke(this, args);
        }

        protected void OnLocalDetectionResultProvided(NewLocalDetectionEventArgs args)
        {
            ProcessingLocalDetectionResult?.Invoke(this, args);
        }

        public void Start()
        {
            FrameProvider.Start();
        }

        public void Stop()
        {
            FrameProvider.Stop();
        }
    }

    public class NewRemoteDetectionEventArgs : EventArgs
    {
        public NewRemoteDetectionEventArgs(VideoFrame frame)
        {
            Frame = frame;
        }
        public VideoFrame Frame { get; }
        public Face[] Faces { get; set; }
        public bool TimedOut { get; set; } = false;
        public Exception Exception { get; set; } = null;
    }

    public class NewLocalDetectionEventArgs : EventArgs
    {
        public NewLocalDetectionEventArgs(VideoFrame frame, List<Rectangle> faces)
        {
            Frame = frame;
            Faces = faces;
        }
        public VideoFrame Frame { get; }
        public List<Rectangle> Faces { get; }
    }

    public class NewIdentifyEventArgs : EventArgs
    {
        public ParticipantIdentifyResult[] Faces { get; set; }
        public bool TimedOut { get; set; } = false;
        public Exception Exception { get; set; } = null;
    }
}
