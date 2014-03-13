using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Runtime.Serialization.Formatters;

using Hik.Communication.Scs;
using Hik.Communication;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Threading;

using ProtocolLibrary.CSProtocol;
using ProtocolLibrary.CSProtocol.CommonConfig;
using ProtocolLibrary.CSProtocol.CommonConfig.ClientMsgCodecSpace;
using ProtocolLibrary;
using ProtocolLibrary.JsonSerializtion;

using Middleware.Communication.CommunicationConfig;
using Middleware.Interface;
using Middleware.Communication;
using Middleware.Communication.Package;
using Middleware.Communication.Package.Internal;
using Middleware.Device;
using Middleware.Interface.ServerProcotolOperator;
using Middleware.Interface.ServerProcotolOperator.Request;
using Middleware.Interface.ServerProcotolOperator.Reply;
using Middleware.LayerProcessor;
using Middleware.Communication.EndPoint.Tcp;

namespace Middleware.LayerProcessor
{
    #region Framework.SCS 调用层 / 服务器字节协议解析层

    #region 服务层交互包装类

    /// <summary>
    /// 数据包内容所属的工作层
    /// </summary>
    internal enum EnumServControlPkgBelongLayerType
    {
        Untype,
        CoreLogicLayer,
        MiddlewareCommunicateLayer,
        GroupCommunicateLayer
    }

    /// <summary>
    /// 对数据包当前的功能进行分类, 同步于协议ServerOprCodec
    /// </summary>
    internal enum EnumServControlPkgCurrentdutyType
    {
        UnType,
        MiddlewareMsgIncomming,
        GroupMsgIncomming,
        CSHandshake,
        Middleware2MiddlewareCommunicat,
        CreateGroup,
        JoinGroup,
        ExitGroup,
        RadioTransport,
        GroupOnlineList,
        GroupClientOnlineList,
        ClientCancle
    }

    internal abstract class ServerCtrBasePackage : IServerCtrBasePackage
    {
        internal ServerCtrBasePackage()
        {
        }

        internal virtual EnumServControlPkgBelongLayerType WorkingLayer
        {
            get { return _pkgWorkingLayer; }
        }
        internal protected EnumServControlPkgBelongLayerType _pkgWorkingLayer = EnumServControlPkgBelongLayerType.Untype;

        internal virtual EnumServControlPkgCurrentdutyType CurrentdutyType
        {
            get { return _pkgCurrentdutyType; }
        }
        internal protected EnumServControlPkgCurrentdutyType _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.UnType;

        internal byte[] RawData
        {
            get { return _rawData; }
        }
        internal protected byte[] _rawData = null;
    }
    /// <summary>
    /// 以策略模式设置状态
    /// </summary>
    internal abstract class ServerCtrRequestPackage : ServerCtrBasePackage, IServerRequsetInfoOperator
    {
        internal ServerCtrRequestPackage(bool bIsWaittingResponse)
            : base()
        {
            _bIsWaittingResponse = bIsWaittingResponse;
        }

        public virtual byte[] Export()
        {
            if (EnumServControlPkgCurrentdutyType.UnType == _pkgCurrentdutyType ||
                EnumServControlPkgBelongLayerType.Untype == _pkgWorkingLayer)
            {
                throw new Exception("状态无效的ServerRequestPackage.");
            }
            return _rawData;
        }

        internal bool WattingResponse
        {
            get { return _bIsWaittingResponse; }
            set { _bIsWaittingResponse = value; }
        }
        private bool _bIsWaittingResponse = false;

        private void __ClearState()
        {
            _rawData = null;
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.UnType;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.Untype;
        }
        #region 导出接口集
        #region IMiddlewareHandShakeRequest
        public IMiddleware2ServerHandShakeRequest Active_Middleware2ServerHandShakeRequest()
        {
            return this as IMiddleware2ServerHandShakeRequest;
        }
        public void HandShake(bool isReLink, string detailOrToken)
        {
            __ClearState();
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.CSHandshake;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.CoreLogicLayer;

            byte relinkFlag;
            byte[] bytDetailOrToken;
            if (isReLink)
            {
                relinkFlag = (byte)1;
                bytDetailOrToken = Encoding.ASCII.GetBytes(detailOrToken);
            }
            else
            {
                relinkFlag = (byte)0;
                bytDetailOrToken = Encoding.UTF8.GetBytes(detailOrToken);
            }
            byte[] bytWilExportContent = new byte[1 + 1 + bytDetailOrToken.Length];
            bytWilExportContent[0] = (byte)ServerOprCodec.Initialize;
            bytWilExportContent[1] = relinkFlag;
            Buffer.BlockCopy(bytDetailOrToken, 0, bytWilExportContent, 2, bytDetailOrToken.Length);
            _rawData = bytWilExportContent;
        }
        #endregion

