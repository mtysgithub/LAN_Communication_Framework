using System;
using System.Text;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Protocols.BinarySerialization;

using ProtocolLibrary.CSProtocol.CommonConfig;

namespace ProtocolLibrary.CSProtocol
{
    public class MyUnderScsWireProtocol : BinarySerializationProtocol
    {
        enum MsgType
        {
            ScsRaw_Msg = 0,
            ScsText_Msg,
            ScsPing_Msg,
            UnType_Msg
        }
        protected override byte[] SerializeMessage(IScsMessage message)
        {
            byte msgTypeCodec = Convert.ToByte((int)MsgType.UnType_Msg);

            byte [] bytMsgId = new byte [ConstData.tokenStringLength];
            byte [] bytReplyMsgId = new byte [ConstData.tokenStringLength];
            if (false == string.IsNullOrEmpty(message.MessageId))
            {
                bytMsgId = Encoding.ASCII.GetBytes(message.MessageId);
            }
            else
            {
                bytMsgId = Encoding.ASCII.GetBytes(ConstData.emptyGuidString);
            }

            if (false == string.IsNullOrEmpty(message.RepliedMessageId))
            {
                bytReplyMsgId = Encoding.ASCII.GetBytes(message.RepliedMessageId);
            }
            else
            {
                bytReplyMsgId = Encoding.ASCII.GetBytes(ConstData.emptyGuidString);
            }

            byte[] content = null;

            if (message is ScsRawDataMessage)
            {
                msgTypeCodec = (byte)MsgType.ScsRaw_Msg;
                content = new byte[((ScsRawDataMessage)message).MessageData.Length];
                Buffer.BlockCopy(((ScsRawDataMessage)message).MessageData, 0, content, 0, content.Length);
            }
            else if (message is ScsTextMessage)
            {
                msgTypeCodec = (byte)MsgType.ScsText_Msg;
                content = Encoding.UTF8.GetBytes(((ScsTextMessage)message).Text);
            }
            else if(message is ScsPingMessage)
            {
                msgTypeCodec = (byte)MsgType.ScsPing_Msg;
            }

            byte [] finalMsg = new byte [1 + ConstData.tokenStringLength * 2 + ((null != content)?(content.Length):(0))];
            finalMsg[0] = msgTypeCodec;
            Buffer.BlockCopy(bytMsgId, 0, finalMsg, 1, ConstData.tokenStringLength);
            Buffer.BlockCopy(bytReplyMsgId, 0, finalMsg, 1 + ConstData.tokenStringLength, ConstData.tokenStringLength);
            if (null != content)
            {
                Buffer.BlockCopy(content, 0, finalMsg, 1 + ConstData.tokenStringLength * 2, content.Length);
            }

            return finalMsg;
        }

        protected override IScsMessage DeserializeMessage(byte[] bytes)
        {
            IScsMessage retMsg = null;

            string msgId = null;
            byte [] bytMsgId = new byte [ConstData.tokenStringLength];
            Buffer.BlockCopy(bytes, 1, bytMsgId, 0, ConstData.tokenStringLength);
            msgId = Encoding.ASCII.GetString(bytMsgId);
            if (msgId.Equals(ConstData.emptyGuidString))
            {
                msgId = null;
            }

            string replyMsgId = null;
            byte [] bytReplyMsgId = new byte [ConstData.tokenStringLength];
            Buffer.BlockCopy(bytes, 1 + ConstData.tokenStringLength, bytReplyMsgId, 0, ConstData.tokenStringLength);
            replyMsgId = Encoding.ASCII.GetString(bytReplyMsgId);
            if (replyMsgId.Equals(ConstData.emptyGuidString))
            {
                replyMsgId = null;
            }

            byte msgTypeCodec = bytes[0];
            switch ((int)msgTypeCodec)
            {
                case ((int)MsgType.ScsRaw_Msg):
                    {
                        if (0 < bytes.Length - (1 + ConstData.tokenStringLength * 2))
                        {
                            byte[] content = new byte[bytes.Length - (1 + ConstData.tokenStringLength * 2)];
                            Buffer.BlockCopy(bytes, 1 + ConstData.tokenStringLength * 2, content, 0, content.Length);
                            retMsg = new ScsRawDataMessage(content);
                        }
                        else
                        {
                            retMsg = new ScsRawDataMessage();
                        }
                        ((ScsRawDataMessage)retMsg).MessageId = msgId;
                        ((ScsRawDataMessage)retMsg).RepliedMessageId = replyMsgId;
                        break;
                    }
                case ((int)MsgType.ScsText_Msg):
                    {
                        if (0 < bytes.Length - (1 + ConstData.tokenStringLength * 2))
                        {
                            byte[] bytContent = new byte[bytes.Length - (1 + ConstData.tokenStringLength * 2)];
                            Buffer.BlockCopy(bytes, 1 + ConstData.tokenStringLength * 2, bytContent, 0, bytContent.Length);
                            string content = Encoding.UTF8.GetString(bytContent);
                            retMsg = new ScsTextMessage(content);
                        }
                        else
                        {
                            retMsg = new ScsTextMessage();
                        }
                        ((ScsTextMessage)retMsg).MessageId = msgId;
                        ((ScsTextMessage)retMsg).RepliedMessageId = replyMsgId;
                        break;
                    }
                case ((int)MsgType.ScsPing_Msg):
                    {
                        // N / A
                        retMsg = new ScsPingMessage();
                        ((ScsPingMessage)retMsg).MessageId = msgId;
                        ((ScsPingMessage)retMsg).RepliedMessageId = replyMsgId;
                        break;
                    }
                default: { throw new NotImplementedException(); }
            }
            return retMsg;
        }
    }
}
