using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ContosoCheckIn
{
    /// <summary>
    /// User of the app and the API; also used for Json.Net deserialization
    /// </summary>
    public class Participant
    {
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string company { get; set; }
        public string workTitle { get; set; }
        public string birth { get; set; }
        public string email { get; set; }
        public string groupName { get; set; }
        public string groupDescription { get; set; }
        public List<FaceImage> faces { get; set; }

        public override string ToString()
        {
            return $"{firstName} {lastName} at {company} as {workTitle} with {faces.Count()} images";
        }
    }

    /// <summary>
    /// Image of an API person's face; also used for Json.Net deserialization
    /// </summary>
    public class FaceImage
    {
        public string id { get; set; }
        public string url { get; set; }

        public ImageSource Source()
        {
            return new BitmapImage(new Uri(url, UriKind.Absolute));
        }
    }

    /// <summary>
    /// Custom API identify request's response; also used for Json.Net deserialization
    /// </summary>
    public class ParticipantIdentifyResult
    {
        public Participant participant { get; set; }
        public float confidence { get; set; }
    }

    public class FaceDetectionResult
    {
        public FaceDetectionResult(Face[] faces)
        {
            Faces = faces;
        }

        public Face[] Faces
        {
            get;
            private set;
        }
    }
}
