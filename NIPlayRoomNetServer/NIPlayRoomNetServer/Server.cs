using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Policy;

using Hik.Communication;
using Hik.Communication.Scs.Server;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Protocols;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication;

using ProtocolLibrary.CSProtocol;
using ProtocolLibrary.CSProtocol.CommonConfig;
using ProtocolLibrary.CSProtocol.CommonConfig.ClientMsgCodecSpace;
using ProtocolLibrary.JsonSerializtion;

using NIPlayRoomNetServer.Interface;
using NIPlayRoomNetServer.ServerClient;
using System.IO;

namespace NIPlayRoomNetServer
{
    public enum AppErrorInfo
    {
        APP_SUCESS = 0,
        APP_FAILD
    };
    public partial class Server : Form, IServer
    {
        public Server()
        {
            __InitFormEvents();
            InitializeComponent();
            (this as IServer).Init(10086);
        }

        #region IServer
        public AppErrorInfo Init(int tcpPort)
        {
            if (null == _server)
            {
                __CreateNewLog();
                try
                {
                    _server = ScsServerFactory.CreateServer(new ScsTcpEndPoint(tcpPort));
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Tcp服务创建异常: " + ex.Data.ToString());
                    __AddLogItem("Tcp服务创建异常: " + ex.Data.ToString());

                    __AddLogItem("Server.Init => AppErrorInfo.APP_FAILD");
                    return AppErrorInfo.APP_FAILD;
                }
                _server.WireProtocolFactory = new MyUnderScsWireProtocolFactory();
                _server.ClientConnected += __Global_server_ClientConnected;

                _unTypeClientBuff = new HashSet<IScsServerClient>();
                _tokenClientMap = new Hashtable();
                _detailClientMap = new Hashtable();

                _server.Start();

                __AddLogItem("Server.Init => AppErrorInfo.APP_SUCESS");
                return AppErrorInfo.APP_SUCESS;
            }
            else
            {
                return AppErrorInfo.APP_FAILD;
            }
        }

        public void Stop()
        {
            try
            {
                __ServerDisableClientProcess();
                _server.Stop();
            }
            catch (System.Exception ex)
            {
                __AddLogItem("Server stopping:" + ex.ToString());
            }
            __ENdLog();
        }

        private void __ServerDisableClientProcess()
        {
            //group disable notify
            this.GroupManagerDisable();

            //logout
            if (0 < _tokenClientMap.Count)
            {
                ArrayList tmpArrayBuff = new ArrayList();
                IDictionaryEnumerator itorTokenClientMap = _tokenClientMap.GetEnumerator();
                while (false != itorTokenClientMap.MoveNext())
                {
                    tmpArrayBuff.Add(itorTokenClientMap.Value);
                }
                foreach (object wilDelItem in tmpArrayBuff)
                {
                    ((BaseServerClient)wilDelItem).Cancle();
                }
            }

            if (0 < _unTypeClientBuff.Count)
            {
                IScsServerClient[] tmpArrayBuff = new IScsServerClient[_unTypeClientBuff.Count];
                _unTypeClientBuff.CopyTo(tmpArrayBuff);
                foreach (IScsServerClient scsClient in tmpArrayBuff)
                {
                    if (CommunicationStates.Connected == scsClient.CommunicationState)
                    {
                        scsClient.Disconnect();
                    }
                }
                _unTypeClientBuff.Clear();
            }
        }

        private void __Global_server_ClientConnected(object sender, ServerClientEventArgs e)
        {
            //客户端推入未分类缓冲区
            Console.WriteLine("A new client is connected. Address: " + e.Client.RemoteEndPoint);
            __AddGlobalClientListener(e.Client);
            if (false == _unTypeClientBuff.Contains(e.Client))
            {
                _unTypeClientBuff.Add(e.Client);
            }
            else
            {
                //在未断开分类器函数的情况下触发此分支是一个错误
                throw new Exception();
            }
        }

        private void __AddGlobalClientListener(IScsServerClient client)
        {
            client.MessageReceived += __Global_client_TypeFilter;
            client.Disconnected += __Global_UntypeClientDisconnect;
        }

        private void __RemoGlobalClientListener(IScsServerClient client)
        {
            client.MessageReceived -= __Global_client_TypeFilter;
            client.Disconnected -= __Global_UntypeClientDisconnect;
        }

