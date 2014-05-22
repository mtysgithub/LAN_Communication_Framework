using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

using ProtoBuf.Meta;

using Hik.Communication.Scs;
using Hik.Communication;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Threading;
using Hik.Collections;

using ProtocolLibrary.CSProtocol.CommonConfig;
using ProtocolLibrary.CSProtocol.CommonConfig.ClientMsgCodecSpace;

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
using Middleware.Communication.Excetion;
using Middleware.Communication.Message;
using Middleware.Communication.Package.CommunicatePackage;
using Middleware.LayerProcessor.Interfcace;

namespace Middleware.LayerProcessor
{
    #region 核心逻辑层
    public class MiddlewareCorelogicLayer
    {
        internal MiddlewareCorelogicLayer()
        {
            _binProcessferProcessor = new BinTransferLayer(this);
            _middlewareCommunicateProcessor = new MiddlewareCommunicateLayer(this);
            _groupCommunicateProcessor = new GroupCommunicateLayer(this);
            mMiddlewareMessenger = MiddlewareMessenger.Instance;

            _asynSendRequestRunner = new SequentialItemProcessor<RequestCommunicatePackage>(this.__CoAsynSendRequestRunning);
            _asynFeedbackCommunicateReplyMessageRunner = new SequentialItemProcessor<ReplyCommunicatePackage>(this.__AsynFeedbackCommunicateReplyMessageRunning);
            _asynGetGroupRunner = new SequentialItemProcessor<KeyValuePair<string, GroupDevice>>(this.__AsynCoGetGroupRunning);
            _asynJoinGroupRunner = new SequentialItemProcessor<KeyValuePair<GroupDevice, GroupMemberRole>>(this.__CoAsynJoinGroupRunning);
            _asynExitGroupRunner = new SequentialItemProcessor<GroupDevice>(this.__CoAsynExitGroupRunning);
            _asynRadioRunner = new SequentialItemProcessor<GroupComunicatePackage>(this.__CoAsynRadioRunning);
            _asynCreategroupRunner = new SequentialItemProcessor<KeyValuePair<string, GroupDevice>>(this.__CoAsynCreateGroupRunning);
        }

        #region Control
        protected void CoStart()
        {
            _binProcessferProcessor.Start();
            _middlewareCommunicateProcessor.Start();
            _groupCommunicateProcessor.Start();
            mMiddlewareMessenger.Initialize(this, _groupCommunicateProcessor, _middlewareCommunicateProcessor, CoMessageRecived_OutisdeNotify);

            _asynSendRequestRunner.Start();
            _asynFeedbackCommunicateReplyMessageRunner.Start();
            _asynGetGroupRunner.Start();
            _asynJoinGroupRunner.Start();
            _asynExitGroupRunner.Start();
            _asynRadioRunner.Start();
            _asynCreategroupRunner.Start();
        }
        protected void CoStop()
        {
            _asynCreategroupRunner.Stop();
            _asynRadioRunner.Stop();
            _asynExitGroupRunner.Stop();
            _asynJoinGroupRunner.Stop();
            _asynGetGroupRunner.Stop();
            _asynFeedbackCommunicateReplyMessageRunner.Stop();
            _asynSendRequestRunner.Stop();

            mMiddlewareMessenger.Release();
            _groupCommunicateProcessor.Stop();
            _middlewareCommunicateProcessor.Stop();
            _binProcessferProcessor.Stop();

            _localGroupDeviceBuffer.Clear();
        }

        public virtual void Dispose()
        {
            _groupCommunicateProcessor.Dispose();
            _middlewareCommunicateProcessor.Dispose();
            _binProcessferProcessor.Dispose();
        }
        #endregion

        #region RunningTime Control
        protected void CoNewClientInitialization(MiddlewareTcpEndPoint endPoint, string detail, List<string> oprRules, List<string> opredRules)
        {
            string token = null;
            _binProcessferProcessor.Initialization(endPoint);
            token = __FirstHandShake(detail);

            _middlewareCommunicateProcessor.Initialization();
            _groupCommunicateProcessor.Initialization();

            _binProcessferProcessor.Connected += __BinLayerConnectedHandler;
            _binProcessferProcessor.Disconnected += __BinLayerDisConnectedHandler;

            _selfDevice = new ClientDevice(token, detail);
            _bFirstLinkSucessful = true;
            _bIsOnline = true;
        }

