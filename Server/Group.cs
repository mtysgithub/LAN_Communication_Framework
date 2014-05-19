using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using Mty.LCF.Server.ServerClient;
using ProtocolLibrary.CSProtocol.CommonConfig.ClientMsgCodecSpace;
using ProtocolLibrary.CSProtocol;

namespace Mty.LCF.Server
{
    public class Group
    {
        public Group(string groupToken, string note)
        {
            this._token = groupToken;
            this._note = note;
        }
        public void Disable()
        {
            //notfiy
            // N/A

            //release
            // N/A
            _memberClientList.Clear();
            _tokenClientMap.Clear();
            _listenerBuff.Clear();
            _radiorBuff.Clear();
        }

        public AppErrorInfo Regist(BaseServerClient client, byte listOrRadioCodec, out byte oprFaildCodec)
        {
            if (_tokenClientMap.ContainsKey(client.Token))
            {
                oprFaildCodec = (byte)JoinGroupFaildCodec.Client_REPEAT;
                return AppErrorInfo.APP_FAILD;
            }
            __AddClient(client.Token, listOrRadioCodec, client);
            oprFaildCodec = (byte)ClientHeadCodec.JoinGroup_ReplySucess;
            return AppErrorInfo.APP_SUCESS;
        }
        public AppErrorInfo Cancle(BaseServerClient client, out byte oprFaildCodec)
        {
            if (false == _tokenClientMap.ContainsValue(client))
            {
                oprFaildCodec = (byte)ExitGroupFaildCodec.Client_UNBELONG;
                return AppErrorInfo.APP_FAILD;
            }

            //将应答操作延迟到ServerClient获取执行结果后，增强内聚
            //__SendCancleMessage(client);

            __RemoveClient(client);
            oprFaildCodec = (byte)ClientHeadCodec.ExitGroup_ReplySucess;
            return AppErrorInfo.APP_SUCESS;
        }
        public AppErrorInfo CancleAll()
        {
            AppErrorInfo ret = AppErrorInfo.APP_SUCESS;
            foreach(BaseServerClient client in _memberClientList)
            {
                byte tmpOprCodec;
                ret |= this.Cancle(client, out tmpOprCodec);
            }
            return ret;
        }
        public AppErrorInfo SendRadioMessage(BaseServerClient srcClient, byte[] content, out byte oprFaildCodec)
        {
            if (_tokenClientMap.ContainsKey(srcClient.Token))
            {
                if (_radiorBuff.Contains(srcClient))
                {
                    Server server = srcClient.Server;
                    foreach(BaseServerClient dstClient in _listenerBuff)
                    {
                        if (false == dstClient.Token.Equals(srcClient.Token))
                        {
                            byte tmpOprFaildCodec;
                            string dstToken = dstClient.Token;
                            //AppErrorInfo p2pSendRet = server.Send(srcClient.Token, dstToken, content, out tmpOprFaildCodec);
                            AppErrorInfo p2pSendRet = server.GroupSend(srcClient.Token, this.Token, dstToken, content, out tmpOprFaildCodec);
                            if (AppErrorInfo.APP_SUCESS != p2pSendRet)
                            {
                                //这里进行的是无责任分发，故不处理目标客户端的错误
                                //Try Log
                            }
                        }
                    }
                    oprFaildCodec = (byte)ClientHeadCodec.RadioTransport_ReplySucess;
                    return AppErrorInfo.APP_SUCESS;
                }else
                {
                    oprFaildCodec = (byte)RadioTransportFaildCodec.Trans_FORBIDDEN;
                    return AppErrorInfo.APP_FAILD;
                }
            }
            else
            {
                oprFaildCodec = (byte)RadioTransportFaildCodec.Client_UNBELONG;
                return AppErrorInfo.APP_FAILD;
            }
        }

        public AppErrorInfo GetOnlineClient(string invisorToken,  out byte oprFaildCodec, out List<CSCommunicateClass.ClientInfo> list)
        {
            if (_tokenClientMap.ContainsKey(invisorToken))
            {
                list = new List<CSCommunicateClass.ClientInfo>();
                foreach(BaseServerClient client in _memberClientList)
                {
                    list.Add(new CSCommunicateClass.ClientInfo(client.Token, client.Detail));
                }
                oprFaildCodec = (byte)ClientHeadCodec.GetGroupClient_ReplySucess;
                return AppErrorInfo.APP_SUCESS;
            }
            else
            {
                oprFaildCodec = (byte)GetGoupClientFaildCodec.Client_UNBELONG;
                list = null;
                return AppErrorInfo.APP_FAILD;
            }
        }

        public bool ContainClient(BaseServerClient client)
        {
            if (_memberClientList.Contains(client))
            {
                return true;
            } 
            else
            {
                return false;
            }
        }

        private AppErrorInfo __AddClient(string token, byte listOrRadioCodec, BaseServerClient client)
        {
            if (_tokenClientMap.ContainsKey(token))
            {
                return AppErrorInfo.APP_FAILD;
            }
            _tokenClientMap.Add(token, client);
            _memberClientList.Add(client);
            if (((listOrRadioCodec >> 1) & 1) == 1)
            {
                _radiorBuff.Add(client);
            }
            if (((listOrRadioCodec >> 0) & 1) == 1)
            {
                _listenerBuff.Add(client);                                
            }
            return AppErrorInfo.APP_SUCESS;
        }
        private AppErrorInfo __RemoveClient(BaseServerClient client)
        {
            if (false == _tokenClientMap.ContainsKey(client.Token))
            {
                return AppErrorInfo.APP_FAILD;
            }

            _memberClientList.Remove(client);
            _tokenClientMap.Remove(client.Token);

            if (_radiorBuff.Contains(client))
            {
                _radiorBuff.Remove(client);
            }
            if (_listenerBuff.Contains(client))
            {
                _listenerBuff.Remove(client);
            }
            return AppErrorInfo.APP_SUCESS;
        }

        public int MemberCount { get { return _memberClientList.Count; } }
        public string Token { get { return _token; } }
        public string Detail { get { return _note; } }
        public List<BaseServerClient> MemberClientList
        {
            get { return _memberClientList; }
        }

        private string _token = string.Empty;
        private string _note = string.Empty;
        private List<BaseServerClient> _memberClientList = new List<BaseServerClient>();
        private Hashtable _tokenClientMap = new Hashtable();
        private List<BaseServerClient> _listenerBuff = new List<BaseServerClient>();
        private List<BaseServerClient> _radiorBuff = new List<BaseServerClient>();
    }
}
