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

        protected GroupCommunicateLayer mGroupCommunicateProcessor = null;
        protected MiddlewareCommunicateLayer mMiddlewareCommunicateProcessor = null;

        protected IMessageFactory mMessageFactory = null;
        protected Hashtable mMsgTyp2Dispatcher = null;
        private const string mDispatcherPrefixi = "Dispatcher_";

        protected MiddlewareMessenger() { }

        public void Initialize(GroupCommunicateLayer groupCommunicateProcessor, 
                                MiddlewareCommunicateLayer middlewareCommunicateProcessor,
                                MessageRecivedHandler coMsgRecivedHandler)
        {
            mGroupCommunicateProcessor = groupCommunicateProcessor;
            mMiddlewareCommunicateProcessor = middlewareCommunicateProcessor;

            this.MessageRecived += coMsgRecivedHandler;
            mMessageFactory = MessageFactory.Instance;
            mMsgTyp2Dispatcher = new Hashtable();
        }

        public void Release()
        {
            mMsgTyp2Dispatcher.Dispose();
            mMsgTyp2Dispatcher = null;
            mMessageFactory = null;
            this.MessageRecived = null;

            mMiddlewareCommunicateProcessor = null;
            mGroupCommunicateProcessor = null;
        }

        public void Listen(ClientDevice messenger, AbstractMessageType typMsg)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RegistMessage(AbstractMessageType typMsg, Type t_Msg)
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
        }

        public AbstractMessage CreateMessage(AbstractMessageType typMsg)
        {
            return mMessageFactory.CreateMessage(typMsg);
        }

        public void SendMessage(AbstractMessage msg)
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
