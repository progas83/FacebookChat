using FacebookPy.EventsArgs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

namespace FacebookPy
{
    internal class FbContacts : IDisposable
    {
        private FbRequest _fbRequest;

        public FbContacts(FbRequest _fbRequest)
        {
            this._fbRequest = _fbRequest;
        }

        internal event EventHandler<FbContactsEventArgs> UpdatedContacts;

        internal event EventHandler<FbContactsIdsEventArgs> OnlineContactsIds;

        internal void UpdateContactsList()
        {
            HttpStatusCode statusCode;
            Dictionary<string, FacebookContact> fbContacts = new Dictionary<string, FacebookContact>();
            NameValueCollection nvc = new NameValueCollection();
            nvc["viewer"] = _fbRequest.DataBag.Uid.ToString();
            string error = string.Empty;
            try
            {
                string resJson = _fbRequest.GetJsonFromPostMethod(FbEndpoints.GetAllFriendsURL, nvc, out statusCode);
                JObject jsonObject = JObject.Parse(resJson);
                string contactsContainer = jsonObject["payload"].ToString();

                fbContacts = JsonConvert.DeserializeObject<Dictionary<string, FacebookContact>>(contactsContainer);
            }
            catch (Exception ex)
            {
                if (_fbRequest != null)
                {
                    _fbRequest.InvokeRequestErrorEvent(this, new FbErrorsEventArgs(ex));
                }
                error = ex.Message;
            }
            if (UpdatedContacts != null)
            {
                UpdatedContacts(this, new FbContactsEventArgs(fbContacts.Values, error));
            }
        }

        internal IEnumerable<UserInfo> FindUsersByName(string name)
        {
            List<UserInfo> users = new List<UserInfo>();
            NameValueCollection nvc = new NameValueCollection();
            nvc["value"] = name.ToLower();
            nvc["viewer"] = _fbRequest.DataBag.Uid.ToString();
            nvc["rsp"] = "search";
            nvc["context"] = "search";
            nvc["context"] = "search";
            nvc["path"] = @"/home.php";
            nvc["request_id"] = Guid.NewGuid().ToString();

            string result = _fbRequest.GetTextResultFromGetMethod(FbEndpoints.SearchURL, nvc);
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Remove(0, result.IndexOf('{'));
                JToken jtoken = JObject.Parse(result);
                if (jtoken["payload"] != null && jtoken["payload"]["entries"] != null)
                {
                    try
                    {
                        users = JsonConvert.DeserializeObject<List<UserInfo>>(jtoken["payload"]["entries"].ToString());
                    }
                    catch (Exception ex)
                    {
                        if (_fbRequest != null)
                        {
                            _fbRequest.InvokeRequestErrorEvent(this, new FbErrorsEventArgs(ex));
                        }
                    }
                }
            }

            return users;
        }

        internal FacebookContact GetUserInfo(string id)
        {
            NameValueCollection nvc = new NameValueCollection();
            FacebookContact contact = new FacebookContact();
            string keyIds = string.Format("ids[{0}]", id);
            nvc[keyIds] = id;

            string result = _fbRequest.GetTextResultFromGetMethod(FbEndpoints.GetUserInfo, nvc);
            try
            {
                result = result.Remove(0, result.IndexOf('{'));

                JObject jsonObject = JObject.Parse(result);
                string contactsContainer = jsonObject["payload"]["profiles"].ToString();

                Dictionary<string, FacebookContact> fbContacts = JsonConvert.DeserializeObject<Dictionary<string, FacebookContact>>(contactsContainer);
                contact = fbContacts[id];
            }
            catch (Exception ex)
            {
                if (_fbRequest != null)
                {
                    _fbRequest.InvokeRequestErrorEvent(this, new FbErrorsEventArgs(ex));
                }
            }

            return contact;
        }

        internal void UpdateUsersStatus()
        {
            HttpStatusCode statusCode;
            string error = string.Empty;
            NameValueCollection nvc = new NameValueCollection();
            nvc["user"] = _fbRequest.DataBag.Uid.ToString();
            nvc["fetch_mobile"] = "False";
            nvc["get_now_available_list"] = "True";
            List<string> nowOnlineContactsId = new List<string>();
            try
            {
                string resJson = _fbRequest.GetJsonFromPostMethod(FbEndpoints.GetUsersStatusURL, nvc, out statusCode);
                JObject jsonObject = JObject.Parse(resJson);
                JObject jsonPayload = jsonObject["payload"] as JObject;
                JObject jsonBuddyList = jsonPayload["buddy_list"]["nowAvailableList"] as JObject;

                string rd = jsonBuddyList.ToString();
                Dictionary<string, Dictionary<string, string>> dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonBuddyList.ToString());

                foreach (var threadId in dict.Keys)
                {
                    if (Convert.ToBoolean(Convert.ToInt32(dict[threadId]["a"])))
                    {
                        nowOnlineContactsId.Add(threadId);
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                if (_fbRequest != null)
                {
                    _fbRequest.InvokeRequestErrorEvent(this, new FbErrorsEventArgs(ex));
                }
            }
            if (OnlineContactsIds != null)
            {
                OnlineContactsIds(this, new FbContactsIdsEventArgs(nowOnlineContactsId, error));
            }
        }

        public void Dispose()
        {
        }
    }
}