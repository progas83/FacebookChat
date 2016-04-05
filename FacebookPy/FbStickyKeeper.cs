using Newtonsoft.Json;

namespace FacebookPy
{
    internal class FbStickyKeeper
    {
        public FbStickyKeeper()
        {
        }

        [JsonProperty(PropertyName = "lb_info")]
        public FbSticky FacebookSticky { get; set; }

        internal class FbSticky
        {
            public FbSticky(string sticky, string pool)
            {
                Sticky = sticky;
                Pool = pool;
            }

            public string Sticky { get; private set; }

            public string Pool { get; private set; }
        }
    }
}