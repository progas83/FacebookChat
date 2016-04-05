using System;
using System.Collections.Generic;

namespace FacebookPy.EventsArgs
{
    public class FbContactsEventArgs : EventArgs
    {
        private Dictionary<string, FacebookContact>.ValueCollection valueCollection;
        private string error;

        public IEnumerable<FacebookContact> Contacts { get; private set; }

        public string ErrorMsg { get; private set; }

        public FbContactsEventArgs(IEnumerable<FacebookContact> contacts, string error = "")
        {
            Contacts = contacts;
            ErrorMsg = error;
        }
    }
}