        private void __Global_UntypeClientDisconnect(object sender, EventArgs e)
        {
            __RemoGlobalClientListener((IScsServerClient)sender);
            _unTypeClientBuff.Remove((IScsServerClient)sender);
        }

        private void __Global_client_TypeFilter(object sender, MessageEventArgs e)
        {
            //按照分类号及TokenID 初始化客户端套接类
            IScsServerClient tmpServerClient = sender as IScsServerClient;
            if (false == (e.Message is ScsRawDataMessage))
            {
                //不在协议内的未知消息
                return;
            }
            byte[] registContent = ((ScsRawDataMessage)e.Message).MessageData;
            if ((byte)ServerOprCodec.Initialize == registContent[0])
            {
                int bIsRlink = Convert.ToInt32(registContent[1]);
                if (0 == bIsRlink)
                {
                    byte[] clientDetail = new byte[registContent.Length - 2];
                    Buffer.BlockCopy(registContent, 2, clientDetail, 0, clientDetail.Length);
                    string strClientDetail = Encoding.UTF8.GetString(clientDetail);
                    if (false == _detailClientMap.ContainsKey(strClientDetail))
                    {
                        string guid = __MakeGuid();
                        BaseServerClient client = new BaseServerClient(guid, tmpServerClient, this, strClientDetail);
                        _detailClientMap.Add(strClientDetail, client);
                        _tokenClientMap.Add(guid, client);
                        __RemoGlobalClientListener(tmpServerClient);
                        _unTypeClientBuff.Remove(tmpServerClient);

                        //reply app's token
                        byte[] sucReply = new byte[1 + ConstData.tokenLength];
                        sucReply[0] = (byte)ClientHeadCodec.Init_ReplySucess;
                        Buffer.BlockCopy(Encoding.ASCII.GetBytes(guid), 0, sucReply, 1, ConstData.tokenLength);
                        tmpServerClient.SendMessage(new ScsRawDataMessage(sucReply, e.Message.MessageId));

                        string logInfo = "{" + "ret:" + "新的套接字已建立:" + " , " + "info:" + "{" + "client_detail:" + client.Detail + " , " + "client_token:" + client.Token + "}" + "}";
                        __AddLogItem(logInfo);
                    }
                    else
                    {
                        //detail repeat.
                        byte[] errorReply = new byte[2];
                        errorReply[0] = (byte)ClientHeadCodec.Init_ReplyFaild;
                        errorReply[1] = (byte)InitFaildCodec.Detail_REPEAT;
                        tmpServerClient.SendMessage(new ScsRawDataMessage(errorReply, e.Message.MessageId));

                        string logInfo = "{" + "ret:" + "__Global_client_TypeFilter => Detail_REPEAT" + " , " + "info:" + "{" + "client_detail:" + strClientDetail + "}" + "}";
                        __AddLogItem(logInfo);
                    }
                }
                else
                {
                    string token = string.Empty;
                    if ((2 + ConstData.tokenLength) == registContent.Length)
                    {
                        byte[] bytToken = new byte[ConstData.tokenLength];
                        Buffer.BlockCopy(registContent, 2, bytToken, 0, ConstData.tokenLength);
                        token = Encoding.ASCII.GetString(bytToken);

                        if (_tokenClientMap.ContainsKey(token))
                        {
                            BaseServerClient preClient = _tokenClientMap[token] as BaseServerClient;
                            preClient.Relink(tmpServerClient);
                            __RemoGlobalClientListener(tmpServerClient);
                            _unTypeClientBuff.Remove(tmpServerClient);

                            //reRlink sucessful
                            byte[] sucReply = new byte[1];
                            sucReply[0] = (byte)ClientHeadCodec.Init_ReplySucess;
                            tmpServerClient.SendMessage(new ScsRawDataMessage(sucReply, e.Message.MessageId));

                            string logInfo = "{" + "ret:" + "套接字重链接成功:" + " , " + "info:" + "{" + "client detail:" + preClient.Detail + " , " + "client_token:" + preClient.Token + "}" + "}";
                            __AddLogItem(logInfo);
                        }
                        else
                        {
                            //重连操作所需Token不存在
                            byte[] errorReply = new byte[2];
                            errorReply[0] = (byte)ClientHeadCodec.Init_ReplyFaild;
                            errorReply[1] = (byte)InitFaildCodec.Relink_FORBIDDEN;
                            tmpServerClient.SendMessage(new ScsRawDataMessage(errorReply, e.Message.MessageId));

                            string logInfo = "{" + "ret:" + "__Global_client_TypeFilter => Relink_FORBIDDEN" + " , " + "info:" + "{" + "client_token:" + token + "}" + "}";
                            __AddLogItem(logInfo);
                        }
                    }
                    else
                    {
                        //重连操作所需Token不存在
                        byte[] errorReply = new byte[2];
                        errorReply[0] = (byte)ClientHeadCodec.Init_ReplyFaild;
                        errorReply[1] = (byte)InitFaildCodec.Relink_FORBIDDEN;
                        tmpServerClient.SendMessage(new ScsRawDataMessage(errorReply, e.Message.MessageId));

                        string logInfo = "{" + "ret:" + "__Global_client_TypeFilter => Relink_FORBIDDEN" + " , " + "info:" + "{" + "client_token:" + token + "}" + "}";
                        __AddLogItem(logInfo);
                    }
                }
                return;
            }
            else
            {
                //若未进行注册便执行其他操作，则打回错误报文
                IScsServerClient _client = sender as IScsServerClient;
                byte[] errorReply = new byte[2];
                errorReply[0] = (byte)ClientHeadCodec.Init_ReplyFaild;
                errorReply[1] = (byte)InitFaildCodec.Initialize_UNFINISH;
                _client.SendMessage(new ScsRawDataMessage(errorReply, e.Message.MessageId));

                string logInfo = "{" + "ret:" + "__Global_client_TypeFilter => Initialize_UNFINISH" + " , " + "info:" + "{" + "OperatCodec:" + registContent[0] + "}" + "}";
                __AddLogItem(logInfo);
                return;
            }
        }