        #region IMiddleware2MiddlewareCommunicatRequest
        public IMiddleware2MiddlewareCommunicatRequest Active_MiddlewareCommunicatRequest()
        {
            return this as IMiddleware2MiddlewareCommunicatRequest;
        }
        public void WilSendMiddleware2MiddlewareMessage(BaseDevice targetDevice, MiddlewareTransferPackage wilSendMiddlewarePkg)
        {
            __ClearState();
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.Middleware2MiddlewareCommunicat;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.MiddlewareCommunicateLayer;

            _MTpkg = wilSendMiddlewarePkg;

            byte[] bytTargetToken = Encoding.ASCII.GetBytes(targetDevice.Token);
            //序列化MiddlewareTransferPackage
            byte[] bytWilSendMiddlewarePkg = wilSendMiddlewarePkg.SerializeMiddlewareMessage();

            byte[] bytWilSendCompletContent = new byte[1 + 1 + ConstData.tokenLength + bytWilSendMiddlewarePkg.Length];
            bytWilSendCompletContent[0] = (byte)ServerOprCodec.P2pTransport;
            bytWilSendCompletContent[1] = (byte)1; //强制
            Buffer.BlockCopy(bytTargetToken, 0, bytWilSendCompletContent, 1 + 1, ConstData.tokenLength);
            Buffer.BlockCopy(bytWilSendMiddlewarePkg, 0, bytWilSendCompletContent, 1 + 1 + ConstData.tokenLength, bytWilSendMiddlewarePkg.Length);

            _rawData = bytWilSendCompletContent;
        }

        public MiddlewareTransferPackage MTPkg
        {
            get { return MTPkg; }
        }
        protected MiddlewareTransferPackage _MTpkg = null;
        #endregion

        #region ICreateGroupRequest
        public ICreateGroupRequest Active_CreateGroupRequest()
        {
            return this as ICreateGroupRequest;
        }
        public void CreateNewGroup(string groupDetail)
        {
            __ClearState();
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.CreateGroup;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;

            byte[] bytDetailContent = Encoding.UTF8.GetBytes(groupDetail);
            byte[] bytWilSendCompletContent = new byte[1 + bytDetailContent.Length];
            bytWilSendCompletContent[0] = (byte)ServerOprCodec.CreateGroup;
            Buffer.BlockCopy(bytDetailContent, 0, bytWilSendCompletContent, 1, bytDetailContent.Length);

            _rawData = bytWilSendCompletContent;
        }
        #endregion

        #region IJoinGroupRequest
        public IJoinGroupRequest Active_JoinGroupRequest()
        {
            return this as IJoinGroupRequest;
        }
        public void JoinGroup(string gropToken, GroupMemberRole memberRole)
        {
            __ClearState();
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.JoinGroup;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;

            byte roleFlag;
            switch (memberRole)
            {
                case GroupMemberRole.Listener:
                    {
                        roleFlag = (byte)(1);
                        break;
                    }
                case GroupMemberRole.Speaker:
                    {
                        roleFlag = (byte)(2);
                        break;
                    }
                case GroupMemberRole.Both:
                    {
                        roleFlag = (byte)(3);
                        break;
                    }
                default:
                    {
                        throw new Exception("无效的枚举值");
                    }
            }

            byte[] bytWilSendCompletContent = new byte[1 + 1 + 1 + ConstData.tokenLength];
            bytWilSendCompletContent[0] = (byte)ServerOprCodec.JoinGroup;
            bytWilSendCompletContent[1] = (byte)1; //强制
            bytWilSendCompletContent[2] = roleFlag;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(gropToken), 0, bytWilSendCompletContent, 3, ConstData.tokenLength);

            _rawData = bytWilSendCompletContent;
        }
        #endregion

        #region IExitGroupRequest
        public IExitGroupRequest Active_ExitGroupRequest()
        {
            return this as IExitGroupRequest;
        }
        public void ExitGroup(string gropToken)
        {
            __ClearState();
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.ExitGroup;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;

            byte[] bytWilSendCompletContent = new byte[1 + 1 + ConstData.tokenLength];
            bytWilSendCompletContent[0] = (byte)ServerOprCodec.ExitGroup;
            bytWilSendCompletContent[1] = 1; //强制
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(gropToken), 0, bytWilSendCompletContent, 2, ConstData.tokenLength);

