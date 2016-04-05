using FacebookPy.EventsArgs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace FacebookPy
{
    public class FbConnection : IDisposable
    {
        private FbRequest _fbRequest;

        internal FbConnection(FbRequest fbRequest)
        {
            _fbRequest = fbRequest;
        }

        private readonly long _pingSleep = 10000;

        private long _startTime;
        private string _userChannel = string.Empty;
        private string _ttstamp = string.Empty;
        private string _fbDtsg = string.Empty;
        private double _prev = 0;
        private double _tmpPrev = 0;
        private double _lastSync = 0;

        private bool _needPing = false;
        private bool _pingIsBusy = false;

        internal event EventHandler<StatusChangedEventArgs> ConnectionStatusChanged;

        internal bool LogIn(string login, string password)
        {
            bool result = false;
            NameValueCollection nvc = new NameValueCollection();
            string errorMessage = string.Empty;
            try
            {
                string textResult = _fbRequest.GetTextResultFromGetMethod(FbEndpoints.MobileURL, null, true);

                nvc = HtmlToXmlParser.GetParametersForLogin(textResult);
                nvc["email"] = login;
                nvc["pass"] = HttpUtility.UrlEncode(password);
                using (HttpWebResponse response = _fbRequest.CleanPostMethod(FbEndpoints.LoginURL, nvc))
                {
                    if (response.ResponseUri.AbsolutePath.Contains("home"))
                    {
                        _fbRequest.DataBag.ClientId = UserAgents.GetRandomHexClientId();
                        this._startTime = CommonData.Now();
                        string c_userValue = _fbRequest.CurrentCookieContainer.GetCookies(response.ResponseUri)["c_user"].Value;
                        _fbRequest.DataBag.Uid = c_userValue;

                        this._userChannel = string.Format("p_{0}", _fbRequest.DataBag.Uid);
                        this._ttstamp = "";

                        string readText = _fbRequest.GetTextResultFromGetMethod(FbEndpoints.BaseURL);

                        this._fbDtsg = HtmlToXmlParser.GetFbDtsgValue(readText);
                        SetTTStamp();

                        _fbRequest.UpdatePayloadDefault(readText, _fbRequest.DataBag.Uid.ToString(), _ttstamp, _fbDtsg);
                        _fbRequest.UpdatePresenceCookie();
                        this._prev = CommonData.Now();
                        this._tmpPrev = CommonData.Now();
                        this._lastSync = CommonData.Now();

                        result = true;
                    }
                    else
                    {
                        errorMessage = "Login or password is incorrect";
                    }
                }
            }
            catch (Exception ex)
            {
                if (_fbRequest != null)
                {
                    _fbRequest.InvokeRequestErrorEvent(this, new FbErrorsEventArgs(ex));
                    errorMessage = ex.Message;
                }
            }
            if (ConnectionStatusChanged != null)
            {
                ConnectionStatusChanged(this, new StatusChangedEventArgs(result, errorMessage));
            }

            _needPing = result;
            StartPinging();

            return result;
        }

        internal void LogOut()
        {
            string logOutProcess = string.Empty;
            _needPing = false;
            if (FacebookLogoutCorrectly())
            {
                logOutProcess = "Facebook logout properly";
            }
            else
            {
                logOutProcess = "Facebook logout roughly";
            }

            if (ConnectionStatusChanged != null)
            {
                ConnectionStatusChanged(this, new StatusChangedEventArgs(false, string.Empty));
            }
        }

        private bool FacebookLogoutCorrectly()
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc["pmid"] = "0";
            HttpStatusCode statusCode;
            string resJson = _fbRequest.GetJsonFromPostMethod(FbEndpoints.ModernSettingsURL, nvc, out statusCode);
            nvc = ParsePayloadForLogOut(resJson);

            if (statusCode == HttpStatusCode.OK)
            {
                _fbRequest.GetJsonFromPostMethod(FbEndpoints.LogOutURL, nvc, out statusCode);
            }
            return statusCode == HttpStatusCode.OK;
        }

        private NameValueCollection ParsePayloadForLogOut(string resJson)
        {
            NameValueCollection nvc = new NameValueCollection();
            try
            {
                JObject jsonObject = JObject.Parse(resJson);
                JToken instancesContainer = jsonObject["jsmods"]["instances"];
                JArray instancesValues = (JArray)instancesContainer[0][2][0];
                JToken markupContainer = instancesValues.FirstOrDefault(i => i["value"] != null && i["value"].ToString().Equals("logout"));
                if (markupContainer != null)
                {
                    JToken markupToken = (JToken)markupContainer["markup"]["__m"];

                    JArray htmls = (JArray)jsonObject["jsmods"]["markup"];

                    JArray htmlContainer = (JArray)htmls.FirstOrDefault(h => h[0].ToString().Equals(markupToken.ToString()));

                    string htmlValue = ((JToken)htmlContainer[1])["__html"].ToString();

                    nvc = HtmlToXmlParser.GetFbLogoutValues(htmlValue);
                }
            }
            catch (Exception ex)
            {
                if (_fbRequest != null)
                {
                    _fbRequest.InvokeRequestErrorEvent(this, new FbErrorsEventArgs(ex));
                }
            }

            return nvc;
        }

        private void PingCallBack(IAsyncResult ar)
        {
            _pingIsBusy = false;
        }

        private void StartPinging()
        {
            Action RunPingFb = delegate
            {
                PingFb();
            };
            if (!_pingIsBusy)
            {
                RunPingFb.BeginInvoke(PingCallBack, null);
            }
        }

        private bool PingFb()
        {
            _pingIsBusy = true;

            bool result = false;

            NameValueCollection nvc = new NameValueCollection();
            while (_needPing)
            {
                string errorMsg = string.Empty;
                result = false;
                nvc["channel"] = _userChannel;
                nvc["clientid"] = _fbRequest.DataBag.ClientId;
                nvc["partition"] = "-2";
                nvc["cap"] = "0";
                nvc["uid"] = _fbRequest.DataBag.Uid.ToString();
                nvc["sticky"] = _fbRequest.DataBag.Sticky;
                nvc["viewer_uid"] = _fbRequest.DataBag.Uid.ToString();

                try
                {
                    using (HttpWebResponse response = _fbRequest.GetMethod(FbEndpoints.PingURL, nvc))
                        result = response.StatusCode == HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    if (_fbRequest != null)
                    {
                        _fbRequest.InvokeRequestErrorEvent(this, new FbErrorsEventArgs(ex));
                    }
                    errorMsg = ex.Message;
                }
                if (result != _fbRequest.Connected)
                {
                    ConnectionStatusChanged(this, new StatusChangedEventArgs(result, errorMsg));
                }

                Thread.Sleep(30000);
            }

            return result;
        }

        private void SetTTStamp()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _fbDtsg.Length; i++)
            {
                Char ch = _fbDtsg[i];
                sb.Append(Convert.ToInt32(ch));
            }
            sb.Append(2);
            _ttstamp = sb.ToString();
        }

        public void Dispose()
        {
        }
    }
}