        public AppErrorInfo GroupSend(string srcClientToken,
                                                    string srcGroupToken,
                                                    string dstToken,
                                                    byte[] content, out byte oprFaildCodec)
        {
            BaseServerClient srcClient = null;
            if (_tokenClientMap.ContainsKey(srcClientToken))
            {
                srcClient = _tokenClientMap[srcClientToken] as BaseServerClient;
            }
            BaseServerClient dstClient = null;
            if (_tokenClientMap.ContainsKey(dstToken))
            {
                dstClient = _tokenClientMap[dstToken] as BaseServerClient;
            }

            if (null != dstClient)
            {
                if (dstClient.Online)
                {
                    byte[] bytTargetContent = new byte[1 + (ConstData.tokenLength * 2) + content.Length];
                    int pTargetContentBufOffset = 0;

                    //ServerOprCodec
                    Buffer.BlockCopy(BitConverter.GetBytes((byte)ClientHeadCodec.GroupTransport), 0, bytTargetContent, pTargetContentBufOffset, 1);
                    ++pTargetContentBufOffset;

                    //GroupToken
                    byte[] bytSrcGroupToken = Encoding.ASCII.GetBytes(srcGroupToken);
                    Buffer.BlockCopy(bytSrcGroupToken, 0, bytTargetContent, pTargetContentBufOffset, ConstData.tokenLength);
                    pTargetContentBufOffset += ConstData.tokenLength;

                    //ClientToken
                    byte[] bytSrcClientToken = Encoding.ASCII.GetBytes(srcClientToken);
                    Buffer.BlockCopy(bytSrcClientToken, 0, bytTargetContent, pTargetContentBufOffset, ConstData.tokenLength);
                    pTargetContentBufOffset += ConstData.tokenLength;

                    //Main Content
                    Buffer.BlockCopy(content, 0, bytTargetContent, pTargetContentBufOffset, content.Length);
                    pTargetContentBufOffset += content.Length;

                    dstClient.Send(bytTargetContent);

                    oprFaildCodec = (byte)ClientHeadCodec.P2pTransport_ReplySucess;
                    return AppErrorInfo.APP_SUCESS;
                }
                else
                {
                    oprFaildCodec = (byte)P2pTransportFaildCodec.Target_UNCONNECT;
                    return AppErrorInfo.APP_FAILD;
                }
            }
            else
            {
                oprFaildCodec = (byte)P2pTransportFaildCodec.Token_UNPARSE;
                return AppErrorInfo.APP_FAILD;
            }
        }

