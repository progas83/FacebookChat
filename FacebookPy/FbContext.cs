using FacebookPy.EventsArgs;
using System;
using System.Collections.Generic;

namespace FacebookPy
{
    public class FbContext : IDisposable
    {
        private bool _connected = false;

        public bool Connected
        {
            get
            {
                return _connected;
            }
            private set
            {
                _connected = value;
            }
        }

        public string FbUserId
        {
            get
            {
                return _fbRequest.DataBag.Uid;
            }
        }

        private readonly string _login;
        private readonly string _password;
        private readonly FbRequest _fbRequest;
        private FbConnection _fbConnection;
        private FbContacts _fbContacts;
        private FbConversation _fbConversation;

        public event EventHandler<StatusChangedEventArgs> ConnectionStatusChanged;

        public event EventHandler<FbContactsEventArgs> UpdatedContacts;

        public event EventHandler<MessageEventArgs> IncomingMessage;

        public event EventHandler<FbContactsIdsEventArgs> UpdatedOnlineUsersIds;

        public event EventHandler<FbErrorsEventArgs> FacebookError;

        public FbContext(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                throw new Exception("Login or password is empty");
            }

            _login = login;
            _password = password;
            _fbRequest = new FbRequest(this);
            _fbRequest.ErrorOccurs += OnFbRequest_ErrorOccurs;

            _fbConnection = new FbConnection(_fbRequest);

            _fbContacts = new FbContacts(_fbRequest);
            _fbContacts.UpdatedContacts += OnUpdateFacebookContacts;
            _fbContacts.OnlineContactsIds += OnUpdateOnlineUsersStatus;
            _fbConversation = new FbConversation(_fbRequest);
            _fbConversation.IncomingMessage += OnFbIncomingMessage;
            _fbConnection.ConnectionStatusChanged += FbConnectionStatusChanged;
        }

        private void OnFbRequest_ErrorOccurs(object sender, FbErrorsEventArgs e)
        {
            EventHandler<FbErrorsEventArgs> handler = FacebookError;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        private void OnUpdateOnlineUsersStatus(object sender, FbContactsIdsEventArgs e)
        {
            var handlerUpdatedOnlineUsersIds = UpdatedOnlineUsersIds;
            if (handlerUpdatedOnlineUsersIds != null)
                handlerUpdatedOnlineUsersIds(this, e);
        }

        public bool LogIn()
        {
            bool result = false;
            if (!Connected && _fbConnection != null)
            {
                result = _fbConnection.LogIn(_login, _password);
            }

            return result;
        }

        public void LogOut()
        {
            if (_fbConnection != null)
                _fbConnection.LogOut();
            if (_fbConversation != null)
                _fbConversation.LogOut();
        }

        public void UpdateFacebookContacts()
        {
            if (_fbContacts != null)
                _fbContacts.UpdateContactsList();
        }

        public FacebookContact GetContactById(long id)
        {
            FacebookContact contact = new FacebookContact();
            if (_fbContacts != null)
                contact = _fbContacts.GetUserInfo(id.ToString());
            return contact;
        }

        public void UpdateUsersStatus()
        {
            if (_fbContacts != null)
            {
                _fbContacts.UpdateUsersStatus();
            }
        }

        public IEnumerable<UserInfo> FindUsersByName(string name)
        {
            List<UserInfo> foundedUsers = new List<UserInfo>();
            if (_fbContacts != null)
                foundedUsers = new List<UserInfo>(_fbContacts.FindUsersByName(name));
            return foundedUsers;
        }

        public bool SendMessage(string recipient, string message)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(recipient))
            {
                result = _fbConversation.SendMessage(recipient, message);
            }
            return result;
        }

        private void OnUpdateFacebookContacts(object sender, FbContactsEventArgs e)
        {
            var handlerContactsUpdate = UpdatedContacts;
            if (handlerContactsUpdate != null)
            {
                handlerContactsUpdate(this, e);
            }
        }

        private void OnFbIncomingMessage(object sender, MessageEventArgs e)
        {
            var handlerIncomingMessage = IncomingMessage;
            if (handlerIncomingMessage != null)
                handlerIncomingMessage(this, e);
        }

        private void FbConnectionStatusChanged(object sender, StatusChangedEventArgs e)
        {
            Connected = e.CurrentStatus;
            var handlerConnectionStatus = ConnectionStatusChanged;
            if (handlerConnectionStatus != null)
                handlerConnectionStatus(this, e);
            if (e.CurrentStatus)
            {
                if (_fbRequest != null)
                    _fbRequest.UpdateSticky();
                if (_fbConversation != null)
                    _fbConversation.RunListeningIncomingMessages();
            }
        }

        public void Dispose()
        {
            _fbContacts.UpdatedContacts -= OnUpdateFacebookContacts;

            _fbConversation.IncomingMessage -= OnFbIncomingMessage;
            _fbConnection.ConnectionStatusChanged -= FbConnectionStatusChanged;
            _fbRequest.ErrorOccurs -= OnFbRequest_ErrorOccurs;
            _fbContacts.Dispose();
            _fbConnection.Dispose();
        }
    }
}