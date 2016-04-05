using FacebookPy.Enums;
using FacebookPy.EventsArgs;
using FacebookPy.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Web;

namespace FacebookPy
{
    public class FbConversation : IDisposable
    {
        private FbRequest _fbRequest;
        private Random rnd = new Random((new Random(50)).Next(150));
        private bool _listening;
        private List<string> _randomValueList = new List<string>();
        private int count = 1;
        private bool _listeningComplete = true;

        internal event EventHandler<MessageEventArgs> IncomingMessage;

        internal FbConversation(FbRequest fbRequest)
        {
            _fbRequest = fbRequest;
            _listening = false;
        }

        internal bool SendMessage(string recipientId, string message = "", ChatLikes like = ChatLikes.None)
        {
            if (!_fbRequest.Connected)
            {
                return false;
            }
            var timestamp = CommonData.Now().ToString();
            NameValueCollection nvc = new NameValueCollection();
            nvc["client"] = _fbRequest.DataBag.Client;
            nvc["message_batch[0][action_type]"] = "ma-type:user-generated-message";
            nvc["message_batch[0][author]"] = string.Format("fbid:{0}", _fbRequest.DataBag.Uid);
            nvc["message_batch[0][specific_to_list][0]"] = string.Format("fbid:{0}", recipientId);
            nvc["message_batch[0][specific_to_list][1]"] = string.Format("fbid:{0}", _fbRequest.DataBag.Uid);
            nvc["message_batch[0][timestamp]"] = timestamp;
            nvc["message_batch[0][timestamp_absolute]"] = "Today";
            nvc["message_batch[0][timestamp_relative]"] = string.Format("{0}:{1}", DateTime.Now.Hour, DateTime.Now.Minute);
            nvc["message_batch[0][timestamp_time_passed]"] = "0";
            nvc["message_batch[0][is_unread]"] = "True";
            nvc["message_batch[0][is_cleared]"] = "False";
            nvc["message_batch[0][is_forward]"] = "True";
            nvc["message_batch[0][is_filtered_content]"] = "False";
            nvc["message_batch[0][is_spoof_warning]"] = "False";
            nvc["message_batch[0][source]"] = "source:chat:web";
            nvc["message_batch[0][source_tags][0]"] = "source:chat";
            nvc["message_batch[0][body]"] = HttpUtility.UrlEncode(message);
            nvc["message_batch[0][html_body]"] = "False";
            nvc["message_batch[0][ui_push_phase]"] = "V3";
            nvc["message_batch[0][status]"] = "0";
            nvc["message_batch[0][message_id]"] = GenerateMessageId(_fbRequest.DataBag.ClientId);
            nvc["message_batch[0][manual_retry_cnt]"] = "0";
            nvc["message_batch[0][thread_fbid]"] = recipientId;
            nvc["message_batch[0][has_attachment]"] = "False";

            if (like != ChatLikes.None)
            {
                string likeCode = string.Empty;
                switch (like)
                {
                    case ChatLikes.Small:
                        likeCode = "369239263222822";
                        break;

                    case ChatLikes.Medium:
                        likeCode = "369239343222814";
                        break;

                    case ChatLikes.Large:
                        likeCode = "369239383222810";
                        break;
                }
                nvc["message_batch[0][sticker_id]"] = likeCode;
            }

            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            try
            {
                string resJson = _fbRequest.GetJsonFromPostMethod(FbEndpoints.SendURL, nvc, out statusCode);
                FbMessageExceptionData exceptionData = JsonConvert.DeserializeObject<FbMessageExceptionData>(resJson);
                if (exceptionData != null && exceptionData.HasError)
                {
                    throw new FbMessageException(exceptionData);
                }
            }
            catch (FbMessageException fbEx)
            {
                throw fbEx;
            }
            catch (Exception ex)
            {
                if (_fbRequest != null)
                {
                    _fbRequest.InvokeRequestErrorEvent(this, new FbErrorsEventArgs(ex));
                }
            }

            return statusCode == HttpStatusCode.OK;
        }

        internal string GetUnread()
        {
            HttpStatusCode statusCode;
            NameValueCollection nvc = new NameValueCollection();
            nvc["client"] = "mercury_sync";
            nvc["folders[0]"] = "inbox";
            nvc["last_action_timestamp"] = (CommonData.Now() - 60000).ToString();

            string resJson = _fbRequest.GetJsonFromPostMethod(FbEndpoints.ThreadSyncURL, nvc, out statusCode);

            return string.Empty;
        }

