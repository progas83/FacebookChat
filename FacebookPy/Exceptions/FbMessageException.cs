using System;

namespace FacebookPy.Exceptions
{
    public class FbMessageException : Exception
    {
        public string Error { get; set; }

        public string ErrorSummary { get; set; }

        public string ErrorDescription { get; set; }

        internal FbMessageException(FbMessageExceptionData errorData)
        {
            Error = errorData.Error;
            ErrorSummary = errorData.ErrorSummary;
            ErrorDescription = errorData.ErrorDescription;
        }
    }
}