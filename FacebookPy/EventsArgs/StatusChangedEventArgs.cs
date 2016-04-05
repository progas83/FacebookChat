using System;

namespace FacebookPy.EventsArgs
{
    public class StatusChangedEventArgs : EventArgs
    {
        public bool CurrentStatus
        {
            get;
            private set;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public bool HasError
        {
            get;
            private set;
        }

        public StatusChangedEventArgs(bool currentStatus, string errorMsg)
        {
            CurrentStatus = currentStatus;
            ErrorMessage = errorMsg;
            if (!string.IsNullOrEmpty(ErrorMessage))
                HasError = true;
        }
    }
}