        internal void RunListeningIncomingMessages()
        {
            _prev = CommonData.Now() / 1000;
            Action listen = delegate
            {
                RunListening(null);
            };

            if (_listeningComplete)
            {
                _listening = true;
                listen.BeginInvoke(CallbackListenning, listen);
            }
        }

        internal void ShutDownListeningIncomingMessages()
        {
            _listening = false;
        }

        internal void LogOut()
        {
            _listening = false;
        }

        private void ParseIncomingMessage(string content)
        {
            FbJsonMessageContainer messageEnvelop = JsonConvert.DeserializeObject<FbJsonMessageContainer>(content);
            _fbRequest.DataBag.Seq = messageEnvelop.seq;
            if (messageEnvelop.ms != null)
                foreach (FacebookPy.FbJsonMessageContainer.FbMessageEnvelop envelop in messageEnvelop.ms)
                {
                    switch (envelop.type)
                    {
                        case "messaging":
                            {
                                if (envelop.Message != null && envelop.Message.Is_unread)
                                    OnMessage(envelop.Message);
                            }
                            break;

                        case "typ":
                            {
                            }
                            break;

                        case "m_read_receipt":
                            {
                            }
                            break;

                        case "buddylist_overlay":
                            {
                            }
                            break;

                        default:
                            string a = envelop.type;
                            break;
                    }
                }
        }

        private void OnMessage(FbMessage fbMessage)
        {
            if (IncomingMessage != null)
            {
                IncomingMessage(this, new MessageEventArgs(fbMessage));
            }
        }

        private void MarkAsRead(long authorId)
        {
            HttpStatusCode statusCode;
            NameValueCollection nvc = new NameValueCollection();
            nvc["watermarkTimestamp"] = CommonData.Now().ToString();
            nvc["shouldSendReadReceipt"] = "False";
            nvc[string.Format("ids[{0}]", authorId)] = "True";

            string resJson = _fbRequest.GetJsonFromPostMethod(FbEndpoints.ReadStatusURL, nvc, out statusCode);
            bool result = statusCode == HttpStatusCode.OK;
        }

        private bool MarkAsDelivered(string messageId, long authorId)
        {
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            NameValueCollection nvc = new NameValueCollection();
            nvc["message_ids[0]"] = messageId;
            nvc[string.Format("thread_ids[{0}][0]", authorId)] = messageId;
            string resJson = _fbRequest.GetJsonFromPostMethod(FbEndpoints.DeliveredURL, nvc, out statusCode);

            return statusCode == HttpStatusCode.OK;
        }

        private long _prev = 0;

        private string PullMessage(string sticky, string pool)
        {
            string result = string.Empty;
            NameValueCollection parametersColl = new NameValueCollection();
            parametersColl.Add("msgs_recv", "0");
            parametersColl.Add("sticky_token", sticky);
            parametersColl.Add("sticky_pool", pool);
            parametersColl.Add("state", "active");
            parametersColl.Add("idle", ((CommonData.Now() / 1000) - _prev).ToString());
            _prev = CommonData.Now() / 1000;
            _fbRequest.UpdatePresenceCookie();

            result = _fbRequest.GetTextResultFromGetMethod(FbEndpoints.StickyURL, parametersColl);
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Remove(0, result.IndexOf('{'));
            }

            return result;
        }

        private string GenerateMessageId(string clientId)
        {
            var nowTimestamp = CommonData.Now();
            string randomValue = (rnd.Next(150) * 4294967295 + count * 1000).ToString();
            if (randomValue.Length > 11)
                randomValue = randomValue.Remove(11);
            if (_randomValueList.Contains(randomValue))
            {
                return GenerateMessageId(clientId);
            }
            else
            {
                _randomValueList.Add(randomValue);
            }

            count++;
            return string.Format("<{0}:{1}-{2}@mail.projektitan.com>", nowTimestamp, randomValue, clientId);
        }

        private void OnTyping(dynamic fbid)
        {
        }

        private void CallbackListenning(IAsyncResult ar)
        {
            _listeningComplete = true;
        }

        private void RunListening(object o)
        {
            _listeningComplete = false;
            while (_listening)
            {
                try
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (_fbRequest.Connected)
                        {
                            Thread.Sleep(100);
                            string content = PullMessage(_fbRequest.DataBag.Sticky, _fbRequest.DataBag.Pool);
                            if (!string.IsNullOrEmpty(content))
                            {
                                ParseIncomingMessage(content);
                            }
                        }
                    }
                }
                catch (System.Net.WebException exept)
                {
                    if (exept.Status == WebExceptionStatus.Timeout)
                    {
                        Thread.Sleep(3000);
                    }
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

        public void Dispose()
        {
            ShutDownListeningIncomingMessages();
        }
    }
}