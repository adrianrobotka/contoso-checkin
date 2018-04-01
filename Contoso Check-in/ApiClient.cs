using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ContosoCheckIn
{
    class ApiClient
    {
        private static readonly HttpClient client = new HttpClient();

        private static readonly string SiteUrl = Properties.Settings.Default.SiteUrl;
        private static readonly string ApiUrlPrefix = SiteUrl + "api/";
        private static readonly string StaticAuthTag = "?apikey=" + Properties.Settings.Default.ApiKey;
        public static string GateName = "no name";

        private static string AuthTag
        {
            get
            {
                return StaticAuthTag + "&gate=" + GateName;
            }
        }

        string ApiUrl = Properties.Settings.Default.SiteUrl;

        private static readonly ImageEncodingParam[] jpegParams = {
            new ImageEncodingParam(ImwriteFlags.JpegQuality, 80)
        };

        /// <summary>
        /// After multiple candidates we need to log the selected candidate
        /// </summary>
        /// <param name="selectedParticipant"></param>
        public static async void LogSelectedParticipant(ParticipantIdentifyResult selectedParticipant)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
               { "candidate_id", selectedParticipant.participant.id.ToString() },
               { "confidence", selectedParticipant.confidence.ToString() },

            });

            string url = ApiUrlPrefix + "selectCandidate" + AuthTag;
            var response = await client.PostAsync(url, content);
            // TODO I had no time to handle errors
        }

        public static async Task<ParticipantIdentifyResult[]> IdentifyPersonAsync(Guid faceId)
        {
            int maxCandidates = Properties.Settings.Default.MaximumCandidates;
            float threshold = Properties.Settings.Default.MinimalThreshold;

            string thresholdString = threshold.ToString().Replace(',', '.');

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
               { "faceId", faceId.ToString() },
               { "maxNumOfCandidatesReturned", maxCandidates.ToString() },
               { "confidenceThreshold", thresholdString }
            });

            string url = ApiUrlPrefix + "identify" + AuthTag;
            var response = await client.PostAsync(url, content);
            string responseString = await response.Content.ReadAsStringAsync();

            ParticipantIdentifyResults candidates = JsonConvert.DeserializeObject<ParticipantIdentifyResults>(responseString);

            return candidates.results.ToArray();
        }

        public static async Task<FaceDetectionResult> DetectAsync(Mat image)
        {
            var jpg = image.ToMemoryStream(".jpg", jpegParams);
            MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new StreamContent(jpg), "image", "image.jpg");

            string url = ApiUrlPrefix + "detect" + AuthTag;
            var response = await client.PostAsync(url, content);
            string responseString = await response.Content.ReadAsStringAsync();

            DetectedFaces faceList = JsonConvert.DeserializeObject<DetectedFaces>(responseString);

            return new FaceDetectionResult(faceList.results.ToArray());
        }

        public static async Task<LoginResult> LoginAsync(string email, string password, string gate)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
               { "email", email },
               { "password", password },
               { "gate", gate }
            });

            string url = ApiUrlPrefix + "login" + AuthTag;
            var response = await client.PostAsync(url, content);
            string responseString = await response.Content.ReadAsStringAsync();

            LoginResult result = JsonConvert.DeserializeObject<LoginResult>(responseString);

            return result;
        }

    }

    /// <summary>
    /// Json.Net deserializator temp class
    /// </summary>
    class DetectedFaces
    {
        public List<Face> results { get; set; }
    }

    /// <summary>
    /// Json.Net deserializator temp class
    /// </summary>
    class IndexPageResponse
    {
        public string clientBackgroundUrl { get; set; }
    }

    /// <summary>
    /// Json.Net deserializator temp class
    /// </summary>
    class ParticipantIdentifyResults
    {
        public List<ParticipantIdentifyResult> results { get; set; }
    }

    /// <summary>
    /// Json.Net deserializator temp class
    /// </summary>
    class LoginResult
    {
        public string status { get; set; }

        public bool isValid
        {
            get
            {
                return status != null && status == "ok";
            }
        }
    }

}
