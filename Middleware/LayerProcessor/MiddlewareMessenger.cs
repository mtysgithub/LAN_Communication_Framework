using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Middleware.LayerProcessor.Interfcace;
using Middleware.Device;
using Middleware.Communication;
using Middleware.Communication.Message;
using Middleware.Communication.Message.Interface;

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

        protected MiddlewareMessenger() { }

        public void Initialize(GroupCommunicateLayer groupCommunicateProcessor, 
                                MiddlewareCommunicateLayer middlewareCommunicateProcessor,
                                MessageRecivedHandler coMsgRecivedHandler)
        {
            mGroupCommunicateProcessor = groupCommunicateProcessor;
            mMiddlewareCommunicateProcessor = middlewareCommunicateProcessor;

            mMessageFactory = MessageFactory.Instance;
            this.MessageRecived += coMsgRecivedHandler;
        }

        public void Release()
        {
            this.MessageRecived = null;
            mMessageFactory = null;

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
        }

        public AbstractMessage CreateMessage(AbstractMessageType typMsg)
        {
            return mMessageFactory.CreateMessage(typMsg);
        }

        public void SendMessage(AbstractMessage msg)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public event MessageRecivedHandler MessageRecived = null;
    }
}
