using Newtonsoft.Json;

namespace FacebookPy
{
    public class UserInfo
    {
        public UserInfo()
        { }

        [JsonProperty(PropertyName = "uid")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "photo")]
        public string PhotoHref { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string PagePath { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string TextName { get; set; }

        [JsonProperty(PropertyName = "names")]
        public string[] Names { get; set; }
    }
}