        protected void CoOldClientInitialization(MiddlewareTcpEndPoint endPoint, string detail, string token, List<string> oprRules, List<string> opredRules)
        {
            _binProcessferProcessor.Initialization(endPoint);
            try
            {
                __Relink(token);
            }
            catch (System.Exception ex)
            {
                throw new InitializationExtion(ex.ToString());
            }
            _middlewareCommunicateProcessor.Initialization();
            _groupCommunicateProcessor.Initialization();

            _binProcessferProcessor.Connected += __BinLayerConnectedHandler;
            _binProcessferProcessor.Disconnected += __BinLayerDisConnectedHandler;

            _selfDevice = new ClientDevice(token, detail);
            _bFirstLinkSucessful = true;
            _bIsOnline = true;
        }

        private string __FirstHandShake(string detail)
        {
            //配置请求包
            ServerRequestPackage reqtPkg = new ServerRequestPackage(true);
            IMiddleware2ServerHandShakeRequest reqtInterface = reqtPkg.Active_Middleware2ServerHandShakeRequest();
            reqtInterface.HandShake(false, detail);
            ServerReplyPackage replyPkg = null;
            replyPkg = _binProcessferProcessor.SynSendMessage(reqtPkg, 100000);

            IMiddleware2ServerHandShakeReply replyInterface = replyPkg.Active_MiddlewareHandShakeReply();
            bool ret;
            string tokenOrErrorInfo;
            replyInterface.HandShakeRet(out ret, out tokenOrErrorInfo);
            if (true == ret)
            {
                return tokenOrErrorInfo;
            }
            else
            {
                throw new InitializationExtion(tokenOrErrorInfo);
            }
        }

