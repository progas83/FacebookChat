using System;

namespace FacebookPy.EventsArgs
{
    public class MessageEventArgs : EventArgs
    {
        public FbMessage Data
        {
            get;
            private set;
        }

        public MessageEventArgs(FbMessage data)
        {
            Data = data;
        }
    }
}