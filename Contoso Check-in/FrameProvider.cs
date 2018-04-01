using System;

namespace ContosoCheckIn
{
    public abstract class FrameProvider
    {
        public int CameraNumber
        {
            get;
            private set;
        }

        public FrameProvider(int cameraNumber)
        {
            CameraNumber = cameraNumber;
        }

        public abstract void Start();
        public abstract void Stop();

        public event EventHandler<NewFrameEventArgs> ProcessingLiveFrame;

        protected void OnNewFrameProvided(VideoFrame frame)
        {
            if(frame == null || frame.Image == null)
            {
                // this should never happen
                // TODO fix this
                throw new Exception("Tell Adrián that fix this bug #23");
            }
            ProcessingLiveFrame?.Invoke(this, new NewFrameEventArgs(frame));
        }
    }

    public class NewFrameEventArgs : EventArgs
    {
        public NewFrameEventArgs(VideoFrame frame)
        {
            Frame = frame;
        }
        public VideoFrame Frame { get; }
    }
}
