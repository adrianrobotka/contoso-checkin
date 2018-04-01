using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using OpenCvSharp.Extensions;
using System.Runtime.InteropServices;

namespace ContosoCheckIn
{
    public class LocalFaceDetector
    {
        private readonly Size minFaceSize = new Size(20, 20);


        public LocalFaceDetector()
        {
            // TODO replace this with setting
            //String classifierName = "haarcascade_frontalface_default.xml";
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
        
        public List<Rectangle> Detect(OpenCvSharp.Mat matImage)
        {
            List<Rectangle> faces = new List<Rectangle>();

            IImage image = null;

            try
            {
                image = new Image<Bgr, byte>(BitmapConverter.ToBitmap(matImage)).Mat;

                using (InputArray iaImage = image.GetInputArray())
                using (CascadeClassifier classifier = new CascadeClassifier("haarcascade_frontalface_default.xml"))
                using (UMat ugray = new UMat())
                {
                    CvInvoke.CvtColor(image, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                    //normalizes brightness and increases contrast of the image
                    CvInvoke.EqualizeHist(ugray, ugray);

                    //Detect the faces  from the gray scale image and store the locations as rectangle
                    //The first dimensional is the channel
                    //The second dimension is the index of the rectangle in the specific channel                     
                    Rectangle[] facesDetected = classifier.DetectMultiScale(ugray, 1.1, 10, minFaceSize);

                    faces.AddRange(facesDetected);
                }

            }
            catch
            { }

            if (image != null)
            {
                DeleteObject(image.Ptr);
            }

            return faces;
        }
    }
}
