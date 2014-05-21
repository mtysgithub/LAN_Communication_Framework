using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Middleware.LayerProcessor.Interfcace;
using Middleware.Device;
using Middleware.Communication;
using Middleware.Communication.Message;
using Middleware.Communication.Message.Interface;
using Middleware.Communication.Package.Internal;

namespace Middleware.LayerProcessor
{
    internal class MiddlewareMessenger : IMiddlewareMessenger
    {
        protected static MiddlewareMessenger mInstance = null;
        public static IMiddlewareMessenger Instance
        {
            get 
            {
                if(null == mInstance)
                {
                    lock (typeof(MiddlewareMessenger))
                    {
                        if(null == mInstance)
                        {
                            mInstance = new MiddlewareMessenger();
                        }
                    }
                }
                return mInstance as IMiddlewareMessenger;
            }
        }

        protected MiddlewareCorelogicLayer mCoreLogicProcessor = null;
        protected GroupCommunicateLayer mGroupCommunicateProcessor = null;
        protected MiddlewareCommunicateLayer mMiddlewareCommunicateProcessor = null;

        protected IMessageFactory mMessageFactory = null;
        protected Hashtable mMsgTyp2Dispatcher = null;
        private const string mDispatcherPrefixi = "Dispatcher_";

        protected Hashtable mListeningGroup = null;
        protected Hashtable mSpeakGroup = null;

        protected MiddlewareMessenger() { }

        public void Initialize(MiddlewareCorelogicLayer coreLogicProcessor,
                                GroupCommunicateLayer groupCommunicateProcessor, 
                                MiddlewareCommunicateLayer middlewareCommunicateProcessor,
                                MessageRecivedHandler coMsgRecivedHandler)
        {
            mListeningGroup = new Hashtable();
            mSpeakGroup = new Hashtable();

            mCoreLogicProcessor = coreLogicProcessor;
            mGroupCommunicateProcessor = groupCommunicateProcessor;
            mMiddlewareCommunicateProcessor = middlewareCommunicateProcessor;

            this.MessageRecived += coMsgRecivedHandler;
            mMessageFactory = MessageFactory.Instance;
            mMsgTyp2Dispatcher = new Hashtable();
        }

        public void Release()
        {
            mMsgTyp2Dispatcher.Clear();
            mMsgTyp2Dispatcher = null;
            mMessageFactory = null;
            this.MessageRecived = null;

            mCoreLogicProcessor = null;
            mMiddlewareCommunicateProcessor = null;
            mGroupCommunicateProcessor = null;

            mSpeakGroup.Clear();
            mSpeakGroup = null;
            mListeningGroup.Clear();
            mListeningGroup = null;
        }

        public void Listen(ClientDevice messenger, BaseMessageType typMsg)
        {
            if (mListeningGroup.ContainsKey(typMsg.Id))
            {
                throw new InvalidOperationException("该设备消息已被监听");
            }

            C2CRequestPackage listenerVertificationRequestPkg =
                new C2CRequestPackage(mCoreLogicProcessor.SelfDevice,
                                        "ListenerVertificationRequest",
                                        true,
                                        new Dictionary<string, byte[]>() 
                                        { 
                                            { "MessageType", 
                                                BitConverter.GetBytes(typMsg.Id) } 
                                        });
            RequestMTPackage mtReqtPkg = new RequestMTPackage(listenerVertificationRequestPkg, 
                                                                mCoreLogicProcessor.SelfDevice,
                                                                messenger,
                                                                true);
            MiddlewareTransferPackage mtReplyBasePkg = null;
            try
            {
                //100s limited time to wait this c2c response.
                mtReplyBasePkg = mMiddlewareCommunicateProcessor.SynSendMessage(mtReqtPkg, 10000);

            }catch(Exception ex)
            {
                throw new Exception("尝试监听一个设备消息时遭遇网络异常：" + ex.ToString());
            }

            ReplyMTPackage mtReplyPkg = mtReplyBasePkg as ReplyMTPackage;
            C2CReplyPackage c2cReplyPkg = mtReplyPkg.C2CReplyPackage;
            if (Communication.Package.ReplyPackage.Middleware_ReplyInfo.S_OK == c2cReplyPkg.ReplyState)
            {
                string gourp_detail = Encoding.ASCII.GetString(c2cReplyPkg.ParamDefalutValues["group_detail"]);
                GroupDevice wilListenGropup = null;
                try
                {
                    wilListenGropup = mGroupCommunicateProcessor.GetGroup(gourp_detail);
                    mGroupCommunicateProcessor.JoinGroup(wilListenGropup, 
                                                            Communication.CommunicationConfig.GroupMemberRole.Listener);

                }catch(Exception ex)
                {
                    throw new Exception("尝试监听一个设备消息时遭遇网络异常：" + ex.ToString());
                }
                mListeningGroup.Add(typMsg.Id, wilListenGropup);
            }else
            {
                throw new Exception("尝试监听一个设备消息时遭遇网络异常：" + 
                                        Encoding.UTF8.GetString(c2cReplyPkg.ParamDefalutValues["excetion_detail"]));
            }
        }

        public void RegistMessage(BaseMessageType typMsg, Type t_Msg)
        {
            mMessageFactory.RegistMessage(typMsg, t_Msg);
            GroupDevice group = null;
            try
            {
                mGroupCommunicateProcessor.CreateGroup(mDispatcherPrefixi + "_" + typMsg.Name + "_" + Guid.NewGuid().ToString());
                mGroupCommunicateProcessor.JoinGroup(group, Communication.CommunicationConfig.GroupMemberRole.Speaker);

            }catch(Exception ex)
            {
                throw new Exception("试图注册消息时遭遇网络失败: " + ex.ToString());
            }
            mSpeakGroup.Add(typMsg.Id, group);
        }

        public BaseMessage CreateMessage(BaseMessageType typMsg)
        {
            return mMessageFactory.CreateMessage(typMsg);
        }

        public void SendMessage(BaseMessage msg)
        {
            if (mMsgTyp2Dispatcher.ContainsKey(msg.Type))
            {
                GroupDevice group = mMsgTyp2Dispatcher[msg.Type] as GroupDevice;
                try
                {
                    mGroupCommunicateProcessor.Radio(new C2CMessageRadioPackage(group, msg));

                }catch(Exception ex)
                {
                    throw new Exception("试图发送消息时遭遇网络失败: " + ex.ToString());
                }
            }
            else
            {
                throw new InvalidOperationException("未注册的消息类型: " + msg.Type.Name);
            }
        }

        public event MessageRecivedHandler MessageRecived = null;
    }
}