        private void __Relink(string token)
        {
            //配置请求包
            ServerRequestPackage reqtPkg = new ServerRequestPackage(true);
            IMiddleware2ServerHandShakeRequest reqtInterface = reqtPkg.Active_Middleware2ServerHandShakeRequest();
            reqtInterface.HandShake(true, token);

            ServerReplyPackage replyPkg = null;
            try
            {
                replyPkg = _binProcessferProcessor.SynSendMessage(reqtPkg, 100000);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            bool ret;
            string errorDetail;
            IMiddleware2ServerHandShakeReply replyInterface = replyPkg.Active_MiddlewareHandShakeReply();
            replyInterface.HandShakeRet(out ret, out errorDetail);
            if (true == ret)
            {
                //nothing to do
            }
            else
            {
                throw new InitializationExtion(errorDetail);
            }
        }

        private void __BinLayerConnectedHandler(object sender, EventArgs evtArgs)
        {
            if ((false == _bIsOnline) && (true == _bFirstLinkSucessful))
            {
                bool isRelinked = false;
                while (!isRelinked)
                {
                    try
                    {
                        //守护实现
                        __Relink(_selfDevice.Token);
                        isRelinked = true;
                    }
                    catch (System.Exception ex)
                    {
                        
                    }
                }
                _bIsOnline = true;
            }
            else
            {
                if (false == _bFirstLinkSucessful)
                {
                    //允许到此分支将是一个逻辑错误
                    throw new Exception("严重错误：在尝试重链接时发现未曾向服务器注册");
                }
            }
        }
        private void __BinLayerDisConnectedHandler(object sender, EventArgs evtArgs)
        {
            _bIsOnline = false;
        }
        #endregion

        #region C2C

        #region CreateRequestCommunicatePackage
        protected RequestCommunicatePackage CoCreateRequestCommunicatePackage(string communicationName,
                                                                        CommunicatType communicateType,
                                                                        ParamPackage reqtParamPkg,
                                                                        ClientDevice targetDevice,
                                                                        bool waitResponse,
                                                                        AsynReponseHandler callback)
        {
            RequestCommunicatePackage reqtPkg = new RequestCommunicatePackage();
            reqtPkg.CommunicationName = communicationName;
            reqtPkg.CommunicateType = communicateType;
            reqtPkg.ParamPackage = reqtParamPkg;
            reqtPkg.TargetDevice = targetDevice;
            reqtPkg.WaitResponse = waitResponse;
            reqtPkg.AsynchronousReponseCame = callback;
            return reqtPkg;
        }
        #endregion

        #region SendRequest
        protected void CoAsynSendRequest(RequestCommunicatePackage communicator)
        {
            _asynSendRequestRunner.EnqueueMessage(communicator);
        }

        private void __CoAsynSendRequestRunning(RequestCommunicatePackage communicator)
        {
            RequestPackage reqtPkg = new RequestPackage(_selfDevice,
                                                                                communicator.CommunicationName,
                                                                                communicator.WaitResponse,
                                                                                communicator.ParamPackage.ParamDefalutValues);

            C2CRequestPackage c2cReqtPkg = new C2CRequestPackage(_selfDevice,
                                                                                                "outside message sent",
                                                                                                communicator.WaitResponse,
                                                                                                null);
            c2cReqtPkg.OutSideMessage = reqtPkg;
            RequestMTPackage mtReqtPkg = new RequestMTPackage(c2cReqtPkg,
                                                                                    _selfDevice,
                                                                                    communicator.TargetDevice,
                                                                                    c2cReqtPkg.WaittingResponse);
            if (communicator.WaitResponse)
            {
                MiddlewareTransferPackage mtReplyBasePkg = null;
                try
                {
                    //100s limited time to wait this c2c response.
                    mtReplyBasePkg = _middlewareCommunicateProcessor.SynSendMessage(mtReqtPkg, 10000);
                }
                catch (System.Exception ex)
                {
                    //throw new MiddlewareCommunicatErrorExcetion(ex.ToString());
                    this.CoMiddleware2MiddlewareAsynReqtCommunicatErrorRecived_OutsideNotify(communicator,
                                                                                                    new MiddlewareCommunicatErrorExcetion(ex.ToString()));
                }
                if (null != (mtReplyBasePkg as ReplyMTPackage))
                {
                    ReplyMTPackage mtReplyPkg = mtReplyBasePkg as ReplyMTPackage;
                    C2CReplyPackage c2cReplyPkg = mtReplyPkg.C2CReplyPackage;
                    if ((null != c2cReplyPkg) && (null != c2cReplyPkg.OutSideMessage))
                    {
                        ReplyPackage outsideReplyPkg = c2cReplyPkg.OutSideMessage;
                        communicator.ResponsePackage = outsideReplyPkg;
                        //asyn outside notify
                        if (null != communicator.AsynchronousReponseCame)
                        {
                            communicator.AsynchronousReponseCame(communicator, new AsynReplyCommunicatePackage(outsideReplyPkg));
                        }
                    }
                    else
                    {
                        ArgumentNullException ex = new ArgumentNullException("应答报文C2CReplyPackage/C2CReplyPackage.OutSideMessage字段空");
                        this.CoMiddleware2MiddlewareAsynReqtCommunicatErrorRecived_OutsideNotify(communicator,
                                                                                                        new MiddlewareCommunicatErrorExcetion(ex.ToString()));
                    }
                }
                else
                {
                    NotImplementedException ex = new NotImplementedException("暂不支持打回RequestMTPackage类型");
                    this.CoMiddleware2MiddlewareAsynReqtCommunicatErrorRecived_OutsideNotify(communicator,
                                                                                                        new MiddlewareCommunicatErrorExcetion(ex.ToString()));
                }
            }
            else
            {
                try
                {
                    _middlewareCommunicateProcessor.AsynSendMessage(mtReqtPkg);
                }
                catch (System.Exception ex)
                {
                    //throw new MiddlewareCommunicatErrorExcetion(ex.ToString());
                    this.CoMiddleware2MiddlewareAsynReqtCommunicatErrorRecived_OutsideNotify(communicator,
                                                                                    new MiddlewareCommunicatErrorExcetion(ex.ToString()));
                }
            }
        }

        protected void CoSynSendRequest(RequestCommunicatePackage communicator, int timeMillionsecond)
        {
            if (communicator.WaitResponse)
            {
                RequestPackage reqtPkg = new RequestPackage(_selfDevice,
                                                        communicator.CommunicationName,
                                                        communicator.WaitResponse,
                                                        communicator.ParamPackage.ParamDefalutValues);

                C2CRequestPackage c2cReqtPkg = new C2CRequestPackage(_selfDevice,
                                                                                                    "outside message sent",
                                                                                                    communicator.WaitResponse,
                                                                                                    null);
                c2cReqtPkg.OutSideMessage = reqtPkg;
                RequestMTPackage mtReqtPkg = new RequestMTPackage(c2cReqtPkg,
                                                                                        _selfDevice,
                                                                                        communicator.TargetDevice,
                                                                                        c2cReqtPkg.WaittingResponse);
                MiddlewareTransferPackage mtReplyBasePkg = null;

                //set param-time to wait this c2c response.
                mtReplyBasePkg = _middlewareCommunicateProcessor.SynSendMessage(mtReqtPkg, timeMillionsecond);

                if (null != (mtReplyBasePkg as ReplyMTPackage))
                {
                    ReplyMTPackage mtReplyPkg = mtReplyBasePkg as ReplyMTPackage;
                    C2CReplyPackage c2cReplyPkg = mtReplyPkg.C2CReplyPackage;
                    if ((null != c2cReplyPkg) && (null != c2cReplyPkg.OutSideMessage))
                    {
                        ReplyPackage outsideReplyPkg = c2cReplyPkg.OutSideMessage;
                        communicator.ResponsePackage = outsideReplyPkg;
                    }
                    else
                    {
                        throw new ArgumentNullException("应答报文C2CReplyPackage/C2CReplyPackage.OutSideMessage字段空");
                    }
                }
                else
                {
                    throw new NotImplementedException("暂不支持打回RequestMTPackage类型");
                }
            }
            else
            {
                throw new MiddlewareCommunicatErrorExcetion("同步调用而不请求应答是一个错误");
            }
        }

        protected event AsynCommunicatErrorHandler CoMiddleware2MiddlewareAsynReqtCommunicatErrorRecived_OutsideNotify = null;

        private SequentialItemProcessor<RequestCommunicatePackage> _asynSendRequestRunner = null;
        #endregion

        #region CreateReplyCommunicatePackage
        protected ReplyCommunicatePackage CoCreateReplyCommunicatePackage(RequestPackage reqtPkg, ReplyPackage.Middleware_ReplyInfo statCodec, ParamPackage paramspkg)
        {
            ReplyCommunicatePackage replyCommunicatePkg = new ReplyCommunicatePackage();

            replyCommunicatePkg.RemotReqtPkg = reqtPkg;
            replyCommunicatePkg.RemotRet = statCodec;
            replyCommunicatePkg.ParamPackage = paramspkg;

            return replyCommunicatePkg;
        }
        #endregion

        #region FeedbackCommunicateReplyMessage
        protected void CoSynFeedbackCommunicateReplyMessage(ReplyCommunicatePackage communicator, int timeMillionsecond)
        {
            throw new NotImplementedException("废弃接口，打回应答报文无同步操作必要");
        }

        protected void CoAsynFeedbackCommunicateReplyMessage(ReplyCommunicatePackage communicator)
        {
            _asynFeedbackCommunicateReplyMessageRunner.EnqueueMessage(communicator);
        }

        private void __AsynFeedbackCommunicateReplyMessageRunning(ReplyCommunicatePackage communicator)
        {
            if (_waitResponOutsideReqtPkg2CCReqtPkg.ContainsKey(communicator.RemotReqtPkg) &&
                _waitResponCCReqtPkg2ReqtMTPkg.ContainsKey(_waitResponOutsideReqtPkg2CCReqtPkg[communicator.RemotReqtPkg]))
            {
                ReplyPackage replyPkg = new ReplyPackage(communicator.RemotRet,
                                                                                    communicator.ParamPackage.ParamDefalutValues);

                C2CReplyPackage c2cReplyPkg = new C2CReplyPackage(ReplyPackage.Middleware_ReplyInfo.S_OK,
                                                                                                    null);
                c2cReplyPkg.OutSideMessage = replyPkg;

                C2CRequestPackage sourC2CReqtPkg = _waitResponOutsideReqtPkg2CCReqtPkg[communicator.RemotReqtPkg] as C2CRequestPackage;
                string mtReplyPkgReplyid = (_waitResponCCReqtPkg2ReqtMTPkg[sourC2CReqtPkg] as RequestMTPackage).MessageId;

                ReplyMTPackage mtReplyPkg = new ReplyMTPackage(c2cReplyPkg,
                                                                                                _selfDevice,
                                                                                                communicator.RemotReqtPkg.SourDevice,
                                                                                                mtReplyPkgReplyid);
                try
                {
                    _middlewareCommunicateProcessor.AsynSendMessage(mtReplyPkg);
                }
                catch (System.Exception ex)
                {
                    //throw excetion "ex"
                    this.CoMiddleware2MiddlewareAsynReplyCommunicatErrorRecived_OutsideNotify(communicator,
                                                                                                                new MiddlewareCommunicatErrorExcetion(ex.ToString()));
                }
                _waitResponOutsideReqtPkg2CCReqtPkg.Remove(communicator.RemotReqtPkg);
                _waitResponCCReqtPkg2ReqtMTPkg.Remove(sourC2CReqtPkg);
            }
            else
            {
                //throw excetion "在尝试匹配应答报文的时候，查询源报文索引失败"
                this.CoMiddleware2MiddlewareAsynReplyCommunicatErrorRecived_OutsideNotify(communicator,
                                                                                                            new MiddlewareCommunicatErrorExcetion("在尝试匹配应答报文的时候，查询源报文索引失败"));
            }
        }

        protected event AsynCommunicatErrorHandler CoMiddleware2MiddlewareAsynReplyCommunicatErrorRecived_OutsideNotify = null;

        private SequentialItemProcessor<ReplyCommunicatePackage> _asynFeedbackCommunicateReplyMessageRunner = null;
        #endregion

        #endregion

        #region RemotReqtIncomming
        protected event RemotReqtRecivedHandler CoRemotReqtRecived_OutsideNotify = null;
        /// <summary>
        /// 应答索引表，根据对应的请求包反查中间件传输包
        /// </summary>
        private Hashtable _waitResponCCReqtPkg2ReqtMTPkg = Hashtable.Synchronized(new Hashtable());
        //private ThreadSafeSortedList<C2CRequestPackage, RequestMTPackage> _waitResponCCReqtPkg2ReqtMTPkg = new ThreadSafeSortedList<C2CRequestPackage, RequestMTPackage>();
        private Hashtable _waitResponOutsideReqtPkg2CCReqtPkg = Hashtable.Synchronized(new Hashtable());
        //private ThreadSafeSortedList<RequestPackage, C2CRequestPackage> _waitResponOutsideReqtPkg2CCReqtPkg = new ThreadSafeSortedList<RequestPackage, C2CRequestPackage>();
        #endregion

        #region Group

        #region CreateGroup
        protected GroupDevice CoSynCreateGroup(string detail)
        {
            GroupDevice synGropDevice = null;
            try
            {
                synGropDevice = GroupCommunicateProcessor.CreateGroup(detail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            _localGroupDeviceBuffer.Add(synGropDevice);
            return synGropDevice;
        }

        protected GroupDevice CoAsynCreateGroup(string detail)
        {
            GroupDevice asynWaitGoup = new GroupDevice(this as MiddlewareCorelogicLayer);
            asynWaitGoup.Detail = detail;
            asynWaitGoup.Token = null;
            asynWaitGoup.Ready = false;
            _asynCreategroupRunner.EnqueueMessage(new KeyValuePair<string, GroupDevice>(detail, asynWaitGoup));
            return asynWaitGoup;
        }

        private void __CoAsynCreateGroupRunning(KeyValuePair<string, GroupDevice> detailAnduncompletGroup)
        {
            GroupDevice synGropDevice = null;
            try
            {
                synGropDevice = GroupCommunicateProcessor.CreateGroup(detailAnduncompletGroup.Key);
            }
            catch (Exception ex)
            {
                detailAnduncompletGroup.Value.excetion = ex as CreateGroupExcetion;
                detailAnduncompletGroup.Value.Ready = false;
            }
            detailAnduncompletGroup.Value.Specific(synGropDevice);
            _localGroupDeviceBuffer.Add(synGropDevice);
        }

        private SequentialItemProcessor<KeyValuePair<string, GroupDevice>> _asynCreategroupRunner = null;
        #endregion

        #region GetGroup
        protected GroupDevice CoSynGetGroup(string detail)
        {
            try
            {
                GroupDevice gropDevice = _groupCommunicateProcessor.GetGroup(detail);
                _localGroupDeviceBuffer.Add(gropDevice);
                return gropDevice;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        protected GroupDevice CoAsynGetGroup(string detail)
        {
            GroupDevice newUncompletGroup = new GroupDevice(this);
            _asynGetGroupRunner.EnqueueMessage(new KeyValuePair<string, GroupDevice>(detail, newUncompletGroup));
            return newUncompletGroup;
        }

        private void __AsynCoGetGroupRunning(KeyValuePair<string, GroupDevice> detailAnduncompletGroup)
        {
            GroupDevice synGropDevice = null;
            try
            {
                synGropDevice = _groupCommunicateProcessor.GetGroup(detailAnduncompletGroup.Key);
            }
            catch (System.Exception ex)
            {
                detailAnduncompletGroup.Value.excetion = ex as GetOnlineGroupExcetion;
                detailAnduncompletGroup.Value.Ready = false;
            }
            detailAnduncompletGroup.Value.Specific(synGropDevice);
            _localGroupDeviceBuffer.Add(synGropDevice);
        }

        private SequentialItemProcessor<KeyValuePair<string, GroupDevice>> _asynGetGroupRunner = null;
        #endregion

        #region JoinGroup
        protected void CoSynJoinGroup(GroupDevice group, GroupMemberRole role)
        {
            try
            {
                group.Join(role);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        protected void CoAsynJoinGroup(GroupDevice group, GroupMemberRole role)
        {
            _asynJoinGroupRunner.EnqueueMessage(new KeyValuePair<GroupDevice, GroupMemberRole>(group, role));
        }

        private void __CoAsynJoinGroupRunning(KeyValuePair<GroupDevice, GroupMemberRole> wilJoinGroupAndRole)
        {
            try
            {
                wilJoinGroupAndRole.Key.Join(wilJoinGroupAndRole.Value);
            }
            catch (System.Exception ex)
            {
                if (ex is JoinGroupExcetion)
                {
                    wilJoinGroupAndRole.Key.excetion = ex as JoinGroupExcetion;
                }
                if (ex is GetOnlineGroupClientExcetion)
                {
                    wilJoinGroupAndRole.Key.excetion = ex as GetOnlineGroupClientExcetion;
                }
            }
        }

        private SequentialItemProcessor<KeyValuePair<GroupDevice, GroupMemberRole>> _asynJoinGroupRunner = null;
        #endregion

        #region ExitGroup
        protected void CoSynExitGroup(GroupDevice group)
        {
            try
            {
                group.Exit();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        protected void CoAsynExitGroup(GroupDevice group)
        {
            _asynExitGroupRunner.EnqueueMessage(group);
        }

        private void __CoAsynExitGroupRunning(GroupDevice group)
        {
            try
            {
                group.Exit();
            }
            catch (System.Exception ex)
            {
                group.excetion = ex as ExitGroupExcetion;
            }
        }

        private SequentialItemProcessor<GroupDevice> _asynExitGroupRunner = null;
        #endregion

        #region CreateRadioCommunicatePackage
        protected GroupComunicatePackage CoCreateGroupCommunicatePackage(string radioName,
                                                                                                    ParamPackage radioParamPkg,
                                                                                                    GroupDevice targetGroup)
        {
            GroupComunicatePackage gropCommunicatPkg = new GroupComunicatePackage();
            gropCommunicatPkg.CommunicationName = radioName;
            gropCommunicatPkg.ParamPackage = radioParamPkg;
            gropCommunicatPkg.TargetDevice = targetGroup;
            gropCommunicatPkg.WaitResponse = false;
            return gropCommunicatPkg;
        }
        #endregion

        #region Radio
        protected void CoAsynRadio(GroupComunicatePackage communicator)
        {
            _asynRadioRunner.EnqueueMessage(communicator);
        }

        private void __CoAsynRadioRunning(GroupComunicatePackage communicator)
        {
            RadioPackage radioPkg = new RadioPackage(communicator.TargetDevice,
                                                                        communicator.CommunicationName,
                                                                        communicator.ParamPackage.ParamDefalutValues);

            C2CRadioPackage c2cRadioPkg = new C2CRadioPackage(communicator.TargetDevice,
                                                                                        "outside radio message",
                                                                                        null);
            c2cRadioPkg.OutsideMessage = radioPkg;
            try
            {
                _groupCommunicateProcessor.Radio(c2cRadioPkg);
            }
            catch (System.Exception ex)
            {
                this.CoRadioErrorRecived_OutsideNotify(communicator, new RadioErrorExcetion(ex.ToString()));
            }
        }
        protected event AsynCommunicatErrorHandler CoRadioErrorRecived_OutsideNotify = null;
        private SequentialItemProcessor<GroupComunicatePackage> _asynRadioRunner = null;
        #endregion

        #region RadioMessageIncomming
        protected event RemotRadioRecivedHandler CoRemotRadioRecived_OutsideNotify = null;
        #endregion

        protected List<GroupDevice> _localGroupDeviceBuffer = new List<GroupDevice>();

        #endregion

        #region Bin传输层接口
        /// <summary>
        /// 下层消息通知
        /// </summary>
        /// <param name="scPkg">服务端响应报文包</param>
        internal void ServerMessageRecived(ServerReplyPackage rplyPkg)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 下层消息通知(有配对来源)
        /// </summary>
        /// <param name="sclPkg">发起服务器请求的报文包</param>
        /// <param name="scrPkg">响应的服务器报文包</param>
        internal void ServerMessageRecived(ServerRequestPackage reqtPkg, ServerReplyPackage rplyPkg)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 中间件传输层接口
        /// <summary>
        /// 中间件传输层请求消息通知
        /// </summary>
        /// <param name="mtPkg">新到的中间件通讯数据包</param>
        internal void MiddlewareTransferMessageRecived(RequestMTPackage reqtMTPkg)
        {
            C2CRequestPackage c2cReqtPkg = reqtMTPkg.C2CNormalTransPackage;

            if (!c2cReqtPkg.OutSideMessage.IsEmpty)
            {
                if (c2cReqtPkg.WaittingResponse)
                {
                    //_waitResponCCReqtPkg2ReqtMTPkg.Add(c2cReqtPkg, reqtMTPkg);
                    _waitResponCCReqtPkg2ReqtMTPkg[c2cReqtPkg] = reqtMTPkg;
                }

                //index waitting response
                RequestPackage outsideReqtPkg = c2cReqtPkg.OutSideMessage;
                if (outsideReqtPkg.WaittingResponse)
                {
                    //_waitResponOutsideReqtPkg2CCReqtPkg.Add(outsideReqtPkg, c2cReqtPkg);
                    _waitResponOutsideReqtPkg2CCReqtPkg[outsideReqtPkg] = c2cReqtPkg;
                }
                //outside notify
                this.CoRemotReqtRecived_OutsideNotify(outsideReqtPkg);
            }
            else
            {
                //消息模块验证消息
                if (c2cReqtPkg.OperatName.Equals("ListenerVertificationRequest"))
                {
                    C2CReplyPackage replyPkg = mMiddlewareMessenger.VertificationInfoRecived(c2cReqtPkg);
                    ReplyMTPackage mtReplyPkg = new ReplyMTPackage(replyPkg, 
                                                                    _selfDevice, 
                                                                    reqtMTPkg.SourceDevice, 
                                                                    reqtMTPkg.MessageId);
                    try
                    {
                        _middlewareCommunicateProcessor.AsynSendMessage(mtReplyPkg);

                    }catch(Exception ex)
                    {
                        //TODO.
                    }
                }
            }
        }

        /// <summary>
        /// 中间件传输层消息通知
        /// </summary>
        /// <param name="sourMtPkg">发起中间件通讯的数据包</param>
        /// <param name="targtMtPkh">匹配的应答包</param>
        internal void MiddlewareTransferMessageRecived(MiddlewareTransferPackage sourMtPkg, MiddlewareTransferPackage targtMtPkh)
        {
            throw new NotImplementedException("目前的调用机制暂时不需要使用MiddlewareCommunicat的异步接口,故不应触发到该分支");
        }

        /// <summary>
        /// 中间件传输层Error信息通知
        /// </summary>
        /// <param name="sourMtPkg">该错误匹配的一次中间件通讯发起报文</param>
        /// <param name="errorInfoPkg">错误信息包装类</param>
        internal void MiddlewareTransferErrorRecived(MiddlewareTransferPackage sourMtPkg, MiddlewareTransferErrorExcetion errExInfo)
        {
            throw new NotImplementedException("目前的调用机制暂时使用同步接口对BinLayer进行操作,故不应触发到该分支");
        }
        #endregion

        #region 群组传输层接口
        internal void RadioTransferMessageRecived(C2CRadioPackage c2cRadioPkg)
        {
            if (!c2cRadioPkg.OutsideMessage.IsEmpty)
            {
                //outside notify
                this.CoRemotRadioRecived_OutsideNotify(c2cRadioPkg.OutsideMessage);
            }
            else
            {
                //消息模块数据包
                if (null != (c2cRadioPkg as C2CMessageRadioPackage))
                {
                    mMiddlewareMessenger.MessagePackageIncoming(c2cRadioPkg as C2CMessageRadioPackage);
                }
            }
        }
        #endregion

        #region 中间件消息层接口
        protected void CoListen(ClientDevice messenger, BaseMessageType typMsg) 
        {
            mMiddlewareMessenger.Listen(messenger, typMsg);
        }
        protected void CoRegistMessage(BaseMessageType typMsg, Type t_Msg) 
        {
            mMiddlewareMessenger.RegistMessage(typMsg, t_Msg);
        }
        protected BaseMessage CoCreateMessage(BaseMessageType typMsg) 
        {
            return mMiddlewareMessenger.CreateMessage(typMsg);
        }
        protected void CoSendMessage(BaseMessage msg) 
        {
            mMiddlewareMessenger.SendMessage(msg);
        }
        protected MessageRecivedHandler CoMessageRecived_OutisdeNotify = null;
        #endregion

        internal GroupCommunicateLayer GroupCommunicateProcessor
        {
            get { return _groupCommunicateProcessor; }
        }
        internal MiddlewareCommunicateLayer MiddlewareCommunicateLayer
        {
            get { return _middlewareCommunicateProcessor; }
        }
        internal BinTransferLayer BinTransferProcessor
        {
            get { return _binProcessferProcessor; }
        }
        public bool Online
        {
            get { return _bIsOnline; }
            internal set { _bIsOnline = false; }
        }
        public ClientDevice SelfDevice
        {
            get { return _selfDevice; }
        }

        private GroupCommunicateLayer _groupCommunicateProcessor = null;
        private MiddlewareCommunicateLayer _middlewareCommunicateProcessor = null;
        private BinTransferLayer _binProcessferProcessor = null;
        private IMiddlewareMessenger mMiddlewareMessenger = null;

        private ClientDevice _selfDevice = null;
        private bool _bIsOnline = false;
        private bool _bFirstLinkSucessful = false;
    }
    #endregion
}

