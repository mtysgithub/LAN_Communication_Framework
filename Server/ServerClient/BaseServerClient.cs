using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Web.Script;
using System.Web.Script.Serialization;

using Hik.Communication.Scs.Server;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication;

using ProtocolLibrary;
using ProtocolLibrary.CSProtocol.CommonConfig;
using ProtocolLibrary.CSProtocol;
using ProtocolLibrary.CSProtocol.CommonConfig.ClientMsgCodecSpace;
using ProtocolLibrary.JsonSerializtion;

namespace NIPlayRoomNetServer.ServerClient
{
    public class BaseServerClient
    {
        public BaseServerClient(string token, IScsServerClient client, Server server, string detail = "")
        {
            _token = token;
            _detail = detail;
            _client = client;
            _server = server;

            _client.MessageReceived += this.Client_MessageRecevied;
            _client.Disconnected += this.Client_DisconnectHadnle;
        }

        /// <summary>
        /// 该函数从上行报文中解析出需要服务器下一步进行的动作
        /// </summary>
        /// <param name="sourMsg">上行报文</param>
        /// <param name="oprCodec">操作码</param>
        /// <param name="parmMap">参数表</param>
        protected void ServerOprInfoFilter(byte [] sourMsg, out byte oprCodec, out Hashtable parmMap)
        {
            parmMap = new Hashtable();

            //opr codec
            int pSourReadOffset = 0;
            oprCodec = Convert.ToByte(sourMsg[pSourReadOffset]);
            ++pSourReadOffset;

            switch (oprCodec)
            {
                case (int)ServerOprCodec.P2pTransport:
                    {
                        ++pSourReadOffset;
                        byte[] tmpBuf = new byte[ConstData.tokenLength];
                        Buffer.BlockCopy(sourMsg, pSourReadOffset, tmpBuf, 0, ConstData.tokenLength);
                        string token = Encoding.ASCII.GetString(tmpBuf);
                        pSourReadOffset += ConstData.tokenLength;

                        //Parse 分发 正文
                        byte[] content = new byte[sourMsg.Length - pSourReadOffset];
                        Buffer.BlockCopy(sourMsg, pSourReadOffset, content, 0, content.Length);
                        pSourReadOffset += content.Length;

                        //push parms
                        parmMap.Add("Token", token);
                        parmMap.Add("Content", content);
                        break;
                    }
                case (int)ServerOprCodec.CreateGroup:
                        {
                            if (sourMsg.Length > 1)
                            {
                                byte[] bytGropDetail = new byte[sourMsg.Length - 1];
                                Buffer.BlockCopy(sourMsg, 1, bytGropDetail, 0, bytGropDetail.Length);
                                string strGropDetail = Encoding.UTF8.GetString(bytGropDetail);
                                parmMap.Add("GroupDetail", strGropDetail);
                            }
                            else
                            {
                                parmMap.Add("GroupDetail", string.Empty);
                            }
                            break;
                        }
                case (int)ServerOprCodec.JoinGroup:
                        {
                            byte listOrRadioFlag = sourMsg[2];
                            byte [] bytToken = new byte [ConstData.tokenLength];
                            Buffer.BlockCopy(sourMsg, 2 + 1, bytToken, 0, ConstData.tokenLength);
                            string token = Encoding.ASCII.GetString(bytToken);

                            parmMap.Add("Token", token);
                            parmMap.Add("ClientState", listOrRadioFlag);
                            break;
                        }
                case (int)ServerOprCodec.ExitGroup:
                        {
                                byte [] bytToken = new byte [ConstData.tokenLength];
                                Buffer.BlockCopy(sourMsg, 2, bytToken, 0, ConstData.tokenLength);
                                string token = Encoding.ASCII.GetString(bytToken);
                                parmMap.Add("Token", token);
                            break;
                        }
                case (int)ServerOprCodec.RadioTransport:
                        {
                            List<string> tokens = new List<string>();
                            int tokensCount = Convert.ToInt32(sourMsg[1]);
                            for (int i = 0; i < tokensCount; ++i )
                            {
                                byte [] bytToken = new byte [ConstData.tokenLength];
                                Buffer.BlockCopy(sourMsg, 2 + i * ConstData.tokenLength, bytToken, 0, ConstData.tokenLength);
                                string token = Encoding.ASCII.GetString(bytToken);
                                tokens.Add(token);
                            }
                            parmMap.Add("Tokens", tokens);

                            byte [] content = new byte [sourMsg.Length - (2 + tokensCount * ConstData.tokenLength)];
                            Buffer.BlockCopy(sourMsg, 2 + (tokensCount * ConstData.tokenLength), content, 0, content.Length);
                            parmMap.Add("Content", content);

                            break;
                        }
                case (int)ServerOprCodec.GroupOnlineList:
                        {
                            // N/A
                            break;
                        }
                case (int)ServerOprCodec.GroupClientOnlineList:
                        {
                            byte [] bytToken = new byte [ConstData.tokenLength];
                            Buffer.BlockCopy(sourMsg, 1, bytToken, 0, ConstData.tokenLength);
                            string token = Encoding.ASCII.GetString(bytToken);
                            parmMap.Add("Token", token);
                            break;
                        }
                case (int)ServerOprCodec.ClientCancle:
                        {
                            // N/A
                            break;
                        }
                    default:
                    {
                        //throw new NotImplementedException();
                        break;
                    }
            }
        }
        protected virtual void Client_MessageRecevied(object sender, MessageEventArgs e)
        {
            string sourMessageid = e.Message.MessageId;
            byte oprCodec;
            Hashtable parmMap;
            ServerOprInfoFilter(((ScsRawDataMessage)e.Message).MessageData, out oprCodec, out parmMap);
            switch (oprCodec)
            {
                case (int)ServerOprCodec.P2pTransport:
                    {
                        byte oprFaildCodec;
                        AppErrorInfo ret = _server.Send(_token, parmMap["Token"] as string, parmMap["Content"] as byte[], out oprFaildCodec);
                        if (AppErrorInfo.APP_SUCESS == ret)
                        {
                            //Reply Suc
                            byte[] sucReply = new byte[1];
                            sucReply[0] = (byte)ClientHeadCodec.P2pTransport_ReplySucess;
                            _client.SendMessage(new ScsRawDataMessage(sucReply, sourMessageid));

                            string logInfo = "{" 
                                                + "ret:" + "P2pTransport => APP_SUCESS" + " , "
                                                + "info:" 
                                                    + "{"
                                                        + "from_detail:" + this.Detail
                                                        + "from_token:" + this.Token + ","
                                                        + "to_token:" + parmMap["Token"]
                                                    + "}" 
                                                + "}";
                            _server.__AddLogItem(logInfo);
                        }
                        else
                        {
                            switch (oprFaildCodec)
                            {
                                case (byte)P2pTransportFaildCodec.Target_UNCONNECT:
                                    {
                                        //向源头打回错误报文
                                        byte[] erroReply = new byte[2];
                                        erroReply[0] = (byte)ClientHeadCodec.P2pTransport_ReplyFaild;
                                        erroReply[1] = (byte)P2pTransportFaildCodec.Target_UNCONNECT;
                                        _client.SendMessage(new ScsRawDataMessage(erroReply, sourMessageid));

                                        string logInfo = "{"
                                                            + "ret:" + "P2pTransport => Target_UNCONNECT" + " , "
                                                            + "info:"
                                                                + "{"
                                                                    + "from_detail:" + this.Detail
                                                                    + "from_token:" + this.Token + ","
                                                                    + "to_token:" + parmMap["Token"]
                                                                + "}"
                                                            + "}";
                                        _server.__AddLogItem(logInfo);
                                        break;
                                    }
                                case (byte)P2pTransportFaildCodec.Token_UNPARSE:
                                    {
                                        //向源头打回错误报文
                                        byte[] erroReply = new byte[2];
                                        erroReply[0] = (byte)ClientHeadCodec.P2pTransport_ReplyFaild;
                                        erroReply[1] = (byte)P2pTransportFaildCodec.Token_UNPARSE;
                                        _client.SendMessage(new ScsRawDataMessage(erroReply, sourMessageid));

                                        string logInfo = "{"
                                                            + "ret:" + "P2pTransport => Token_UNPARSE" + " , "
                                                            + "info:"
                                                                + "{"
                                                                    + "from_detail:" + this.Detail
                                                                    + "from_token:" + this.Token + ","
                                                                    + "to_token:" + parmMap["Token"]
                                                                + "}"
                                                            + "}";
                                        _server.__AddLogItem(logInfo);
                                        break;
                                    }
                                    default:
                                    {
                                        string logInfo = "{"
                                                            + "ret:" + "P2pTransport => default:错误码分支未实现" + " , "
                                                            + "info:"
                                                                + "{"
                                                                    + "from_detail:" + this.Detail
                                                                    + "from_token:" + this.Token + ","
                                                                    + "to_token:" + parmMap["Token"]
                                                                + "}"
                                                            + "}";
                                        _server.__AddLogItem(logInfo);
                                        return;
                                    }
                            }
                        }
                        break;
                    }
                case (int)ServerOprCodec.CreateGroup:
                    {
                        string gropDetail = parmMap["GroupDetail"] as string;
                        if (false == string.IsNullOrEmpty(gropDetail))
                        {
                            string token;
                            byte oprFaildCodec;
                            AppErrorInfo ret = _server.CreateGroup(gropDetail, out token, out oprFaildCodec);
                            if (AppErrorInfo.APP_SUCESS == ret)
                            {
                                byte [] sucReply = new byte[1 + ConstData.tokenLength];
                                sucReply[0] = (byte)ClientHeadCodec.CreateGroup_ReplySucess;
                                Buffer.BlockCopy(Encoding.ASCII.GetBytes(token), 0, sucReply, 1, ConstData.tokenLength);
                                _client.SendMessage(new ScsRawDataMessage(sucReply, sourMessageid));

                                string logInfo = "{"
                                                    + "ret:" + "CreateGroup => APP_SUCESS" + " , "
                                                    + "info:"
                                                        + "{"
                                                            + "from_detail:" + this.Detail
                                                            + "from_token:" + this.Token + ","
                                                            + "group_detail:" + gropDetail
                                                            + "group_token:" + token
                                                        + "}"
                                                    + "}";
                                _server.__AddLogItem(logInfo);
                            }
                            else
                            {
                                switch (oprFaildCodec)
                                {
                                    case (byte)CreateGroupFaildCodec.Detail_REPEAT: 
                                        {
                                            byte[] errorReply = new byte[2];
                                            errorReply[0] = (byte)ClientHeadCodec.CreateGroup_ReplyFaild;
                                            errorReply[1] = (byte)CreateGroupFaildCodec.Detail_REPEAT;
                                            _client.SendMessage(new ScsRawDataMessage(errorReply, sourMessageid));

                                            string logInfo = "{"
                                                                + "ret:" + "CreateGroup => Detail_REPEAT" + " , "
                                                                + "info:"
                                                                    + "{"
                                                                        + "from_detail:" + this.Detail
                                                                        + "from_token:" + this.Token + ","
                                                                        + "group_detail:" + gropDetail
                                                                    + "}"
                                                                + "}";
                                            _server.__AddLogItem(logInfo);
                                            break; 
                                        }
                                    default:
                                        {
                                            string logInfo = "{"
                                                                + "ret:" + "CreateGroup => default:错误码分支未实现" + " , "
                                                                + "info:"
                                                                    + "{"
                                                                        + "from_detail:" + this.Detail
                                                                        + "from_token:" + this.Token + ","
                                                                        + "group_detail:" + gropDetail
                                                                    + "}"
                                                                + "}";
                                            _server.__AddLogItem(logInfo);
                                            return;
                                        }
                                }
                            }
                        }
                        else
                        {
                            //空名称视为非法detail
                            byte[] errorReply = new byte[2];
                            errorReply[0] = (byte)ClientHeadCodec.CreateGroup_ReplyFaild;
                            errorReply[1] = (byte)CreateGroupFaildCodec.Detail_REPEAT;
                            _client.SendMessage(new ScsRawDataMessage(errorReply, sourMessageid));

                            string logInfo = "{"
                                                + "ret:" + "CreateGroup => Detail_NULL" + " , "
                                                + "info:"
                                                    + "{"
                                                        + "from_detail:" + this.Detail
                                                        + "from_token:" + this.Token + ","
                                                        + "group_detail:" + gropDetail
                                                    + "}"
                                                + "}";
                            _server.__AddLogItem(logInfo);
                        }
                        break;
                    }
                case (int)ServerOprCodec.JoinGroup:
                    {
                        string token = parmMap["Token"] as string;
                        byte listOrRadioFlag = Byte.Parse(parmMap["ClientState"].ToString());
                        byte oprFaildCodec;
                        AppErrorInfo ret = _server.JoinGroup(this, token, listOrRadioFlag, out oprFaildCodec);
                        if (AppErrorInfo.APP_SUCESS == ret)
                        {
                            byte[] sucReply = new byte[1];
                            sucReply[0] = (byte)ClientHeadCodec.JoinGroup_ReplySucess;
                            _client.SendMessage(new ScsRawDataMessage(sucReply, sourMessageid));

                            string logInfo = "{"
                                                + "ret:" + "JoinGroup => APP_SUCESS" + " , "
                                                + "info:"
                                                    + "{"
                                                        + "from_detail:" + this.Detail
                                                        + "from_token:" + this.Token + ","
                                                        + "group_token:" + token
                                                    + "}"
                                                + "}";
                            _server.__AddLogItem(logInfo);
                        }else
                        {
                            switch (oprFaildCodec)
                            {
                                case (byte)JoinGroupFaildCodec.Client_REPEAT: { break; }
                                case (byte)JoinGroupFaildCodec.Group_INEXISTANCE: { break; }
                                default: 
                                    {
                                        string logInfo1 = "{"
                                                            + "ret:" + "CreateGroup => default:错误码分支未实现" + " , "
                                                            + "info:"
                                                                + "{"
                                                                    + "from_detail:" + this.Detail
                                                                    + "from_token:" + this.Token + ","
                                                                    + "group_token:" + token
                                                                + "}"
                                                            + "}";
                                        _server.__AddLogItem(logInfo1);
                                        return;
                                    }
                            }
                            byte[] errorReply = new byte[1 + 1];
                            errorReply[0] = (byte)ClientHeadCodec.JoinGroup_ReplyFaild;
                            errorReply[1] = (byte)oprFaildCodec;
                            _client.SendMessage(new ScsRawDataMessage(errorReply, sourMessageid));

                            string logInfo2 = "{"
                                                + "ret:" + "JoinGroup => JoinGroup_ReplyFaild" + " , "
                                                + "info:"
                                                    + "{"
                                                        + "from_detail:" + this.Detail
                                                        + "from_token:" + this.Token + ","
                                                        + "group_token:" + token
                                                    + "}"
                                                + "}";
                            _server.__AddLogItem(logInfo2);
                        }
                        break;
                    }
                case (int)ServerOprCodec.ExitGroup:
                    {
                        string token = parmMap["Token"] as string;
                        byte oprFaildCodec;
                        AppErrorInfo ret = _server.ExitGroup(this, token, out oprFaildCodec);
                        if (AppErrorInfo.APP_SUCESS == ret)
                        {
                            byte [] sucReply = new byte [1];
                            sucReply[0] = (byte)ClientHeadCodec.ExitGroup_ReplySucess;
                            _client.SendMessage(new ScsRawDataMessage(sucReply, sourMessageid));
                        }else
                        {
                            switch (oprFaildCodec)
                            {
                                case (byte)ExitGroupFaildCodec.Client_UNBELONG: { break; }
                                case (byte)ExitGroupFaildCodec.Group_INEXISTANCE: { break; }
                                default: 
                                    {
                                        throw new NotImplementedException(); 
                                    }
                            }
                            byte[] errorReply = new byte[1 + 1];
                            errorReply[0] = (byte)ClientHeadCodec.ExitGroup_ReplyFaild;
                            errorReply[1] = (byte)oprFaildCodec;
                            _client.SendMessage(new ScsRawDataMessage(errorReply, sourMessageid));
                        }
                        break;
                    }
                case (int)ServerOprCodec.RadioTransport:
                    {
                        List<string> tokens = parmMap["Tokens"] as List<string>;
                        byte[] content = parmMap["Content"] as byte[];
                        foreach (string gropToken in tokens)
                        {
                            byte oprFaildCodec;
                            AppErrorInfo ret = _server.SendRadioMessage(this, gropToken, content, out oprFaildCodec);
                            if (AppErrorInfo.APP_SUCESS == ret)
                            {
                                byte[] sucReply = new byte[1];
                                sucReply[0] = (byte)ClientHeadCodec.RadioTransport_ReplySucess;
                                _client.SendMessage(new ScsRawDataMessage(sucReply, sourMessageid));
                            }else
                            {
                                switch (oprFaildCodec)
                                {
                                    case (byte)RadioTransportFaildCodec.Trans_FORBIDDEN: { break; }
                                    case (byte)RadioTransportFaildCodec.Client_UNBELONG: { break; }
                                    case (byte)RadioTransportFaildCodec.Group_INEXISTANCE: { break; }
                                    default: 
                                        { 
                                            throw new NotImplementedException(); 
                                        }
                                }
                                byte[] errorReply = new byte[2];
                                errorReply[0] = (byte)ClientHeadCodec.RadioTransport_ReplyFaild;
                                errorReply[1] = (byte)oprFaildCodec;
                                _client.SendMessage(new ScsRawDataMessage(errorReply, sourMessageid));
                            }
                        }
                        break;
                    }
                case (int)ServerOprCodec.GroupOnlineList:
                    {
                        List<Group> grops = Server.Groups;
                        List<CSCommunicateClass.GroupInfo> jsonGrops = new List<CSCommunicateClass.GroupInfo>();
                        foreach(Group grop in grops)
                        {
                            string token = grop.Token;
                            string detail = grop.Detail;

                            CSCommunicateClass.GroupInfo gropInfoInst = new CSCommunicateClass.GroupInfo(token, detail);
                            jsonGrops.Add(gropInfoInst);
                        }
                        byte[] bytJson = JsonSerializtionFactory.JSON<CSCommunicateClass.GroupInfo>(jsonGrops);

                        byte [] replyInfo = new byte [1 + bytJson.Length];
                        replyInfo[0] = (byte)ClientHeadCodec.GetOnlineGroup_ReplySucess;
                        Buffer.BlockCopy(bytJson, 0, replyInfo, 1, bytJson.Length);

                        _client.SendMessage(new ScsRawDataMessage(replyInfo, sourMessageid));
                        break;
                    }
                case (int)ServerOprCodec.GroupClientOnlineList:
                    {
                        string gropToken = parmMap["Token"] as string;
                        Group gropInst;
                        AppErrorInfo ret = _server.GetGroup(gropToken, out gropInst);
                        if (AppErrorInfo.APP_SUCESS == ret)
                        {
                            byte oprFaildCodec;
                            List<CSCommunicateClass.ClientInfo> jsonList;
                            AppErrorInfo ret2 = gropInst.GetOnlineClient(_token, out oprFaildCodec, out jsonList);
                            if (AppErrorInfo.APP_SUCESS == ret2)
                            {
                                byte[] bytJson = JsonSerializtionFactory.JSON<CSCommunicateClass.ClientInfo>(jsonList);

                                byte [] replyInfo = new byte [1 + bytJson.Length];
                                replyInfo[0] = (byte)ClientHeadCodec.GetGroupClient_ReplySucess;
                                Buffer.BlockCopy(bytJson, 0, replyInfo, 1, bytJson.Length);

                                _client.SendMessage(new ScsRawDataMessage(replyInfo, sourMessageid));
                            }
                            else
                            {
                                byte [] errorReply = new byte [2];
                                errorReply[0] = (byte)ClientHeadCodec.GetGroupClient_ReplyFaild;
                                errorReply[1] = (byte)oprFaildCodec;
                                _client.SendMessage(new ScsRawDataMessage(errorReply, sourMessageid));
                            }
                        }
                        else
                        {
                            if (null == gropInst)
                            {
                                byte [] errorReply = new byte [2];
                                errorReply[0] = (byte)ClientHeadCodec.GetGroupClient_ReplyFaild;
                                errorReply[1] = (byte)GetGoupClientFaildCodec.Group_INEXISTENCE;
                                _client.SendMessage(new ScsRawDataMessage(errorReply, sourMessageid));
                            }
                        }
                        break;
                    }
                case (int)ServerOprCodec.ClientCancle:
                    {
                        byte[] sucReplyInfo = new byte[1];
                        sucReplyInfo[0] = (byte)ClientHeadCodec.ClientCancle_ReplySucess;
                        _client.SendMessage(new ScsRawDataMessage(sucReplyInfo, sourMessageid));
                        this.Cancle();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public virtual void Relink(IScsServerClient client)
        {
            _client.MessageReceived -= Client_MessageRecevied;
            _client.Disconnected -= Client_DisconnectHadnle;

            _client = client;

            _client.MessageReceived += Client_MessageRecevied;
            _client.Disconnected += Client_DisconnectHadnle;

            this.Online = true;
        }

        public void Send(byte [] content)
        {
            _client.SendMessage(new ScsRawDataMessage(content));
        }
        public void Send(IScsMessage msg)
        {
            _client.SendMessage(msg);
        }

        public AppErrorInfo Cancle()
        {
            AppErrorInfo ret = _server.ClientCancle(this);
            if ((CommunicationStates.Connected == _client.CommunicationState) && 
                (AppErrorInfo.APP_SUCESS == ret))
            {
                _client.Disconnect();
            }
            return ret;
        }
        protected void Client_DisconnectHadnle(object sender, EventArgs e)
        {
            //已经失去链接
            this.Online = false;
        }

        public string Token
        {
            get { return _token; }
        }
        public string Detail { get { return _detail; } }
        public bool Online
        {
            get { return _online; }
            set { _online = value; }
        }
        public Server Server
        {
            get { return _server; }
        }

        protected IScsServerClient _client = null;
        protected string _token = string.Empty;
        protected string _detail = string.Empty;
        protected Server _server = null;
        protected bool _online = true;
    }
}
