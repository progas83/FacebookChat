namespace FacebookPy.Exceptions
{
    internal class FbMessageExceptionData
    {
        public string Error { get; set; }

        public string ErrorSummary { get; set; }

        public string ErrorDescription { get; set; }

        public FbMessageExceptionData()
        {
        }

        public bool HasError
        {
            get
            {
                return !string.IsNullOrEmpty(this.Error);
            }
        }
    }
}