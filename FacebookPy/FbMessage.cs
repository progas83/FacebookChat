namespace FacebookPy
{
    public class FbMessage
    {
        public FbMessage()
        {
        }

        public string Mercury_author_id
        {
            get;
            set;
        }

        public long Timestamp { get; set; }

        public string Mid { get; set; }

        public string Sender_name { get; set; }

        public bool Has_attachment { get; set; }

        public string Body { get; set; }

        public string Sender_fbid { get; set; }

        public bool Is_unread { get; set; }
    }
}