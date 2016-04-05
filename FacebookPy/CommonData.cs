using System;

namespace FacebookPy
{
    internal class CommonData
    {
        private string _clientId = string.Empty;

        public string ClientId
        {
            get { return _clientId; }
            set { _clientId = value; }
        }

        private int _reqCounter;
        private string _seq;

        public CommonData()
        {
            _reqCounter = 1;
            _seq = Convert.ToString(0);
        }

        public static long Now()
        {
            return (long)Math.Round((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds);
        }

        public int RequestCounter
        {
            get { return _reqCounter; }
            set { _reqCounter = value; }
        }

        public string Seq
        {
            get { return _seq; }
            set
            {
                _seq = value;
            }
        }

        public string Client
        {
            get { return "mercury"; }
        }

        private string _uid = string.Empty;

        public string Uid
        {
            get { return _uid; }
            set { _uid = value; }
        }

        private string _sticky;

        public string Sticky
        {
            get { return _sticky; }
            set { _sticky = value; }
        }

        private string _pool;

        public string Pool
        {
            get
            {
                return _pool;
            }
            set
            {
                _pool = value;
            }
        }
    }
}