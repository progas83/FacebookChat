using System;
using System.Collections.Generic;

namespace FacebookPy.EventsArgs
{
    public class FbContactsIdsEventArgs : EventArgs
    {
        public List<string> OnlineIds { get; private set; }

        public string Error { get; private set; }

        public FbContactsIdsEventArgs(IEnumerable<string> onlineIds, string error = "")
        {
            OnlineIds = (onlineIds != null) ? new List<string>(onlineIds) : new List<string>();
            Error = error;
        }
    }
}