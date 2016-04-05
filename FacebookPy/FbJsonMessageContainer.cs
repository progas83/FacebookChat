using System.Collections.Generic;

namespace FacebookPy
{
    public class FbJsonMessageContainer
    {
        public string t { get; set; }

        public string seq { get; set; }

        public List<FbMessageEnvelop> ms { get; set; }

        public FbJsonMessageContainer()
        {
        }

        public class FbMessageEnvelop
        {
            public string type { get; set; }

            public FbMessageEnvelop()
            {
            }

            public FbMessage Message { get; set; }
        }
    }
}