using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ProjectOxford.Face.Contract;
using Emgu.CV;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using OpenCvSharp.Extensions;
using Emgu.CV.Structure;

namespace ContosoCheckIn
{
    /// <summary>
    /// Utility codes
    /// </summary>
    class Util
    {
        /// <summary>
        /// Get biggest face index 
        /// but returns -1 if difference between the two biggest face 
        /// is under minDelta (determined by settings)
        /// </summary>
        /// <param name="faces">Faces</param>
        /// <returns>biggest face index or -1</returns>
        public static int GetDominantFaceIndex(Face[] faces)
        {
            float minDelta = Properties.Settings.Default.MinFaceDeltaPercent;
            return GetDominantFaceIndex(faces, minDelta);
        }

        /// <summary>
        /// Get biggest face index 
        /// but returns -1 if difference between the two biggest face 
        /// is under minDelta
        /// </summary>
        /// <param name="faces">Faces array</param>
        /// <param name="minDeltaPercent">minDelta in percentage</param>
        /// <returns>biggest face index or -1</returns>
        public static int GetDominantFaceIndex(Face[] faceArray, float minDeltaPercent = 0.1f)
        {
            if (faceArray == null)
            {
                return -1;
            }

            if (faceArray.Length < 2)
            {
                return 0;
            }

            List<FaceWithArea> faces = new List<FaceWithArea>();
            for (int i = 0; i < faceArray.Length; i++)
            {
                faces.Add(new FaceWithArea() { Face = faceArray[i], Index = i });
            }

            FaceWithArea biggest = faces.OrderByDescending(x => x.Area).First();
            FaceWithArea secondBiggest = faces.OrderByDescending(x => x.Area).ElementAt(1);

            float delta = biggest.Area - secondBiggest.Area;
            float deltaPercentage = delta / biggest.Area;

            if (deltaPercentage < minDeltaPercent)
            {
                return -1;
            }

            return biggest.Index;
        }

        private static IImage FrameToImpossibleImageType(VideoFrame frame)
        {
            BitmapSource bitmapsource = frame.Image.ToBitmapSource();

            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }

            return new Image<Gray, Byte>(bitmap);
        }

        class FaceWithArea
        {
            public Face Face
            {
                get;
                set;
            }

            public int Index
            {
                get;
                set;
            }

            public int Area
            {
                get
                {
                    return Face.FaceRectangle.Height * Face.FaceRectangle.Width;
                }
            }
        }
    }
}