        public AppErrorInfo Send(string srcToken, string dstToken, byte[] content, out byte oprFaildCodec)
        {
            BaseServerClient srcClient = null;
            if (_tokenClientMap.ContainsKey(srcToken))
            {
                srcClient = _tokenClientMap[srcToken] as BaseServerClient;
            }
            BaseServerClient dstClient = null;
            if (_tokenClientMap.ContainsKey(dstToken))
            {
                dstClient = _tokenClientMap[dstToken] as BaseServerClient;
            }

            if (null != dstClient)
            {
                if (dstClient.Online)
                {
                    byte[] bytSrcToken;
                    bytSrcToken = Encoding.ASCII.GetBytes(srcToken);

                    byte[] bytTargetContent = new byte[1 + content.Length + ConstData.tokenLength];
                    int pTargetContentBufOffset = 0;

                    //ServerOprCodec
                    Buffer.BlockCopy(BitConverter.GetBytes((byte)ClientHeadCodec.Transport), 0, bytTargetContent, pTargetContentBufOffset, 1);
                    ++pTargetContentBufOffset;

                    //Token
                    Buffer.BlockCopy(bytSrcToken, 0, bytTargetContent, pTargetContentBufOffset, ConstData.tokenLength);
                    pTargetContentBufOffset += ConstData.tokenLength;

                    //Main Content
                    Buffer.BlockCopy(content, 0, bytTargetContent, pTargetContentBufOffset, content.Length);
                    pTargetContentBufOffset += content.Length;

                    dstClient.Send(bytTargetContent);

                    oprFaildCodec = (byte)ClientHeadCodec.P2pTransport_ReplySucess;
                    return AppErrorInfo.APP_SUCESS;
                }
                else
                {
                    oprFaildCodec = (byte)P2pTransportFaildCodec.Target_UNCONNECT;
                    return AppErrorInfo.APP_FAILD;
                }
            }
            else
            {
                oprFaildCodec = (byte)P2pTransportFaildCodec.Token_UNPARSE;
                return AppErrorInfo.APP_FAILD;
            }
        }

        public AppErrorInfo ClientCancle(BaseServerClient client)
        {
            AppErrorInfo ret = AppErrorInfo.APP_SUCESS;
            if (_tokenClientMap.ContainsKey(client.Token))
            {
                _tokenClientMap.Remove(client.Token);
                _detailClientMap.Remove(client.Detail);
            }
            ret |= this.GropClientCancle(client);
            return ret;
        }

        private string __MakeGuid()
        {
            return System.Guid.NewGuid().ToString();
        }

