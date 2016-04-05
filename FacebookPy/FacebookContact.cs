using Newtonsoft.Json;
using System;
using System.Net;

namespace FacebookPy
{
    public class FacebookContact
    {
        public FacebookContact()
        {
        }

        [JsonProperty(PropertyName = "id")]
        public string ThreadId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string FullName { get; set; }

        public string FirstName { get; set; }

        public string Vanity { get; set; }

        public string ThumbSrc { get; set; }

        [JsonProperty(PropertyName = "uri")]
        public string HomePage { get; set; }

        public int Gender { get; set; }

        public string Type { get; set; }

        [JsonProperty(PropertyName = "is_friend")]
        public bool IsFriend { get; set; }

        [JsonProperty(PropertyName = "mThumbSrcSmall")]
        public string ThumbSrcSmall { get; set; }

        [JsonProperty(PropertyName = "mThumbSrcLarge")]
        public string ThumbSrcLarge { get; set; }

        public byte[] LoadAvatar()
        {
            byte[] imageBytes = new byte[0];
            if (!string.IsNullOrEmpty(ThumbSrc))
            {
                var webClient = new WebClient();
                try
                {
                    imageBytes = webClient.DownloadData(ThumbSrc);
                }
                catch (Exception ex)
                {
                }
            }
            return imageBytes;
        }

        public bool IsOnline { get; set; }
    }
}