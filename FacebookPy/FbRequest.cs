using FacebookPy.EventsArgs;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace FacebookPy
{
    internal class FbRequest
    {
        private CookieContainer _cookieContainer;
        private string _userAgent;
        private const CookieContainer _defaultNullCookies = null;
        private NameValueCollection _payloadDefault;
        private FbContext _fbContext;
        private Random _rnd = new Random();
        private NameValueCollection _matchesDictionary;
        private CommonData _sharedAccountData;

        internal event EventHandler<FbErrorsEventArgs> ErrorOccurs;

        internal CookieContainer CurrentCookieContainer
        {
            get { return _cookieContainer; }
        }

        internal FbRequest(FbContext fbContext)
        {
            _fbContext = fbContext;
            _cookieContainer = new CookieContainer();
            _userAgent = UserAgents.UserAgentRamdom;
            _payloadDefault = new NameValueCollection();
            _sharedAccountData = new CommonData();
            FillMatchesDictionary();
        }

        internal void UpdateSticky()
        {
            if (Connected)
            {
                try
                {
                    NameValueCollection nvc = new NameValueCollection();
                    nvc["msgs_recv"] = "0";

                    string resJson = GetTextResultFromGetMethod(FbEndpoints.StickyURL, nvc);

                    resJson = resJson.Remove(0, resJson.IndexOf('{'));
                    FbStickyKeeper stickyKeeper = JsonConvert.DeserializeObject<FbStickyKeeper>(resJson);
                    if (stickyKeeper != null && stickyKeeper.FacebookSticky != null)
                    {
                        DataBag.Sticky = stickyKeeper.FacebookSticky.Sticky;
                        DataBag.Pool = stickyKeeper.FacebookSticky.Pool;
                    }
                }
                catch (Exception ex)
                {
                    InvokeRequestErrorEvent(this, new FbErrorsEventArgs(ex));
                }
            }
        }

        internal CommonData DataBag
        {
            get { return _sharedAccountData; }
        }

        internal bool Connected
        {
            get { return _fbContext != null ? _fbContext.Connected : false; }
        }

        private void UpdateCookies(CookieCollection cookieCollection)
        {
            _cookieContainer.Add(cookieCollection);
        }

        internal string GetJsonFromPostMethod(string URI, NameValueCollection data, out HttpStatusCode statusCode)
        {
            string resJson = string.Empty;
            HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest;
            try
            {
                using (HttpWebResponse response = PostMethod(URI, data))
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    string readText = reader.ReadToEnd();
                    resJson = readText.Remove(0, readText.IndexOf('{'));
                    httpStatusCode = response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                InvokeRequestErrorEvent(this, new FbErrorsEventArgs(ex, httpStatusCode));
            }
            statusCode = httpStatusCode;
            return resJson;
        }

        internal void InvokeRequestErrorEvent(object sender, FbErrorsEventArgs args)
        {
            EventHandler<FbErrorsEventArgs> handler = ErrorOccurs;
            if (handler != null)
            {
                handler(sender, args);
            }
        }

        internal HttpWebResponse CleanPostMethod(string URI, NameValueCollection payload)
        {
            DataBag.RequestCounter++;
            Uri uri = new Uri(URI);
            HttpWebRequest request = GetCurrentSessionRequest(uri, "POST", _cookieContainer);
            string data = ConvertPayloadToString(payload);
            HttpWebResponse response = (HttpWebResponse)AddParamToPostRequest(request, data).GetResponse();
            _cookieContainer.Add(response.Cookies);
            return response;
        }

        private static object _padlockGetMethod = new object();

        internal HttpWebResponse GetMethod(string uri, NameValueCollection query = null)
        {
            lock (_padlockGetMethod)
            {
                NameValueCollection payload = GeneratePayload(query);
                string startParam = ConvertPayloadToString(payload);
                Uri encodinUriWithParameters = GetEncodingUriWithParameters(uri, startParam);
                HttpWebRequest request = GetCurrentSessionRequest(encodinUriWithParameters, "GET");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return response;
            }
        }

        //  private static object _padlock = new object();
        internal string GetTextResultFromGetMethod(string uri, NameValueCollection query = null, bool needUpdateCookie = false)
        {
            string result = string.Empty;
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            try
            {
                using (HttpWebResponse response = GetMethod(uri, query))
                using (Stream stream = response.GetResponseStream())
                using (TextReader sr = new StreamReader(stream))
                {
                    statusCode = response.StatusCode;
                    result = sr.ReadToEnd();
                    if (needUpdateCookie)
                    {
                        UpdateCookies(response.Cookies);
                    }
                }
            }
            catch (System.Net.WebException we)
            {
                if (we.Status != WebExceptionStatus.Timeout)
                {
                    InvokeRequestErrorEvent(this, new FbErrorsEventArgs(we, statusCode) { StatusDescription = uri });
                }
            }
            catch (Exception ex)
            {
                InvokeRequestErrorEvent(this, new FbErrorsEventArgs(ex, statusCode) { StatusDescription = uri });
            }
            return result;
        }

        internal void UpdatePayloadDefault(string readText, string uid, string ttstamp, string fbDtsg)
        {
            string revisionValue = Regex.Match(readText, @"""revision"":([^,]*),").Groups[1].Value;

            _payloadDefault.Add("__rev", revisionValue);
            _payloadDefault.Add("__user", uid);
            _payloadDefault.Add("__a", "1");
            _payloadDefault.Add("ttstamp", ttstamp);
            _payloadDefault.Add("fb_dtsg", fbDtsg);
        }

        internal void UpdatePresenceCookie()
        {
            string presenceCookieValue = GeneratePresence(DataBag.Uid);
            string accessibilityCookie = GenerateAccessibilityCookie();
            Cookie cookieAccess = new Cookie("a11y", accessibilityCookie, "/", ".facebook.com");
            Cookie cookiePresence = new Cookie("presence", presenceCookieValue, "/", ".facebook.com");
            CookieCollection coll = new CookieCollection();
            coll.Add(cookiePresence);
            coll.Add(cookieAccess);
            UpdateCookies(coll);
        }

        private string GetCookieValue(string p)
        {
            string result = _cookieContainer.GetCookies(new Uri("facebook.com"))[p].ToString();
            return result;
        }

        private HttpWebResponse PostMethod(string URI, NameValueCollection data)
        {
            Uri uri = new Uri(URI);
            NameValueCollection payload = GeneratePayload(data);
            string resultData = ConvertPayloadToString(payload);
            HttpWebRequest request = GetCurrentSessionRequest(uri, "POST", _defaultNullCookies);
            HttpWebResponse response = (HttpWebResponse)AddParamToPostRequest(request, resultData).GetResponse();
            _cookieContainer.Add(response.Cookies);
            return response;
        }

        private HttpWebRequest GetCurrentSessionRequest(Uri uri, string method, CookieContainer cookieContainer = _defaultNullCookies)
        {
            HttpWebRequest _get = (HttpWebRequest)WebRequest.Create(uri);
            _get.Timeout = 10000;
            _get.ContentType = "application/x-www-form-urlencoded";
            _get.Referer = FbEndpoints.BaseURL;
            _get.Headers.Add("Origin", FbEndpoints.BaseURL);
            _get.UserAgent = this._userAgent;
            _get.KeepAlive = true;

            _get.Method = method;
            if (cookieContainer != null)
                _get.CookieContainer = cookieContainer;
            else
            {
                _get.CookieContainer = _cookieContainer;
            }

            return _get;
        }

        private NameValueCollection GeneratePayload(NameValueCollection pl)
        {
            NameValueCollection nvc = new NameValueCollection(_payloadDefault);
            if (pl != null && pl.Count > 0)
            {
                nvc.Add(pl);
            }

            nvc["__req"] = StrBase(DataBag.RequestCounter, 36);
            nvc["seq"] = DataBag.Seq;

            DataBag.RequestCounter++;
            return nvc;
        }

        private HttpWebRequest AddParamToPostRequest(HttpWebRequest request, string parameters)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(parameters);
            request.ContentLength = bytes.Length;
            try
            {
                Stream os = request.GetRequestStream();
                os.Write(bytes, 0, bytes.Length);
                os.Close();
            }
            catch (Exception ex)
            {
                InvokeRequestErrorEvent(this, new FbErrorsEventArgs(ex));
            }
            return request;
        }

        private string ConvertPayloadToString(NameValueCollection payloadCollection)
        {
            string result = string.Empty;
            if (payloadCollection != null && payloadCollection.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var key in payloadCollection.Keys)
                {
                    sb.Append(string.Format("{0}={1}&", key, payloadCollection.Get((string)key)));
                }
                result = sb.ToString();
                if (result.LastIndexOf('&') == result.Length - 1)
                    result = string.Format("{0}", result.ToString().Remove(result.LastIndexOf('&')));
            }

            return result;
        }

        private string StrBase(int number, int baseN)
        {
            if (number < 0)
            {
                return StrBase(number * -1, baseN);
            }
            int d = number / baseN;
            int m = number % baseN;
            if (d > 0)
            {
                string resul = string.Format("{0}{1}", StrBase(d, baseN), DigitToChar(m));
                return resul;
            }

            int o = 2 % 36;
            string f = DigitToChar(m).ToString();
            return f;
        }

        private string DigitToChar(int digit)
        {
            if (digit < 10)
            {
                var ch = Convert.ToString(digit);
                return ch;
            }
            int res = Convert.ToInt32(97 + digit - 10);
            char chRes = Convert.ToChar(res);
            return chRes.ToString();
        }

        private Uri GetEncodingUriWithParameters(string uri, string parameters)
        {
            string res = string.Format("{0}?{1}", uri, parameters);
            string resEncoded = HttpUtility.UrlEncode(res, Encoding.Default);

            return new Uri(res);
        }

        private void FillMatchesDictionary()
        {
            _matchesDictionary = new NameValueCollection();
            _matchesDictionary.Add("&", "_");
            _matchesDictionary.Add("%2", "A");
            _matchesDictionary.Add("000", "B");
            _matchesDictionary.Add("%7d", "C");
            _matchesDictionary.Add("%7b%22", "D");
            _matchesDictionary.Add("%2c%22", "E");
            _matchesDictionary.Add("%22%3a", "F");
            _matchesDictionary.Add("%2c%22ut%22%3a1", "G");
            _matchesDictionary.Add("%2c%22bls%22%3a", "H");
            _matchesDictionary.Add("%2c%22n%22%3a%22%", "I");
            _matchesDictionary.Add("%22%3a%7b%22i%22%3a0%7d", "J");
            _matchesDictionary.Add("%2c%22pt%22%3a0%2c%22vis%22%3a", "K");
            _matchesDictionary.Add("%2c%22ch%22%3a%7b%22h%22%3a%22", "L");
            _matchesDictionary.Add("%7b%22v%22%3a2%2c%22time%22%3a1", "M");
            _matchesDictionary.Add(".channel%22%2c%22sub%22%3a%5b", "N");
            _matchesDictionary.Add("%2c%22sb%22%3a1%2c%22t%22%3a%5b", "O");
            _matchesDictionary.Add("%2c%22ud%22%3a100%2c%22lc%22%3a0", "P");
            _matchesDictionary.Add("%5d%2c%22f%22%3anull%2c%22uct%22%3a", "Q");
            _matchesDictionary.Add(".channel%22%2c%22sub%22%3a%5b1%5d", "R");
            _matchesDictionary.Add("%22%2c%22m%22%3a0%7d%2c%7b%22i%22%3a", "S");
            _matchesDictionary.Add("%2c%22blc%22%3a1%2c%22snd%22%3a1%2c%22ct%22%3a", "T");
            _matchesDictionary.Add("%2c%22blc%22%3a0%2c%22snd%22%3a1%2c%22ct%22%3a", "U");
            _matchesDictionary.Add("%2c%22blc%22%3a0%2c%22snd%22%3a0%2c%22ct%22%3a", "V");
            _matchesDictionary.Add("%2c%22s%22%3a0%2c%22blo%22%3a0%7d%2c%22bl%22%3a%7b%22ac%22%3a", "W");
            _matchesDictionary.Add("%2c%22ri%22%3a0%7d%2c%22state%22%3a%7b%22p%22%3a0%2c%22ut%22%3a1", "X");
            _matchesDictionary.Add("%2c%22pt%22%3a0%2c%22vis%22%3a1%2c%22bls%22%3a0%2c%22blc%22%3a0%2c%22snd%22%3a1%2c%22ct%22%3a", "Y");
            _matchesDictionary.Add("%2c%22sb%22%3a1%2c%22t%22%3a%5b%5d%2c%22f%22%3anull%2c%22uct%22%3a0%2c%22s%22%3a0%2c%22blo%22%3a0%7d%2c%22bl%22%3a%7b%22ac%22%3a", "Z");
        }

        private string GeneratePresence(string userId)
        {
            var timestamp = CommonData.Now();
            long randedValue = (long)Math.Round(Math.Floor(_rnd.NextDouble() * 4294967295) + 1);
            string p_userId = string.Format("p_{0}", userId);
            string jsonRes = string.Format(@"{{""v"":3,""time"":{0},""user"":{1},""state"":{{""ut"":0,""t2"":[],""lm2"": null,""uct2"":{2},""tr"":null,""tw"":{3},""at"":{2}}},""ch"":{{""{4}"":0}} }}", timestamp / 1000, userId, timestamp, randedValue, p_userId);
            var encoded = PresenceEncode(HttpUtility.UrlEncode(jsonRes));

            return encoded;
        }

        private string PresenceEncode(string incomingString)
        {
            foreach (string match in SortStringByDescending(_matchesDictionary.Keys))
            {
                incomingString = incomingString.Replace(match, _matchesDictionary[match]);
            }
            return incomingString;
        }

        private IEnumerable<string> SortStringByDescending(ICollection keys)
        {
            List<string> names = keys.Cast<string>().ToList();
            var sorted = from key in names orderby key.Length descending select key;
            return sorted;
        }

        private string GenerateAccessibilityCookie()
        {
            var timestamp = CommonData.Now();
            string jsonRes = string.Format(@"{{""sr"":0,""sr-ts"":{0},""jk"":0,""jk-ts"":{0},""kb"":0,""kb-ts"":{0},""hcm"":0,""hcm-ts"":{0}}}", timestamp);
            return HttpUtility.UrlEncode(jsonRes);
        }
    }
}