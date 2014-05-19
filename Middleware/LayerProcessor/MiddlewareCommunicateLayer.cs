using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using ProtoBuf;

using Hik.Communication.Scs;
using Hik.Communication;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Threading;
using Hik.Collections;

using ProtocolLibrary.CCProtocol;
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
using Middleware.Communication.Excetion;

namespace Middleware.LayerProcessor
{
    #region 中间件交换层
    internal class MiddlewareTransferErrorExcetion : Exception
    {
        public MiddlewareTransferErrorExcetion(string info)
            : base(info) { }
        public MiddlewareTransferErrorExcetion(Exception baseExcetion)
            : base(baseExcetion.ToString())
        {
        }
    }
    internal enum MiddlewareTransferServerStateInfo
    {
        WaittingForResponse,
        ResponseReceived,
        Base,
        TimeOut,
    }

    internal class MiddlewareCommunicateLayer
    {
        internal MiddlewareCommunicateLayer(MiddlewareCorelogicLayer middlewareCorelogicLayer)
        {
            _middlewareCorelogicLayer = middlewareCorelogicLayer;
        }

        internal MiddlewareErrorInfo Initialization()
        {
            MiddlewareErrorInfo ret = MiddlewareErrorInfo.S_OK;
            if ((false == _bIsInited) && (false == _bIsRunning))
            {
                _synWattingMiddlewareReplyMessages.ClearAll();
                _asynWattingMiddlewareReplyMessages.ClearAll();
                _bIsInited = true;
            }
            else
            {
                throw new Exception("中间件交换层 已经初始化");
            }
            return ret;
        }
        internal void Start()
        {
            if ((true == _bIsInited) && (false == _bIsRunning))
            {
                try
                {
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
                    throw new Exception("中间件交换层 未初始化");

                if (true == _bIsRunning)
                    throw new Exception("中间件交换层 已经运行");
            }
        }
        internal void Stop()
        {
            if ((true == _bIsInited) && (true == _bIsRunning))
            {
                try
                {
                    _synWattingMiddlewareReplyMessages.ClearAll();
                    _asynWattingMiddlewareReplyMessages.ClearAll();
                    _bIsRunning = false;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                if (false == _bIsInited)
                    throw new Exception("中间件交换层 未初始化");

                if (false == _bIsRunning)
                    throw new Exception("中间件交换层 未运行");
            }
        }
        internal void Dispose()
        {
            if ((true == _bIsInited) && (false == _bIsRunning))
            {
                _bIsInited = false;
            }
            else
            {
                if (false == _bIsInited)
                    throw new Exception("中间件交换层 未进行初始化");
                if (true == _bIsRunning)
                    throw new Exception("中间件交换层 处在运行态");
            }

        }

        /// <summary>
        /// 阻塞式执行一次消息发送请求
        /// </summary>
        /// <param name="mtPkg">发往远程中间件设备的消息包</param>
        /// <returns>远程中间件返回的报文包</returns>
        /// 
        internal MiddlewareTransferPackage SynSendMessage(MiddlewareTransferPackage mtPkg)
        {
            return SynSendMessage(mtPkg, -1);
        }
        internal MiddlewareTransferPackage SynSendMessage(MiddlewareTransferPackage mtPkg, int timeoutMilliseconds)
        {
            if (mtPkg is RequestMTPackage)
            {
                return __SendMessageAndWattingForResponse(mtPkg, timeoutMilliseconds);
            }
            else
            {
                throw new MiddlewareCommunicatErrorExcetion("不能尝试同步一份应答报文");
            }
        }

        /// <summary>
        /// 非阻塞式执行一次消息发送请求
        /// </summary>
        /// <param name="mtPkg">发往远程中间件设备的消息包</param>
        internal void AsynSendMessage(MiddlewareTransferPackage mtPkg)
        {
            //will send a message
            if ((mtPkg is RequestMTPackage) && ((mtPkg as RequestMTPackage).WattingResponse))
            {
                __ActiveMiddlewareMessageWattinghere(mtPkg);
            }

            bool mustWaitReponse = true;
            ServerRequestPackage wilSendScsMsg = new ServerRequestPackage(mustWaitReponse);

            //配置为Middleware传输包
            IMiddleware2MiddlewareCommunicatRequest statSeter = wilSendScsMsg.Active_MiddlewareCommunicatRequest();
            statSeter.WilSendMiddleware2MiddlewareMessage(mtPkg.TargetDevice, mtPkg);

            BinTransferLayer btlprocessor = _middlewareCorelogicLayer.BinTransferProcessor;
            ServerReplyPackage replyServPkg = null;
            try
            {
                replyServPkg = btlprocessor.SynSendMessage(wilSendScsMsg, 100000);
            }
            catch (System.Exception ex)
            {
                //TODO
                throw new MiddlewareTransferErrorExcetion(ex);
            }
            //分析服务端报文
            IMiddleware2MiddlewareCommunicatReply replyInterface = replyServPkg.Active_Middleware2MiddlewareCommunicatReply();
            bool ret;
            string detail;
            try
            {
                replyInterface.SendMiddlewareMsgOprRet(out ret, out detail);
            }
            catch (Exception ex)
            {
                throw new MiddlewareTransferErrorExcetion(ex.ToString());
            }
            if (true == ret)
            {
                //noting to do
            }
            else
            {
                throw new MiddlewareTransferErrorExcetion(detail);
            }
        }

        ///// <summary>
        ///// 阻塞式执行一次消息发送请求
        ///// </summary>
        ///// <param name="reqtPkg">发往服务端的消息包</param>
        ///// <returns>服务端返回报文包</returns>
        //internal ServerCtrReplyPackage SynSendMessage(ServerRequestPackage reqtPkg)
        //{
        //    //TODO
        //    //同步能力在下一个子版本中实现
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// 非阻塞的执行一次消息发送请求
        ///// </summary>
        ///// <param name="reqtPkg">发往服务端的消息包</param>
        //internal void ASynSendMessage(ServerRequestPackage reqtPkg)
        //{
        //}

        /// <summary>
        /// 下层消息通知
        /// </summary>
        /// <param name="rplyPkg">服务端响应报文包</param>
        internal void ServerMessageRecived(ServerReplyPackage rplyPkg)
        {
            if ((ServerReplyPackage.ReplyType.middleware_propagate == rplyPkg.MsgType) && (null != rplyPkg.MTPkg))
            {
                __ProcessMiddlewareMessageRecived(rplyPkg.MTPkg);
            }
            else
            {
                if (ServerReplyPackage.ReplyType.middleware_propagate != rplyPkg.MsgType)
                {
                    //由于未索引的应答报文已经被BinLayer放弃，执行到此分支是一个错误
                    throw new Exception("一个未被catch的应答报文");
                }
                if (null == rplyPkg.MTPkg)
                {
                    //无法解析的中间件消息包
                    throw new NotImplementedException("无法解析的中间件消息包");
                }
            }
        }

        /// <summary>
        /// 下层消息通知(有配对来源)
        /// </summary>
        /// <param name="reqtPkg">发起本次服务器请求的报文包</param>
        /// <param name="rplyPkg">响应的服务器报文包</param>
        internal void ServerMessageRecived(ServerRequestPackage reqtPkg, ServerReplyPackage rplyPkg)
        {
            //由于对下层调用的实现机制目前完全以同步调用实现，故触发此分支将是一个错误
            throw new NotImplementedException("MiddleCommunicateLayer目前对Bin层调用全部以同步实现，执行此分支是一个错误");

            //if (ServerReplyPackage.ReplyType.reply == rplyPkg.MsgType)
            //{
            //    //判断服务器端执行是否挂掉
            //    if (true != rplyPkg.MiddlewarePkgTransferServOprRet)
            //    {
            //        //按挂掉的源请求是否同步区分处理
            //        bool bSynArrIndexExist = false;
            //        lock (_synReadLockObject)
            //        {
            //            bSynArrIndexExist = _synWattingMiddlewareReplyMessages.ContainsKey(reqtPkg.MTPkg.MessageId);
            //        }
            //        if (true == bSynArrIndexExist)
            //        {
            //            //处理同步消息错误
            //            MiddlewareTransferServerStateInfo servOprStat = MiddlewareTransferErrorExcetion.Parse(rplyPkg);
            //            (_synWattingMiddlewareReplyMessages[reqtPkg.MTPkg.MessageId] as WaitingMessage).State = servOprStat;
            //            //此处不清理同步资源，由__SendMessageAndWattingForResponse执行退出时完成
            //        }
            //        else
            //        {
            //            //处理异步消息错误,向上提交错误
            //            MiddlewareTransferPackage sourMtPkg = reqtPkg.MTPkg;
            //            MiddlewareTransferErrorExcetion transErrorExInfo = new MiddlewareTransferErrorExcetion(MiddlewareTransferErrorExcetion.Parse(rplyPkg));
            //            _middlewareCorelogicLayer.MiddlewareTransferErrorRecived(sourMtPkg, transErrorExInfo);

            //            //清理资源
            //            bool havWattingRecored;
            //            lock (_synReadLockObject)
            //            {
            //                havWattingRecored = _asynWattingMiddlewareReplyMessages.ContainsKey(sourMtPkg.MessageId);
            //            }
            //            if (havWattingRecored)
            //            {
            //                _asynWattingMiddlewareReplyMessages.Remove(sourMtPkg.MessageId);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        //若中间件消息正常发送，则忽略掉此服务器报文，否则上报
            //    }
            //}
            //else
            //{
            //    //有catch的报文必为服务器响应报文
            //    throw new Exception("有catch的报文必为服务器响应报文");
            //}
        }

        private void __ProcessMiddlewareMessageRecived(MiddlewareTransferPackage mtMsg)
        {
            if (mtMsg is RequestMTPackage)
            {
                //向上请求消息
                _middlewareCorelogicLayer.MiddlewareTransferMessageRecived(mtMsg as RequestMTPackage);
            }
            if (mtMsg is ReplyMTPackage)
            {
                //Noting to do, push up
                ReplyMTPackage mtReplyPkg = mtMsg as ReplyMTPackage;
                bool bAsynArrIndexExist = false;
                bool bSynArrIndexExist = false;
                lock (_synReadLockObject)
                {
                    if (false == string.IsNullOrEmpty(mtReplyPkg.RepliedMessageId))
                    {
                        bAsynArrIndexExist = _asynWattingMiddlewareReplyMessages.ContainsKey(mtReplyPkg.RepliedMessageId);
                        bSynArrIndexExist = _synWattingMiddlewareReplyMessages.ContainsKey(mtReplyPkg.RepliedMessageId);
                    }
                }
                if ((false == bAsynArrIndexExist) &&
                    (false == bSynArrIndexExist))
                {
                    //一个未知的应答消息，没有索引，丢弃
                }
                else
                {
                    if (true == bSynArrIndexExist)
                    {
                        //处理同步消息
                        WaitingMessage waitingMessage = null;
                        lock (_synReadLockObject)
                        {
                            if (_synWattingMiddlewareReplyMessages.ContainsKey(mtReplyPkg.RepliedMessageId))
                            {
                                waitingMessage = _synWattingMiddlewareReplyMessages[mtReplyPkg.RepliedMessageId] as WaitingMessage;
                            }
                        }

                        //If there is a thread waiting for this response message, pulse it
                        if (waitingMessage != null)
                        {
                            waitingMessage.ResponseMessage = mtReplyPkg;
                            waitingMessage.State = MiddlewareTransferServerStateInfo.ResponseReceived;
                            waitingMessage.WaitEvent.Set();
                            return;
                        }
                    }
                    else
                    {
                        //处理异步消息    
                        MiddlewareTransferPackage sourMtPkg = _asynWattingMiddlewareReplyMessages[mtReplyPkg.RepliedMessageId] as MiddlewareTransferPackage;
                        _middlewareCorelogicLayer.MiddlewareTransferMessageRecived(sourMtPkg, mtReplyPkg);

                        //清理资源
                        bool havWattingRecored;
                        lock (_synReadLockObject)
                        {
                            havWattingRecored = _asynWattingMiddlewareReplyMessages.ContainsKey(mtReplyPkg.RepliedMessageId);
                        }
                        if (havWattingRecored)
                        {
                            _asynWattingMiddlewareReplyMessages.Remove(mtReplyPkg.RepliedMessageId);
                        }
                    }
                }
            }
        }
        private void __ActiveMiddlewareMessageWattinghere(MiddlewareTransferPackage mtPkg)
        {
            lock (_synWriteLockObject)
            {
                _asynWattingMiddlewareReplyMessages[mtPkg.MessageId] = mtPkg;
            }
        }
        //private void __ActiveServOprRetMessageWattinghere(ServerRequestPackage wilSendScsMsg) 
        //{
        //    lock (_synWriteLockObject)
        //    {
        //        _asynWaittingServCtrReplyMessages[wilSendScsMsg.MessageId] = wilSendScsMsg;
        //    }
        //}

        private object _synReadLockObject = new object();
        private object _synWriteLockObject = new object();

        private ThreadSafeSortedList<string, MiddlewareTransferPackage> _asynWattingMiddlewareReplyMessages = new ThreadSafeSortedList<string, MiddlewareTransferPackage>();

        private bool _bIsInited = false;
        private bool _bIsRunning = false;

        private MiddlewareCorelogicLayer _middlewareCorelogicLayer = null;

        #region 中间件传输层同步机制
        /// <summary>
        /// 阻塞式执行一次中间层报文通讯
        /// </summary>
        /// <param name="msg">发起方报文</param>
        /// <returns>远程中间件响应报文</returns>
        private MiddlewareTransferPackage __SendMessageAndWattingForResponse(MiddlewareTransferPackage msg)
        {
            return __SendMessageAndWattingForResponse(msg, -1);
        }
        private MiddlewareTransferPackage __SendMessageAndWattingForResponse(MiddlewareTransferPackage msg, int timeoutMilliseconds)
        {
            WaitingMessage waitMsg = new WaitingMessage();
            lock (_synWriteLockObject)
            {
                _synWattingMiddlewareReplyMessages[msg.MessageId] = waitMsg;
            }

            //send message
            bool mustWaitReponse = true;
            ServerRequestPackage wilSendServMsg = new ServerRequestPackage(mustWaitReponse);

            //配置为中间件传输包
            IMiddleware2MiddlewareCommunicatRequest statSetter = wilSendServMsg.Active_MiddlewareCommunicatRequest();
            statSetter.WilSendMiddleware2MiddlewareMessage(msg.TargetDevice, msg);

            BinTransferLayer btlpeocessor = _middlewareCorelogicLayer.BinTransferProcessor;
            ServerReplyPackage replyServPkg = null;
            try
            {
                replyServPkg = btlpeocessor.SynSendMessage(wilSendServMsg, 100000);
            }
            catch (System.Exception ex)
            {
                //释放锁记录
                lock (_synWriteLockObject)
                {
                    _synWattingMiddlewareReplyMessages.Remove(msg.MessageId);
                }
                throw new MiddlewareTransferErrorExcetion(ex.ToString());
            }
            //分析服务端报文
            IMiddleware2MiddlewareCommunicatReply replyInterface = replyServPkg.Active_Middleware2MiddlewareCommunicatReply();
            bool ret;
            string detail;
            replyInterface.SendMiddlewareMsgOprRet(out ret, out detail);
            if (true == ret)
            {
                //noting to do
            }
            else
            {
                throw new MiddlewareTransferErrorExcetion(detail);
            }

            //wait for the recived event
            waitMsg.WaitEvent.Wait(timeoutMilliseconds);

            //Check for exceptions
            switch (waitMsg.State)
            {
                case MiddlewareTransferServerStateInfo.WaittingForResponse:
                    {
                        //同步超时
                        lock (_synWriteLockObject)
                        {
                            _synWattingMiddlewareReplyMessages.Remove(msg.MessageId);
                        }
                        throw new MiddlewareTransferErrorExcetion(MiddlewareTransferServerStateInfo.TimeOut.ToString());
                    }
                case MiddlewareTransferServerStateInfo.ResponseReceived:
                    {
                        lock (_synWriteLockObject)
                        {
                            _synWattingMiddlewareReplyMessages.Remove(msg.MessageId);
                        }
                        break;
                    }
            }
            return waitMsg.ResponseMessage;
        }

        #region WaitingMessage class
        /// <summary>
        /// This class is used to store messaging context for a request message
        /// until response is received.
        /// </summary>
        private sealed class WaitingMessage
        {
            /// <summary>
            /// Response message for request message 
            /// (null if response is not received yet).
            /// </summary>
            public MiddlewareTransferPackage ResponseMessage { get; set; }

            /// <summary>
            /// ManualResetEvent to block thread until response is received.
            /// </summary>
            public ManualResetEventSlim WaitEvent { get; private set; }

            /// <summary>
            /// State of the request message.
            /// </summary>
            public MiddlewareTransferServerStateInfo State { get; set; }

            /// <summary>
            /// Creates a new WaitingMessage object.
            /// </summary>
            public WaitingMessage()
            {
                WaitEvent = new ManualResetEventSlim(false);
                State = MiddlewareTransferServerStateInfo.WaittingForResponse;
            }
        }
        #endregion
        private ThreadSafeSortedList<string, WaitingMessage> _synWattingMiddlewareReplyMessages = new ThreadSafeSortedList<string, WaitingMessage>();
        #endregion
    }

    /// <summary>
    /// 中间件 - 中间件 传输基类
    /// </summary>
    public class MiddlewareTransferPackage : IMiddlewareMiddlewareTransferMessage, ICCSerializeOperat<CCCommunicateClass.Seria_MiddlewareTransferPackage>
    {
        public MiddlewareTransferPackage() 
        {
        }

        public MiddlewareTransferPackage(ParamPackage c2cMessagePackage,
                                                            ClientDevice sourceDevice,
                                                            ClientDevice targetDevice)
        {
            _c2cMessagePackage = c2cMessagePackage;
            _sourceDevice = sourceDevice;
            _targetDevice = targetDevice;
        }

        public string MessageId
        {
            get { return _id; }
            set { _id = value; }
        }

        public ClientDevice TargetDevice
        {
            get { return _targetDevice; }
            set { _targetDevice = value; }
        }

        public ClientDevice SourceDevice
        {
            get { return _sourceDevice; }
            set { _sourceDevice = value; }
        }

        #region SerializProcotol
        protected enum SerializObjectType
        {
            MiddlewareTransferPackage,
            RequestMTPackage,
            ReplyMTPackage
        }
        internal virtual byte[] SerializeMiddlewareMessage()
        {
            byte[] bytMiddlewareTransferPackage = null;
            CJNet_SerializeTool serializeTool = new CJNet_SerializeTool();
            using (MemoryStream m = new MemoryStream())
            {
                serializeTool.Serialize(m, this.ExportSerializeData());
                bytMiddlewareTransferPackage = m.ToArray();
            }

            byte objTypeCodec = (byte)SerializObjectType.MiddlewareTransferPackage;

            byte[] bytWilSend = new byte[1 + bytMiddlewareTransferPackage.Length];
            bytWilSend[0] = objTypeCodec;
            Buffer.BlockCopy(bytMiddlewareTransferPackage, 0, bytWilSend, 1, bytMiddlewareTransferPackage.Length);

            return bytWilSend;
        }
        internal MiddlewareTransferPackage DeserializeMessage(byte[] bytes)
        {
            if ((null == bytes) || (0 == bytes.Length))
            {
                throw new Exception("Bin数据不存在或为空");
            }
            byte bytOpjTypeCodec = bytes[0];
            switch (bytOpjTypeCodec)
            {
                case (byte)SerializObjectType.MiddlewareTransferPackage:
                    {
                        try
                        {
                            byte[] bytObjContent = new byte[bytes.Length - 1];
                            Buffer.BlockCopy(bytes, 1, bytObjContent, 0, bytObjContent.Length);

                            CCCommunicateClass.Seria_MiddlewareTransferPackage middlewareSerializeObj = null;
                            MiddlewareTransferPackage retPkg = new MiddlewareTransferPackage();
                            using (MemoryStream m = new MemoryStream(bytObjContent))
                            {
                                CJNet_SerializeTool deSerializeTool = new CJNet_SerializeTool();
                                middlewareSerializeObj = deSerializeTool.Deserialize(m, null, typeof(CCCommunicateClass.Seria_MiddlewareTransferPackage)) 
                                                                    as CCCommunicateClass.Seria_MiddlewareTransferPackage;
                            }
                            retPkg.ParseSerializeData(middlewareSerializeObj);
                            return retPkg;
                        }
                        catch (System.Exception ex)
                        {
                            throw new Exception("针对Bin数据尝试反序列失败，请检验数据格式: " + ex.ToString());
                        }
                    }
                case (byte)SerializObjectType.RequestMTPackage:
                    {
                        try
                        {
                            byte[] bytObjContent = new byte[bytes.Length - 1];
                            Buffer.BlockCopy(bytes, 1, bytObjContent, 0, bytObjContent.Length);

                            CCCommunicateClass.Seria_RequestMTPackage reqtMTPSerializeObj = null;
                            RequestMTPackage retPkg = new RequestMTPackage();
                            using (MemoryStream m = new MemoryStream(bytObjContent))
                            {
                                CJNet_SerializeTool deSerializeTool = new CJNet_SerializeTool();
                                reqtMTPSerializeObj = deSerializeTool.Deserialize(m, null, typeof(CCCommunicateClass.Seria_RequestMTPackage)) as CCCommunicateClass.Seria_RequestMTPackage;
                            }
                            retPkg.ParseSerializeData(reqtMTPSerializeObj);
                            return retPkg;
                        }
                        catch (System.Exception ex)
                        {
                            throw new Exception("针对Bin数据尝试反序列失败，请检验数据格式: " + ex.ToString());
                        }
                    }
                case (byte)SerializObjectType.ReplyMTPackage:
                    {
                        try
                        {
                            byte[] bytObjContent = new byte[bytes.Length - 1];
                            Buffer.BlockCopy(bytes, 1, bytObjContent, 0, bytObjContent.Length);

                            CCCommunicateClass.Seria_ReplyMTPackage replyMTPSerializeObj = null; 
                            ReplyMTPackage retPkg = new ReplyMTPackage();
                            using (MemoryStream m = new MemoryStream(bytObjContent))
                            {
                                CJNet_SerializeTool deSerializeTool = new CJNet_SerializeTool();
                                replyMTPSerializeObj = deSerializeTool.Deserialize(m, null, typeof(CCCommunicateClass.Seria_ReplyMTPackage)) as CCCommunicateClass.Seria_ReplyMTPackage;
                            }
                            retPkg.ParseSerializeData(replyMTPSerializeObj);
                            return retPkg;
                        }
                        catch (System.Exception ex)
                        {
                            throw new Exception("针对Bin数据尝试反序列失败，请检验数据格式: " + ex.ToString());
                        }
                    }
                default:
                    {
                        throw new NotImplementedException("二进制数据指向无法识别的类型");
                    }
            }
        }
        #endregion

        #region ICCSerializeOperat<CCCommunicateClass.Seria_MiddlewareTransferPackage>
        public void ParseSerializeData(CCCommunicateClass.Seria_MiddlewareTransferPackage obj)
        {
            this.MessageId = obj.MessageId;
            this.TargetDevice.ParseSerializeData(obj.TargetDeviceInfo);
            this.SourceDevice.ParseSerializeData(obj.SourceDeviceInfo);
        }

        public CCCommunicateClass.Seria_MiddlewareTransferPackage ExportSerializeData()
        {
            CCCommunicateClass.Seria_MiddlewareTransferPackage ret = new CCCommunicateClass.Seria_MiddlewareTransferPackage();
            ret.MessageId = this.MessageId;
            ret.TargetDeviceInfo = this.TargetDevice.ExportSerializeData();
            ret.SourceDeviceInfo = this.SourceDevice.ExportSerializeData();
            return ret;
        }
        #endregion

        protected ClientDevice _targetDevice = ClientDevice.Empty;
        protected ClientDevice _sourceDevice = ClientDevice.Empty;

        protected string _id = System.Guid.NewGuid().ToString();
        protected ParamPackage _c2cMessagePackage = null;
    }

    /// <summary>
    /// 中间件 - 中间件 正文传输数据包, 使服务器透明，可用于中间件p2p间的同异步消息
    /// </summary>
    public class RequestMTPackage : MiddlewareTransferPackage, ICCSerializeOperat<CCCommunicateClass.Seria_RequestMTPackage>
    {
        public RequestMTPackage() 
            : base() 
        {
            this.C2CNormalTransPackage = C2CRequestPackage.Empty;
        }

        public RequestMTPackage(C2CRequestPackage c2cNormalTransPackage,
                                                ClientDevice sourceDevice,
                                                ClientDevice targetDevice,
                                                bool waittingResponse)
            : base(c2cNormalTransPackage, sourceDevice, targetDevice)
        {
            _bWaittingResponse = waittingResponse;
        }

        #region SerializProcotol
        internal override byte[] SerializeMiddlewareMessage()
        {
            byte[] bytMiddlewareTransferPackage = null;
            CJNet_SerializeTool serializeTool = new CJNet_SerializeTool();
            using (MemoryStream m = new MemoryStream())
            {
                serializeTool.Serialize(m, this.ExportSerializeData());
                bytMiddlewareTransferPackage = m.ToArray();
            }

            byte objTypeCodec = (byte)SerializObjectType.RequestMTPackage;

            byte[] bytWilSend = new byte[1 + bytMiddlewareTransferPackage.Length];
            bytWilSend[0] = objTypeCodec;
            Buffer.BlockCopy(bytMiddlewareTransferPackage, 0, bytWilSend, 1, bytMiddlewareTransferPackage.Length);

            return bytWilSend;
        }
        #endregion

        #region ICCSerializeOperat<CCCommunicateClass.Seria_RequestMTPackage>
        public void ParseSerializeData(CCCommunicateClass.Seria_RequestMTPackage obj)
        {
            base.ParseSerializeData(obj as CCCommunicateClass.Seria_MiddlewareTransferPackage);
            this.WattingResponse = obj.WattingResponse;
            this.C2CNormalTransPackage.ParseSerializeData(obj.C2CNormalTransPackage);
        }
        public new CCCommunicateClass.Seria_RequestMTPackage ExportSerializeData()
        {
            CCCommunicateClass.Seria_RequestMTPackage ret = new CCCommunicateClass.Seria_RequestMTPackage(base.ExportSerializeData());
            ret.C2CNormalTransPackage = this.C2CNormalTransPackage.ExportSerializeData();
            ret.WattingResponse = this.WattingResponse;
            return ret;
        }
        #endregion

        public bool WattingResponse
        {
            get { return _bWaittingResponse; }
            set { _bWaittingResponse = value; }
        }
        public C2CRequestPackage C2CNormalTransPackage
        {
            get { return _c2cMessagePackage as C2CRequestPackage; }
            set { _c2cMessagePackage = value; }
        }

        protected bool _bWaittingResponse = false;
    }

    /// <summary>
    /// 中间件应答报文包，使服务器透明，可用于中间件p2p间的同异步消息
    /// </summary>
    public class ReplyMTPackage : MiddlewareTransferPackage, ICCSerializeOperat<CCCommunicateClass.Seria_ReplyMTPackage>
    {
        public ReplyMTPackage() 
            : base() 
        {
            this.C2CReplyPackage = C2CReplyPackage.Empty;
        }

        public ReplyMTPackage(C2CReplyPackage c2cReplyPackage,
                                            ClientDevice sourceDevice,
                                            ClientDevice tagetDevice,
                                            string replyId)
            : base(c2cReplyPackage, sourceDevice, tagetDevice)
        {
            this.RepliedMessageId = replyId;
        }

        #region SerializProcotol
        internal override byte[] SerializeMiddlewareMessage()
        {
            byte[] bytMiddlewareTransferPackage = null;
            CJNet_SerializeTool serializeTool = new CJNet_SerializeTool();
            using (MemoryStream m = new MemoryStream())
            {
                serializeTool.Serialize(m, this.ExportSerializeData());
                bytMiddlewareTransferPackage = m.ToArray();
            }

            byte objTypeCodec = (byte)SerializObjectType.ReplyMTPackage;

            byte[] bytWilSend = new byte[1 + bytMiddlewareTransferPackage.Length];
            bytWilSend[0] = objTypeCodec;
            Buffer.BlockCopy(bytMiddlewareTransferPackage, 0, bytWilSend, 1, bytMiddlewareTransferPackage.Length);

            return bytWilSend;
        }
        #endregion

        #region ICCSerializeOperat<CCCommunicateClass.Seria_ReplyMTPackage>
        public void ParseSerializeData(CCCommunicateClass.Seria_ReplyMTPackage obj)
        {
            base.ParseSerializeData(obj as CCCommunicateClass.Seria_MiddlewareTransferPackage);
            this.C2CReplyPackage.ParseSerializeData(obj.C2CReplyPackage);
            this.RepliedMessageId = obj.RepliedMessageId;
        }
        public new CCCommunicateClass.Seria_ReplyMTPackage ExportSerializeData()
        {
            CCCommunicateClass.Seria_ReplyMTPackage ret = new CCCommunicateClass.Seria_ReplyMTPackage(base.ExportSerializeData());
            ret.C2CReplyPackage = this.C2CReplyPackage.ExportSerializeData();
            ret.RepliedMessageId = this.RepliedMessageId;
            return ret;
        }
        #endregion

        public C2CReplyPackage C2CReplyPackage
        {
            get { return _c2cMessagePackage as C2CReplyPackage; }
            set { _c2cMessagePackage = value; }
        }
        public string RepliedMessageId
        {
            get { return _replyid; }
            set
            {
                _replyid = value;
            }
        }

        protected string _replyid = string.Empty;
    }
    #endregion
}