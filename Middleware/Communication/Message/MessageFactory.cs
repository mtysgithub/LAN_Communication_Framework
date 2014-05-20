using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Middleware.Communication.Message.Interface;

namespace Middleware.Communication.Message
{
    internal class MessageFactory : IMessageFactory
    {
        private static MessageFactory mInstance = null;
        protected Hashtable mTypTable = new Hashtable();

        public static IMessageFactory Instance
        {
            get 
            {
                if(null == mInstance)
                {
                    lock(typeof(MessageFactory))
                    {
                        if(null == mInstance)
                        {
                            mInstance = new MessageFactory();
                        }
                    }
                }
                return mInstance as IMessageFactory;
            }
        }

        protected MessageFactory() { }

        public void RegistMessage(BaseMessageType typMsg, Type t_Msg)
        {
            if ((null != typMsg) && (!mTypTable.ContainsKey(typMsg.Id)))
            {
                mTypTable.Add(typMsg.Id, t_Msg);
            }else
            {
                throw new InvalidOperationException("已经注册的消息类型: " + typMsg.Id);
            }
        }

        public BaseMessage CreateMessage(BaseMessageType typMsg)
        {
            if ((null != typMsg) && (mTypTable.ContainsKey(typMsg.Id)))
            {
                BaseMessage msg = Activator.CreateInstance(mTypTable[typMsg.Id] as Type) as BaseMessage;
                msg.Type = typMsg;
                return msg;
            }
            else
            {
                throw new InvalidOperationException("未知的消息类型: " + typMsg.Id);
            }
        }
    }
}
