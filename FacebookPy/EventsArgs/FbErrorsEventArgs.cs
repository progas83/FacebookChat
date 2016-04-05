using System;
using System.Net;

namespace FacebookPy.EventsArgs
{
    public class FbErrorsEventArgs : EventArgs
    {
        public HttpStatusCode StatusCode { get; set; }

        public string StatusDescription { get; set; }

        public Exception FbException { get; private set; }

        public FbErrorsEventArgs(Exception ex, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            FbException = ex;
            StatusCode = statusCode;
            StatusDescription = string.Empty;
        }
    }
}