        #region Log
        private void __CreateNewLog()
        {
            string fileDate = DateTime.Today.ToShortDateString() + "_" + DateTime.Now.ToLongTimeString();
            fileDate = fileDate.Replace('/', '_');
            fileDate = fileDate.Replace(':', '_');

            string dirPath = Application.StartupPath + "/log/";
            if (false == Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            _log = File.CreateText(dirPath + fileDate + ".txt");
            _log.AutoFlush = true;
        }

        internal void __AddLogItem(string content)
        {
            string timeStamp = "< " + DateTime.Now.ToString() + ">";
            _logMsgSeq.Enqueue(content + " " + timeStamp);
            _log.WriteLine(content + " " + timeStamp);

        }

        private void __ENdLog()
        {
            _log.Flush();
            _log.Close();
        }
        #endregion

        private IScsServer _server = null;
        private HashSet<IScsServerClient> _unTypeClientBuff = null;
        private Hashtable _tokenClientMap = null;                   //按Token索引
        private Hashtable _detailClientMap = null;

        private StreamWriter _log = null;
        private Queue<string> _logMsgSeq = new Queue<string>();
        #endregion

        #region IGroupManager
        /// <summary>
        /// 创建空群组
        /// </summary>
        /// <returns>群组Token ID</returns>
        public void GroupManagerDisable()
        {
            foreach (Group item in _groups)
            {
                item.Disable();
            }
            _groups.Clear();
            _tokenGroupsMap.Clear();
            _detailGroupMap.Clear();
        }

        public AppErrorInfo CreateGroup(string detail, out string token, out byte retFaildCodec)
        {
            if (_detailGroupMap.ContainsKey(detail))
            {
                retFaildCodec = (byte)CreateGroupFaildCodec.Detail_REPEAT;
                token = string.Empty;
                return AppErrorInfo.APP_FAILD;
            }
            token = __MakeGuid();
            Group gopInst = new Group(token, detail);
            __AddGroup(token, detail, gopInst);
            retFaildCodec = (byte)ClientHeadCodec.CreateGroup_ReplySucess;
            return AppErrorInfo.APP_SUCESS;
        }

        public AppErrorInfo JoinGroup(BaseServerClient client, string groupToken, byte listOrRadioCodec, out byte oprFaildCodec)
        {
            if (false == _tokenGroupsMap.ContainsKey(groupToken))
            {
                oprFaildCodec = (byte)JoinGroupFaildCodec.Group_INEXISTANCE;
                return AppErrorInfo.APP_FAILD;
            }
            return (_tokenGroupsMap[groupToken] as Group).Regist(client, listOrRadioCodec, out oprFaildCodec);
        }
        public AppErrorInfo ExitGroup(BaseServerClient client, string groupToken, out byte oprFaildCodec)
        {
            if (false == _tokenGroupsMap.ContainsKey(groupToken))
            {
                oprFaildCodec = (byte)ExitGroupFaildCodec.Group_INEXISTANCE;
                return AppErrorInfo.APP_FAILD;
            }
            Group willCanleFromGroup = _tokenGroupsMap[groupToken] as Group;
            AppErrorInfo ret = AppErrorInfo.APP_SUCESS;
            ret |= willCanleFromGroup.Cancle(client, out oprFaildCodec);
            if (AppErrorInfo.APP_SUCESS != ret)
            {
                return ret;
            }
            //若Group已清空则Release
            //if (0 == willCanleFromGroup.MemberCount)
            //{
            //    ret |= __RemoveGroup(willCanleFromGroup);
            //}
            return ret;
        }

        public AppErrorInfo SendRadioMessage(BaseServerClient client, string gropToken, byte[] content, out byte oprFaildCodec)
        {
            if (_tokenGroupsMap.ContainsKey(gropToken))
            {
                return (_tokenGroupsMap[gropToken] as Group).SendRadioMessage(client, content, out oprFaildCodec);
            }
            else
            {
                oprFaildCodec = (byte)RadioTransportFaildCodec.Group_INEXISTANCE;
                return AppErrorInfo.APP_FAILD;
            }
        }

        public AppErrorInfo GetGroup(string token, out Group gropInst)
        {
            if (_tokenGroupsMap.ContainsKey(token))
            {
                gropInst = _tokenGroupsMap[token] as Group;
                return AppErrorInfo.APP_SUCESS;
            }
            else
            {
                gropInst = null;
                return AppErrorInfo.APP_FAILD;
            }
        }

        public AppErrorInfo GropClientCancle(BaseServerClient client)
        {
            AppErrorInfo ret = AppErrorInfo.APP_SUCESS;
            foreach (Group gropInst in _groups)
            {
                if (gropInst.ContainClient(client))
                {
                    byte tmpOprFaildCodec;
                    ret |= this.ExitGroup(client, gropInst.Token, out tmpOprFaildCodec);
                }
            }
            return ret;
        }

        private AppErrorInfo __AddGroup(string token, string detail, Group group)
        {
            if (_tokenGroupsMap.ContainsValue(group))
            {
                return AppErrorInfo.APP_FAILD;
            }
            _groups.Add(group);
            _detailGroupMap.Add(detail, group);
            _tokenGroupsMap.Add(token, group);
            return AppErrorInfo.APP_SUCESS;
        }
        private AppErrorInfo __RemoveGroup(Group group)
        {
            if (false == _tokenGroupsMap.ContainsValue(group))
            {
                return AppErrorInfo.APP_FAILD;
            }
            group.CancleAll();
            _groups.Remove(group);
            _tokenGroupsMap.Remove(group.Token);
            _detailGroupMap.Remove(group.Detail);
            return AppErrorInfo.APP_SUCESS;
        }

        public List<Group> Groups { get { return _groups; } }
        private List<Group> _groups = new List<Group>();
        private Hashtable _tokenGroupsMap = new Hashtable();
        private Hashtable _detailGroupMap = new Hashtable();
        #endregion
    }
}