            _rawData = bytWilSendCompletContent;
        }
        #endregion

        #region IRadioMessageRequest
        public IRadioMessageRequest Active_RadioMessageRequest()
        {
            return this as IRadioMessageRequest;
        }
        public void RadioMessage(string gropToken, RadioPackage radioPkg)
        {
            __ClearState();
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.RadioTransport;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;

            byte[] bytRadioPkg = radioPkg.SerializeMiddlewareMessage();
            byte[] bytGropToken = Encoding.ASCII.GetBytes(gropToken);

            byte[] bytWilSendCompletContent = new byte[1 + 1 + ConstData.tokenLength + bytRadioPkg.Length];
            bytWilSendCompletContent[0] = (byte)ServerOprCodec.RadioTransport;
            bytWilSendCompletContent[1] = 1; //强制
            Buffer.BlockCopy(bytGropToken, 0, bytWilSendCompletContent, 2, ConstData.tokenLength);
            Buffer.BlockCopy(bytRadioPkg, 0, bytWilSendCompletContent, 1 + 1 + ConstData.tokenLength, bytRadioPkg.Length);

            _rawData = bytWilSendCompletContent;
        }
        #endregion

        #region IOnlineGroupListRequest
        public IOnlineGroupListRequest Active_OnlineGroupListRequest()
        {
            return this as IOnlineGroupListRequest;
        }
        public void OnlineGroupRequest()
        {
            __ClearState();
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.GroupOnlineList;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;

            byte[] bytWilSendCompletContent = new byte[1];
            bytWilSendCompletContent[0] = (byte)ServerOprCodec.GroupOnlineList;

            _rawData = bytWilSendCompletContent;
        }
        #endregion

        #region IOnlineGroupClientRequest
        public IOnlineGroupClientRequest Active_OnlineGroupClientRequest()
        {
            return this as IOnlineGroupClientRequest;
        }
        public void OnlineGroupClientRequest(string gropToken)
        {
            __ClearState();
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.GroupClientOnlineList;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;

            byte[] bytGropToken = Encoding.ASCII.GetBytes(gropToken);
            byte[] bytWilSendCompletContent = new byte[1 + ConstData.tokenLength];
            bytWilSendCompletContent[0] = (byte)ServerOprCodec.GroupClientOnlineList;
            Buffer.BlockCopy(bytGropToken, 0, bytWilSendCompletContent, 1, ConstData.tokenLength);

            _rawData = bytWilSendCompletContent;
        }
        #endregion

        #region IClientCancleRequest
        public IClientCancleRequest Active_ClientCancleRequest()
        {
            return this as IClientCancleRequest;
        }
        public void ClientCancle()
        {
            __ClearState();
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.ClientCancle;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.CoreLogicLayer;

            byte[] bytWilSendCompletContent = new byte[1];
            bytWilSendCompletContent[0] = (byte)ServerOprCodec.ClientCancle;

            _rawData = bytWilSendCompletContent;
        }
        #endregion
        #endregion
    }
    internal abstract class ServerCtrReplyPackage : ServerCtrBasePackage, IServerReplyInfoOperator
    {
        internal enum ReplyType
        {
            group_propagate,
            middleware_propagate,
            reply
        }

        internal ServerCtrReplyPackage(byte[] content)
            : base()
        {
            _rawData = content;
            byte bytServReplyMsgRetCodec = content[0];
            switch (bytServReplyMsgRetCodec)
            {
                case (byte)ClientHeadCodec.Transport:
                    {
                        _replyType = ReplyType.middleware_propagate;
                        _pkgWorkingLayer = EnumServControlPkgBelongLayerType.MiddlewareCommunicateLayer;
                        _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.MiddlewareMsgIncomming;
                        break;
                    }
                case (byte)ClientHeadCodec.GroupTransport:
                    {
                        _replyType = ReplyType.group_propagate;
                        _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;
                        _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.GroupMsgIncomming;
                        break;
                    }
                case (byte)ClientHeadCodec.Init_ReplySucess:
                case (byte)ClientHeadCodec.Init_ReplyFaild:
                    {
                        _replyType = ReplyType.reply;
                        _pkgWorkingLayer = EnumServControlPkgBelongLayerType.CoreLogicLayer;
                        _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.CSHandshake;
                        break;
                    }
                case (byte)ClientHeadCodec.P2pTransport_ReplySucess:
                case (byte)ClientHeadCodec.P2pTransport_ReplyFaild:
                    {
                        _replyType = ReplyType.reply;
                        _pkgWorkingLayer = EnumServControlPkgBelongLayerType.MiddlewareCommunicateLayer;
                        _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.Middleware2MiddlewareCommunicat;
                        break;
                    }
                case (byte)ClientHeadCodec.CreateGroup_ReplySucess:
                case (byte)ClientHeadCodec.CreateGroup_ReplyFaild:
                    {
                        _replyType = ReplyType.reply;
                        _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;
                        _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.CreateGroup;
                        break;
                    }
                case (byte)ClientHeadCodec.JoinGroup_ReplySucess:
                case (byte)ClientHeadCodec.JoinGroup_ReplyFaild:
                    {
                        _replyType = ReplyType.reply;
                        _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;
                        _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.JoinGroup;
                        break;
                    }
                case (byte)ClientHeadCodec.ExitGroup_ReplySucess:
                case (byte)ClientHeadCodec.ExitGroup_ReplyFaild:
                    {
                        _replyType = ReplyType.reply;
                        _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;
                        _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.ExitGroup;
                        break;
                    }
                case (byte)ClientHeadCodec.RadioTransport_ReplySucess:
                case (byte)ClientHeadCodec.RadioTransport_ReplyFaild:
                    {
                        _replyType = ReplyType.reply;
                        _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;
                        _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.RadioTransport;
                        break;
                    }
                case (byte)ClientHeadCodec.GetOnlineGroup_ReplySucess:
                case (byte)ClientHeadCodec.GetOnlineGroup_ReplyFaild:
                    {
                        _replyType = ReplyType.reply;
                        _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;
                        _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.GroupOnlineList;
                        break;
                    }
                case (byte)ClientHeadCodec.GetGroupClient_ReplySucess:
                case (byte)ClientHeadCodec.GetGroupClient_ReplyFaild:
                    {
                        _replyType = ReplyType.reply;
                        _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;
                        _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.GroupClientOnlineList;
                        break;
                    }
                case (byte)ClientHeadCodec.ClientCancle_ReplySucess:
                case (byte)ClientHeadCodec.ClientCancle_ReplyFaild:
                    {
                        _replyType = ReplyType.reply;
                        _pkgWorkingLayer = EnumServControlPkgBelongLayerType.CoreLogicLayer;
                        _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.ClientCancle;
                        break;
                    }
                default:
                    {
                        throw new NotImplementedException("无法识别的下行报文头");
                    }
            }
        }
        internal ReplyType MsgType
        {
            get { return _replyType; }
        }
        private ReplyType _replyType;

        #region 构造接口集
        #region IMiddlewareHandShakeReply
        public IMiddleware2ServerHandShakeReply Active_MiddlewareHandShakeReply()
        {
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.CSHandshake;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.CoreLogicLayer;
            return this as IMiddleware2ServerHandShakeReply;
        }
        public void HandShakeRet(out bool ret, out string errorDetailOrToken)
        {
            if (EnumServControlPkgCurrentdutyType.CSHandshake == _pkgCurrentdutyType)
            {
                byte bytHandShakeRetCodec = _rawData[0];
                if ((byte)ClientHeadCodec.Init_ReplySucess == bytHandShakeRetCodec)
                {
                    ret = true;
                    if (1 + ConstData.tokenLength == _rawData.Length)
                    {
                        //初始化
                        byte[] bytClientSoketToken = new byte[ConstData.tokenLength];
                        Buffer.BlockCopy(_rawData, 1, bytClientSoketToken, 0, ConstData.tokenLength);
                        errorDetailOrToken = Encoding.ASCII.GetString(bytClientSoketToken);
                    }
                    else if (1 == _rawData.Length)
                    {
                        //重连
                        errorDetailOrToken = string.Empty;
                    }
                    else
                    {
                        throw new Exception("无法解析的报文格式");
                    }
                }
                else
                {
                    ret = false;
                    byte bytFaildCodec = _rawData[1];
                    switch (bytFaildCodec)
                    {
                        case (byte)InitFaildCodec.Detail_REPEAT:
                            {
                                errorDetailOrToken = InitFaildCodec.Detail_REPEAT.ToString();
                                break;
                            }
                        case (byte)InitFaildCodec.Relink_FORBIDDEN:
                            {
                                errorDetailOrToken = InitFaildCodec.Relink_FORBIDDEN.ToString();
                                break;
                            }
                        case (byte)InitFaildCodec.Initialize_UNFINISH:
                            {
                                errorDetailOrToken = InitFaildCodec.Relink_FORBIDDEN.ToString();
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("无效的InitFaildCodec");
                            }
                    }
                }
            }
            else
            {
                throw new Exception("不是一份合法的HandShake响应报文, 阻止调用");
            }
        }
        #endregion

        #region IMiddleware2MiddlewareCommunicatReply
        public IMiddleware2MiddlewareCommunicatReply Active_Middleware2MiddlewareCommunicatReply()
        {
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.Middleware2MiddlewareCommunicat;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.MiddlewareCommunicateLayer;
            return this as IMiddleware2MiddlewareCommunicatReply;
        }
        public void SendMiddlewareMsgOprRet(out bool ret, out string errorDetail)
        {
            if (EnumServControlPkgCurrentdutyType.Middleware2MiddlewareCommunicat == _pkgCurrentdutyType)
            {
                byte bytMiddlewareMsgSendRetCodec = _rawData[0];
                if ((byte)ClientHeadCodec.P2pTransport_ReplySucess == bytMiddlewareMsgSendRetCodec)
                {
                    _middlewarePkgTransferServOprRet = true;
                    ret = true;
                    errorDetail = string.Empty;
                }
                else
                {
                    _middlewarePkgTransferServOprRet = false;
                    ret = false;
                    byte bytFaildCodec = _rawData[1];
                    switch (bytFaildCodec)
                    {
                        case (byte)P2pTransportFaildCodec.Target_UNCONNECT:
                            {
                                errorDetail = P2pTransportFaildCodec.Target_UNCONNECT.ToString();
                                break;
                            }
                        case (byte)P2pTransportFaildCodec.Token_UNPARSE:
                            {
                                errorDetail = P2pTransportFaildCodec.Token_UNPARSE.ToString();
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("无效的P2pTransportFaildCodec");
                            }
                    }
                }
            }
            else
            {
                throw new Exception("不是一份合法的Middleware2MiddlewareCommunicat响应报文, 阻止调用");
            }
        }

        internal bool MiddlewarePkgTransferServOprRet
        {
            get { return _middlewarePkgTransferServOprRet; }
        }
        private bool _middlewarePkgTransferServOprRet = false;
        #endregion

        #region ICreateGroupReply
        public ICreateGroupReply Active_CreateGroupReply()
        {
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.CreateGroup;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;
            return this as ICreateGroupReply;
        }
        public void CreateGroupRet(out bool ret, out string erroDetailOrToken)
        {
            byte bytCreateGropRetCodec = _rawData[0];
            if ((byte)ClientHeadCodec.CreateGroup_ReplySucess == bytCreateGropRetCodec)
            {
                ret = true;
                byte[] bytToken = new byte[ConstData.tokenLength];
                Buffer.BlockCopy(_rawData, 1, bytToken, 0, ConstData.tokenLength);
                erroDetailOrToken = Encoding.ASCII.GetString(bytToken);
            }
            else
            {
                ret = false;
                byte bytCreatGropFaildCodec = _rawData[1];
                switch (bytCreatGropFaildCodec)
                {
                    case (byte)CreateGroupFaildCodec.Detail_REPEAT:
                        {
                            erroDetailOrToken = CreateGroupFaildCodec.Detail_REPEAT.ToString();
                            break;
                        }
                    default:
                        {
                            throw new ArgumentException("无效的CreateGroupFaildCodec");
                        }
                }
            }
        }
        #endregion

        #region IJoinGroupReply
        public IJoinGroupReply Active_JoinGroupReply()
        {
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.JoinGroup;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;
            return this as IJoinGroupReply;
        }
        public void JoinGroupRet(out bool ret, out string erroDetail)
        {
            byte bytJoinGroupRetCodc = _rawData[0];
            if ((byte)ClientHeadCodec.JoinGroup_ReplySucess == bytJoinGroupRetCodc)
            {
                ret = true;
                erroDetail = string.Empty;
            }
            else
            {
                ret = false;
                byte bytJoinGropFaildCodc = _rawData[1];
                switch (bytJoinGropFaildCodc)
                {
                    case (byte)JoinGroupFaildCodec.Client_REPEAT:
                        {
                            erroDetail = JoinGroupFaildCodec.Client_REPEAT.ToString();
                            break;
                        }
                    case (byte)JoinGroupFaildCodec.Group_INEXISTANCE:
                        {
                            erroDetail = JoinGroupFaildCodec.Group_INEXISTANCE.ToString();
                            break;
                        }
                    default:
                        {
                            throw new ArgumentException("无效的JoinGroupFaildCodec");
                        }
                }
            }
        }
        #endregion

        #region IExitGroupRely
        public IExitGroupRely Active_ExitGroupReply()
        {
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.ExitGroup;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;
            return this as IExitGroupRely;
        }
        public void ExitGroupRet(out bool ret, out string errorDetail)
        {
            byte bytExitGropRetCodc = _rawData[0];
            if ((byte)ClientHeadCodec.ExitGroup_ReplySucess == bytExitGropRetCodc)
            {
                ret = true;
                errorDetail = string.Empty;
            }
            else
            {
                ret = false;
                byte bytExitGropFaildCodec = _rawData[1];
                switch (bytExitGropFaildCodec)
                {
                    case (byte)ExitGroupFaildCodec.Client_UNBELONG:
                        {
                            errorDetail = ExitGroupFaildCodec.Client_UNBELONG.ToString();
                            break;
                        }
                    case (byte)ExitGroupFaildCodec.Group_INEXISTANCE:
                        {
                            errorDetail = ExitGroupFaildCodec.Group_INEXISTANCE.ToString();
                            break;
                        }
                    default:
                        {
                            throw new ArgumentException("无效的ExitGroupFaildCodec");
                        }
                }
            }
        }
        #endregion

        #region RadioRet
        public IRadioReply Active_RadioGroupReply()
        {
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.RadioTransport;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;
            return this as IRadioReply;
        }
        public void RadioRet(out bool ret, out string errorDetail)
        {
            byte bytRadioRetCodec = _rawData[0];
            if ((byte)ClientHeadCodec.RadioTransport_ReplySucess == bytRadioRetCodec)
            {
                ret = true;
                errorDetail = string.Empty;
            }
            else
            {
                ret = false;
                byte bytRadioFaildCodec = _rawData[1];
                switch (bytRadioFaildCodec)
                {
                    case (byte)RadioTransportFaildCodec.Client_UNBELONG:
                        {
                            errorDetail = RadioTransportFaildCodec.Client_UNBELONG.ToString();
                            break;
                        }
                    case (byte)RadioTransportFaildCodec.Group_INEXISTANCE:
                        {
                            errorDetail = RadioTransportFaildCodec.Group_INEXISTANCE.ToString();
                            break;
                        }
                    case (byte)RadioTransportFaildCodec.Trans_FORBIDDEN:
                        {
                            errorDetail = RadioTransportFaildCodec.Trans_FORBIDDEN.ToString();
                            break;
                        }
                    default:
                        {
                            throw new ArgumentException("无效的RadioTransportFaildCodec");
                        }
                }
            }
        }
        #endregion

        #region IOnlineGroupReply
        public IOnlineGroupReply Active_OnlineGroupReply()
        {
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.GroupOnlineList;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;
            return this as IOnlineGroupReply;
        }
        public void OnlineGroupRet(out bool ret, out string errorDetail, out List<CSCommunicateClass.GroupInfo> gropInfoList)
        {
            byte bytOnlineGropRetCodec = _rawData[0];
            if ((byte)ClientHeadCodec.GetOnlineGroup_ReplySucess == bytOnlineGropRetCodec)
            {
                ret = true;
                errorDetail = string.Empty;

                byte[] bytGropsInfoJson = new byte[_rawData.Length - 1];
                Buffer.BlockCopy(_rawData, 1, bytGropsInfoJson, 0, bytGropsInfoJson.Length);
                gropInfoList = JsonSerializtionFactory.DeJSON<CSCommunicateClass.GroupInfo>(bytGropsInfoJson);
            }
            else
            {
                ret = false;
                errorDetail = string.Empty;  //no detail
                gropInfoList = null;
            }
        }
        #endregion

        #region IOnlineGroupClientReply
        public IOnlineGroupClientReply Active_OnlineGroupClientReply()
        {
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.GroupClientOnlineList;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;
            return this as IOnlineGroupClientReply;
        }
        public void OnlineGroupClientRet(out bool ret, out string errorDetail, out List<CSCommunicateClass.ClientInfo> gropClientInfoList)
        {
            byte bytGetOnlineGroupClientRetCodec = _rawData[0];
            if ((byte)ClientHeadCodec.GetGroupClient_ReplySucess == bytGetOnlineGroupClientRetCodec)
            {
                ret = true;
                errorDetail = string.Empty;

                byte[] bytGropClientInfoJson = new byte[_rawData.Length - 1];
                Buffer.BlockCopy(_rawData, 1, bytGropClientInfoJson, 0, bytGropClientInfoJson.Length);
                gropClientInfoList = JsonSerializtionFactory.DeJSON<CSCommunicateClass.ClientInfo>(bytGropClientInfoJson);
            }
            else
            {
                ret = false;
                gropClientInfoList = null;
                byte bytGetOnlineClientFaildCodec = _rawData[1];
                switch (bytGetOnlineGroupClientRetCodec)
                {
                    case (byte)GetGoupClientFaildCodec.Client_UNBELONG:
                        {
                            errorDetail = GetGoupClientFaildCodec.Client_UNBELONG.ToString();
                            break;
                        }
                    case (byte)GetGoupClientFaildCodec.Group_INEXISTENCE:
                        {
                            errorDetail = GetGoupClientFaildCodec.Group_INEXISTENCE.ToString();
                            break;
                        }
                    default:
                        {
                            throw new ArgumentException("无效的GetGoupClientFaildCodec");
                        }
                }
            }
        }
        #endregion

        #region IClientCancleReply
        public IClientCancleReply Active_ClientCancleReply()
        {
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.ClientCancle;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.CoreLogicLayer;
            return this as IClientCancleReply;
        }
        public void ClientCancleRet(out bool ret)
        {
            byte bytClientCancleRet = _rawData[0];
            if ((byte)ClientHeadCodec.ClientCancle_ReplySucess == bytClientCancleRet)
            {
                ret = true;
            }
            else
            {
                ret = false;
            }
        }
        #endregion

        #region IMiddlewareMessageIncomming
        //TODO
        public IMiddlewareMessageIncomming Active_MiddlewareMessageIncomming()
        {
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.MiddlewareMsgIncomming;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.MiddlewareCommunicateLayer;
            return this as IMiddlewareMessageIncomming;
        }
        public MiddlewareTransferPackage MTPkg
        {
            get
            {
                if (null == _MTpkg)
                {
                    byte[] bytDeviceToken = new byte[ConstData.tokenLength];
                    Buffer.BlockCopy(_rawData, 1, bytDeviceToken, 0, ConstData.tokenLength);
                    _sourMiddleDeviceToken = Encoding.ASCII.GetString(bytDeviceToken);

                    byte[] bytObjectContent = new byte[_rawData.Length - (1 + ConstData.tokenLength)];
                    Buffer.BlockCopy(_rawData, 1 + ConstData.tokenLength, bytObjectContent, 0, bytObjectContent.Length);
                    _MTpkg = (new MiddlewareTransferPackage()).DeserializeMessage(bytObjectContent);
                }
                return _MTpkg;
            }
        }
        protected string _sourMiddleDeviceToken = null;
        protected MiddlewareTransferPackage _MTpkg = null;
        #endregion

        #region IRadioMessageIncomming
        public IRadioMessageIncomming Active_RadioMessageIncomming()
        {
            _pkgCurrentdutyType = EnumServControlPkgCurrentdutyType.GroupMsgIncomming;
            _pkgWorkingLayer = EnumServControlPkgBelongLayerType.GroupCommunicateLayer;
            return this as IRadioMessageIncomming;
        }
        public C2CRadioPackage RadioPkg
        {
            get
            {
                if (null == _radioPkg)
                {
                    byte[] bytGropToken = new byte[ConstData.tokenLength];
                    Buffer.BlockCopy(_rawData, 1, bytGropToken, 0, ConstData.tokenLength);
                    _gropToken = Encoding.ASCII.GetString(bytGropToken);

                    byte[] bytMiddlewareToken = new byte[ConstData.tokenLength];
                    Buffer.BlockCopy(_rawData, 1 + ConstData.tokenLength, bytMiddlewareToken, 0, ConstData.tokenLength);
                    _middlewareDeviceToken = Encoding.ASCII.GetString(bytMiddlewareToken);

                    byte[] bytObjectContent = new byte[_rawData.Length - (1 + 2 * ConstData.tokenLength)];
                    Buffer.BlockCopy(_rawData, 1 + 2 * ConstData.tokenLength, bytObjectContent, 0, bytObjectContent.Length);
                    _radioPkg = RadioPackage.Empty.DeserializeMessage(bytObjectContent) as C2CRadioPackage;
                }
                return _radioPkg;
            }
        }
        protected string _gropToken = null;
        protected string _middlewareDeviceToken = null;
        protected C2CRadioPackage _radioPkg = null;
        #endregion
        #endregion
    }

    /// <summary>
    /// 服务端请求类
    /// </summary>
    internal class ServerRequestPackage : ServerCtrRequestPackage
    {
        internal ServerRequestPackage(bool waittingResponse)
            : base(waittingResponse)
        {
            InitID(null, null);
        }
        internal ServerRequestPackage(bool waittingResponse, string id)
            : base(waittingResponse)
        {
            InitID(id, null);
        }
        internal ServerRequestPackage(bool waittingResponse, string id, string replyid)
            : base(waittingResponse)
        {
            InitID(id, replyid);
        }

        void InitID(string id, string replyid)
        {
            if (false == string.IsNullOrEmpty(id))
            {
                _id = id;
            }
            _replyid = replyid;
        }

        public string RepliedMessageId
        {
            get { return _replyid; }
            set { _replyid = value; }
        }
        public string MessageId
        {
            get { return _id; }
        }

        private string _id = System.Guid.NewGuid().ToString();
        private string _replyid = null;
    }

    /// <summary>
    /// 服务端应答包装类
    /// </summary>
    internal class ServerReplyPackage : ServerCtrReplyPackage
    {
        internal ServerReplyPackage(byte[] bytMsg, string repyid, string id)
            : base(bytMsg)
        {
            _replyid = repyid;
            _id = id;
        }

        public string MessageId
        {
            get { return _id; }
            set { _id = value; }
        }
        public string RepliedMessageId
        {
            get { return _replyid; }
            set { _replyid = value; }
        }

        private string _id = System.Guid.NewGuid().ToString();
        private string _replyid = null;
    }
    #endregion

    internal class BinTransferLayer
    {
        public BinTransferLayer(MiddlewareCorelogicLayer middlewareCorelogicLayerProcessor)
        {
            _middlewareCorelogicLayerProcessor = middlewareCorelogicLayerProcessor;
        }

        /// <summary>
        /// This event is raised when client connected to server.
        /// </summary>
        internal event EventHandler Connected;

        /// <summary>
        /// This event is raised when client disconnected from server.
        /// </summary>
        internal event EventHandler Disconnected;

        internal MiddlewareErrorInfo Initialization(MiddlewareTcpEndPoint tcpEndPoint)
        {
            if ((false == _bIsInited) && (false == _bIsRunning))
            {
                _incommingScsMessageNotifier = new SequentialItemProcessor<IScsMessage>(this.__ServerMsgNotify);
                _asynWaittingBinReplyMessages.Clear();
                _tcpClient = ScsClientFactory.CreateClient(tcpEndPoint.TcpEndPoint);
                _tcpClient.WireProtocol = new MyUnderScsWireProtocol();

                _serverReplyMessenger = new RequestReplyMessenger<IScsClient>(_tcpClient);
                _serverReplyMessenger.MessageReceived += this.__RawMessageRecived;
                _serverReplyMessenger.Start();

                _tcpClient.Connect();

                //set notify outside
                _tcpClient.Connected += this.Connected;
                _tcpClient.Disconnected += this.Disconnected;

                //set relinker
                _relinker = new ClientReConnecter(_tcpClient);

                _bIsInited = true;
            }
            else
            {
                throw new Exception("Framework.SCS 调用层 / 服务器协议解析层 已经初始化");
            }
            return MiddlewareErrorInfo.S_OK;
        }
        internal void Start()
        {
            if ((true == _bIsInited) && (false == _bIsRunning))
            {
                try
                {
                    _incommingScsMessageNotifier.Start();
                    _bIsRunning = true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                if (false == _bIsInited)
                    throw new Exception("Framework.SCS 调用层 / 服务器协议解析层 未初始化");

                if (true == _bIsRunning)
                    throw new Exception("Framework.SCS 调用层 / 服务器协议解析层 已经运行");
            }
        }
        internal void Stop()
        {
            if ((true == _bIsInited) && (true == _bIsRunning))
            {
                _incommingScsMessageNotifier.Stop();
                _asynWaittingBinReplyMessages.Clear();
                _bIsRunning = false;
            }
            else
            {
                if (false == _bIsInited)
                    throw new Exception("Framework.SCS 调用层 / 服务器协议解析层 未初始化");

                if (false == _bIsRunning)
                    throw new Exception("Framework.SCS 调用层 / 服务器协议解析层 未运行");
            }
        }
        internal void Dispose()
        {
            if ((true == _bIsInited) && (false == _bIsRunning))
            {
                //logout
                ServerRequestPackage reqtPkg = new ServerRequestPackage(true);
                IClientCancleRequest reqtInterfacce = reqtPkg.Active_ClientCancleRequest();
                reqtInterfacce.ClientCancle();
                ServerReplyPackage replyPkg = null;
                try
                {
                    replyPkg = this.SynSendMessage(reqtPkg, 100000);
                }
                catch (System.Exception ex)
                {
                	//忽略异常，强退
                }
                _middlewareCorelogicLayerProcessor.Online = false;

                _serverReplyMessenger.Stop();
                _tcpClient.Disconnect();

                _serverReplyMessenger.MessageReceived -= this.__RawMessageRecived;
                _serverReplyMessenger = null;
                _tcpClient = null;

                _bIsInited = false;
            }
            else
            {
                if (false == _bIsInited)
                    throw new Exception("Framework.SCS 调用层 / 服务器协议解析层 未进行初始化");
                if (true == _bIsRunning)
                    throw new Exception("Framework.SCS 调用层 / 服务器协议解析层 处在运行态");
            }
        }

        /// <summary>
        /// 阻塞式执行一次对服务器发送
        /// </summary>
        /// <param name="reqtPkg">向服务器发送的报文包</param>
        /// <returns>服务器响应报文包</returns>
        internal ServerReplyPackage SynSendMessage(ServerRequestPackage reqtPkg, int timeoutMilliseconds)
        {
            ScsRawDataMessage wilSendScsMessage = new ScsRawDataMessage(reqtPkg.Export());
            wilSendScsMessage.MessageId = reqtPkg.MessageId;
            IScsMessage respScsMsg;
            try
            {
                respScsMsg = _serverReplyMessenger.SendMessageAndWaitForResponse(wilSendScsMessage, timeoutMilliseconds);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            ServerReplyPackage srPkg = new ServerReplyPackage(((ScsRawDataMessage)respScsMsg).MessageData,
                                                                                        respScsMsg.MessageId,
                                                                                        respScsMsg.RepliedMessageId);
            return srPkg;
        }
        internal void AsynSendMseeage(ServerRequestPackage reqtPkg)
        {
            ScsRawDataMessage wilSendScsMessage = new ScsRawDataMessage(reqtPkg.Export());
            wilSendScsMessage.MessageId = reqtPkg.MessageId;
            if (reqtPkg.WattingResponse)
            {
                lock (_synWriteLockObject)
                {
                    __ActiveBinMessageWattinghere(reqtPkg);
                }
            }
            try
            {
                _serverReplyMessenger.SendMessage(wilSendScsMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void __ActiveBinMessageWattinghere(ServerRequestPackage reqtPkg)
        {
            __ActiveBinMessageWattinghere(reqtPkg, -1);
        }
        private void __ActiveBinMessageWattinghere(ServerRequestPackage reqtPkg, int time)
        {
            //TODO
            //设计超时计时器
            _asynWaittingBinReplyMessages[reqtPkg.MessageId] = reqtPkg;
        }
        private void __RawMessageRecived(object sender, MessageEventArgs e)
        {
            if (sender is RequestReplyMessenger<IScsClient>)
            {
                ScsRawDataMessage replyServerScsMsg = e.Message as ScsRawDataMessage;
                if (null != replyServerScsMsg)
                {
                    //向中间件传输层推送
                    //_incommingScsMessageNotifier.EnqueueMessage(e.Message);
                    SequentialItemProcessor<IScsMessage> simultaneousNotifyRunner = new SequentialItemProcessor<IScsMessage>(this.__ServerMsgNotify);
                    simultaneousNotifyRunner.Start();
                    simultaneousNotifyRunner.EnqueueMessage(e.Message);
                }
                else
                {
                    throw new Exception("底层消息解析遇到未支持的报文格式");
                }
            }
            else
            {
                throw new Exception("未知来源的字节层消息");
            }
        }
        private void __ServerMsgNotify(IScsMessage msg)
        {
            ServerReplyPackage replyServerMsg = new ServerReplyPackage(((ScsRawDataMessage)msg).MessageData,
                                                                                    msg.RepliedMessageId,
                                                                                    msg.MessageId);

            string replyMsgId = msg.RepliedMessageId;
            if (false == string.IsNullOrEmpty(replyMsgId))
            {
                bool bIsWaittingForTheMsg = false;
                if (false == string.IsNullOrEmpty(replyMsgId))
                {
                    lock (_synReadLockObject)
                    {
                        bIsWaittingForTheMsg = _asynWaittingBinReplyMessages.ContainsKey(replyMsgId);
                    }
                }
                //response
                if (bIsWaittingForTheMsg)
                {
                    if (EnumServControlPkgBelongLayerType.MiddlewareCommunicateLayer == replyServerMsg.WorkingLayer)
                    {
                        //notify mieddleware layer
                        //MiddlewareCommunicateLayer mclprocessor = _middlewareCorelogicLayerProcessor.MiddlewareCommunicateLayer;
                        //mclprocessor.ServerMessageRecived(_asynWaittingBinReplyMessages[replyMsgId] as ServerRequestPackage,
                        //                                                                                replyServerMsg);

                        //该层消息包的通讯目前全部以同步实现，执行到此分支将是一个错误
                        throw new NotImplementedException("MiddlewareCommunicateLayer消息包的通讯目前全部以同步实现，执行到此分支将是一个错误");
                    }
                    if (EnumServControlPkgBelongLayerType.CoreLogicLayer == replyServerMsg.WorkingLayer)
                    {
                        //notify corelogic layer
                        //_middlewareCorelogicLayerProcessor.ServerMessageRecived(_asynWaittingBinReplyMessages[replyMsgId] as ServerRequestPackage,
                        //                                                                                replyServerMsg);

                        //该层消息包的通讯目前全部以同步实现，执行到此分支将是一个错误
                        throw new NotImplementedException("CoreLogicLayer消息包的通讯目前全部以同步实现，执行到此分支将是一个错误");
                    }
                    if (EnumServControlPkgBelongLayerType.GroupCommunicateLayer == replyServerMsg.WorkingLayer)
                    {
                        //TODO
                        //该层消息包的通讯目前全部以同步实现，执行到此分支将是一个错误
                        throw new NotImplementedException("GroupCommunicateLayer消息包的通讯目前全部以同步实现，执行到此分支将是一个错误");
                    }
                    //Remove Record
                    _asynWaittingBinReplyMessages.Remove(replyMsgId);
                }
                else
                {
                    //give up the message
                }
            }
            else
            {
                //unknow message
                if (EnumServControlPkgBelongLayerType.MiddlewareCommunicateLayer == replyServerMsg.WorkingLayer)
                {
                    //notify mieddleware layer
                    MiddlewareCommunicateLayer mclprocessor = _middlewareCorelogicLayerProcessor.MiddlewareCommunicateLayer;
                    mclprocessor.ServerMessageRecived(replyServerMsg);
                }
                if (EnumServControlPkgBelongLayerType.CoreLogicLayer == replyServerMsg.WorkingLayer)
                {
                    //notify corelogic layer
                    //_middlewareCorelogicLayerProcessor.ServerMessageRecived(replyServerMsg);

                    //CoreLogicLayer消息包的通讯目前全部以Q/A模式对服务器进行通信，未知消息执行到此分支将是一个错误"
                    throw new NotImplementedException("CoreLogicLayer消息包的通讯目前全部以Q/A模式对服务器进行通信，未知消息执行到此分支将是一个错误");
                }
                if (EnumServControlPkgBelongLayerType.GroupCommunicateLayer == replyServerMsg.WorkingLayer)
                {
                    //notify group layer
                    GroupCommunicateLayer gcprocessor = _middlewareCorelogicLayerProcessor.GroupCommunicateProcessor;
                    gcprocessor.ServerMessageRecived(replyServerMsg);
                }
            }
        }

        private object _synWriteLockObject = new object();
        private object _synReadLockObject = new object();
        private Hashtable _asynWaittingBinReplyMessages = new Hashtable();
        private IScsClient _tcpClient = null;
        private RequestReplyMessenger<IScsClient> _serverReplyMessenger = null;
        private SequentialItemProcessor<IScsMessage> _incommingScsMessageNotifier = null;

        private ClientReConnecter _relinker = null;

        private bool _bIsInited = false;
        private bool _bIsRunning = false;

        private MiddlewareCorelogicLayer _middlewareCorelogicLayerProcessor = null;
    }

    #endregion
}