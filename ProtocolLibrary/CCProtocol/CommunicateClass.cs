using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ProtoBuf;

namespace ProtocolLibrary.CCProtocol
{
    [ProtoContract]
    public class CCCommunicateClass
    {
        public static List<Type> CreateSpecializationSerializeTypeSet()
        {
            List<Type> typList = new List<Type>();
            typList.Add(typeof(Seria_BaseMessageType));
            typList.Add(typeof(Seria_ParamPackage));
            typList.Add(typeof(Seria_Device));
            typList.Add(typeof(Seria_MiddlewareTransferPackage));
            return typList;
        }

        [ProtoContract]
        public class Seria_BaseMessageType
        {
            public Seria_BaseMessageType() { }

            public Seria_BaseMessageType(Seria_BaseMessageType obj)
            {
                this.Name = obj.Name;
                this.Id = obj.Id;
            }

            [ProtoMember (1)]
            public string Name;

            [ProtoMember(2)]
            public uint Id;
        }

        [ProtoContract,
        ProtoInclude(1, typeof(Seria_RadioPackage)),
        ProtoInclude(2, typeof(Seria_RequestPackage)),
        ProtoInclude(3, typeof(Seria_ReplyPackage)),
        ProtoInclude(9, typeof(Seria_BaseMessage))]
        public class Seria_ParamPackage
        {
            public Seria_ParamPackage() { }

            public Seria_ParamPackage(Seria_ParamPackage param)
            {
                ParamDefalutValues = param.ParamDefalutValues;
            }

            [ProtoMember(4)]
            public Dictionary<string, byte[]> ParamDefalutValues
            {
                get;
                set;
            }
        }

        [ProtoContract]
        public class Seria_BaseMessage : Seria_ParamPackage
        {
            public Seria_BaseMessage() : base() { }

            public Seria_BaseMessage(Seria_BaseMessage obj)
                : base(obj as Seria_ParamPackage)
            {
                this.MessageType = obj.MessageType;
            }

            public Seria_BaseMessage(Seria_ParamPackage parent)
                : base(parent)
            {

            }

            [ProtoMember (1)]
            public Seria_BaseMessageType MessageType;
        }

        [ProtoContract,
        ProtoInclude(4, typeof(Seria_C2CRadioPackage))]
        public class Seria_RadioPackage : Seria_ParamPackage
        {
            public Seria_RadioPackage() : base() { }

            public Seria_RadioPackage(Seria_RadioPackage obj)
                : base(obj as Seria_ParamPackage)
            {
                this.GroupInfo = obj.GroupInfo;
                this.RadioName = obj.RadioName;
            }

            public Seria_RadioPackage(Seria_ParamPackage parent)
                : base(parent)
            {

            }

            [ProtoMember(1)]
            public Seria_GroupDevice GroupInfo
            {
                get;
                set;
            }

            [ProtoMember(2)]
            public string RadioName
            {
                get;
                set;
            }
        }

        [ProtoContract,
        ProtoInclude(6, typeof(Seria_C2CRequestPackage))]
        public class Seria_RequestPackage : Seria_ParamPackage
        {
            public Seria_RequestPackage() : base() { }

            public Seria_RequestPackage(Seria_RequestPackage obj)
                : base(obj as Seria_ParamPackage)
            {
                this.OperatName = obj.OperatName;
                this.SourDeviceInfo = obj.SourDeviceInfo;
                this.WaittingResponse = obj.WaittingResponse;
            }

            public Seria_RequestPackage(Seria_ParamPackage parent)
                : base(parent)
            {

            }

            [ProtoMember(1)]
            public string OperatName
            {
                get;
                set;
            }

            [ProtoMember(2)]
            public Seria_ClientDevice SourDeviceInfo
            {
                get;
                set;
            }

            [ProtoMember(3)]
            public bool WaittingResponse
            {
                get;
                set;
            }
        }

        [ProtoContract,
        ProtoInclude(5, typeof(Seria_C2CReplyPackage))]
        public class Seria_ReplyPackage : Seria_ParamPackage
        {
            public Seria_ReplyPackage() : base() { }

            public Seria_ReplyPackage(Seria_ReplyPackage obj)
                : base(obj as Seria_ParamPackage)
            {
                this.ReplyState = obj.ReplyState;
            }

            public Seria_ReplyPackage(Seria_ParamPackage parent)
                : base(parent)
            {

            }

            [ProtoMember(1)]
            public int ReplyState
            {
                get;
                set;
            }
        }

        [ProtoContract,
        ProtoInclude(8, typeof(Seria_C2CMessageRadioPackage))]
        public class Seria_C2CRadioPackage : Seria_RadioPackage
        {
            public Seria_C2CRadioPackage() : base() { }

            public Seria_C2CRadioPackage(Seria_C2CRadioPackage obj)
                : base(obj as Seria_RadioPackage)
            {
                this.OutsideMessage = obj.OutsideMessage;
            }

            public Seria_C2CRadioPackage(Seria_RadioPackage paret)
                : base(paret)
            {
            }

            [ProtoMember(1)]
            public Seria_RadioPackage OutsideMessage
            {
                get;
                set;
            }
        }

        [ProtoContract]
        public class Seria_C2CMessageRadioPackage : Seria_C2CRadioPackage
        {
            public Seria_C2CMessageRadioPackage() : base() { }

            public Seria_C2CMessageRadioPackage(Seria_C2CMessageRadioPackage obj)
                : base(obj as Seria_C2CRadioPackage)
            {
                this.Message = obj.Message;
            }

            public Seria_C2CMessageRadioPackage(Seria_C2CRadioPackage paret)
                : base(paret) 
            {
            }

            [ProtoMember (1)]
            public Seria_BaseMessage Message;
        }

        [ProtoContract,
        ProtoInclude(10, typeof(Seria_C2CMessageVerificationRequestPackage))]
        public class Seria_C2CRequestPackage : Seria_RequestPackage
        {
            public Seria_C2CRequestPackage() : base() { }

            public Seria_C2CRequestPackage(Seria_C2CRequestPackage obj)
                : base(obj as Seria_RequestPackage)
            {
                this.OutsideMessage = obj.OutsideMessage;
            }

            public Seria_C2CRequestPackage(Seria_RequestPackage parent)
                : base(parent)
            {

            }

            [ProtoMember(1)]
            public Seria_RequestPackage OutsideMessage
            {
                get;
                set;
            }
        }

        [ProtoContract]
        public class Seria_C2CReplyPackage : Seria_ReplyPackage
        {
            public Seria_C2CReplyPackage() : base() { }

            public Seria_C2CReplyPackage(Seria_C2CReplyPackage obj)
                : base(obj as Seria_ParamPackage)
            {
                this.OutsideMessage = obj.OutsideMessage;
            }

            public Seria_C2CReplyPackage(Seria_ReplyPackage parent)
                : base(parent)
            {
                
            }

            [ProtoMember(1)]
            public Seria_ReplyPackage OutsideMessage
            {
                get;
                set;
            }
        }

        [ProtoContract]
        public class Seria_C2CMessageVerificationRequestPackage : Seria_C2CRequestPackage
        {
            public Seria_C2CMessageVerificationRequestPackage() : base() { }

            public Seria_C2CMessageVerificationRequestPackage(Seria_C2CMessageVerificationRequestPackage obj)
                : base(obj as Seria_C2CRequestPackage)
            {
                this.MessageType = obj.MessageType;
            }

            public Seria_C2CMessageVerificationRequestPackage(Seria_C2CRequestPackage parent)
                : base(parent)
            {

            }

            [ProtoMember (1)]
            public Seria_BaseMessageType MessageType;
        }

        [ProtoContract,
        ProtoInclude(6, typeof(Seria_GroupDevice)),
        ProtoInclude(7, typeof(Seria_ClientDevice))]
        public class Seria_Device
        {
            public Seria_Device() { }
            public Seria_Device(Seria_Device obj)
            {
                this.Token = obj.Token;
                this.Detail = obj.Detail;
            }

            [ProtoMember(1)]
            public string Token
            {
                get;
                set;
            }

            [ProtoMember(2)]
            public string Detail
            {
                get;
                set;
            }
        }

        [ProtoContract]
        public class Seria_GroupDevice : Seria_Device
        {
            public Seria_GroupDevice() : base() { }

            public Seria_GroupDevice(Seria_Device parent)
                : base(parent)
            {

            }

            public Seria_GroupDevice(Seria_GroupDevice obj)
                : base(obj as Seria_Device)
            {
                //Nothing todo
            }

            [ProtoMember(1)]
            public new string Token
            {
                get { return base.Token; }
                set { base.Token = value; }
            }

            [ProtoMember(2)]
            public new string Detail
            {
                get { return base.Detail; }
                set { base.Detail = value; }
            }
        }

        [ProtoContract]
        public class Seria_ClientDevice : Seria_Device
        {
            public Seria_ClientDevice() : base() { }

            public Seria_ClientDevice(Seria_ClientDevice obj)
                : base(obj as Seria_Device)
            {

            }

            public Seria_ClientDevice(Seria_Device parent)
                : base(parent)
            {

            }

            [ProtoMember(1)]
            public new string Token
            {
                get { return base.Token; }
                set { base.Token = value; }
            }

            [ProtoMember(2)]
            public new string Detail
            {
                get { return base.Detail; }
                set { base.Detail = value; }
            }
        }

        [ProtoContract,
        ProtoInclude(256, typeof(Seria_RequestMTPackage)),
        ProtoInclude(257, typeof(Seria_ReplyMTPackage))]
        public class Seria_MiddlewareTransferPackage
        {
            public Seria_MiddlewareTransferPackage() { }

            public Seria_MiddlewareTransferPackage(Seria_MiddlewareTransferPackage obj)
            {
                this.MessageId = obj.MessageId;
                this.SourceDeviceInfo = obj.SourceDeviceInfo;
                this.TargetDeviceInfo = obj.TargetDeviceInfo;
            }

            [ProtoMember(1)]
            public string MessageId
            {
                get;
                set;
            }

            [ProtoMember(2)]
            public Seria_ClientDevice TargetDeviceInfo
            {
                get;
                set;
            }

            [ProtoMember(3)]
            public Seria_ClientDevice SourceDeviceInfo
            {
                get;
                set;
            }

            protected Seria_ParamPackage C2CMessagePackage
            {
                get;
                set;
            }
        }

        [ProtoContract]
        public class Seria_RequestMTPackage : Seria_MiddlewareTransferPackage
        {
            public Seria_RequestMTPackage() : base() { }

            public Seria_RequestMTPackage(Seria_RequestMTPackage obj)
                : base(obj as Seria_MiddlewareTransferPackage)
            {
                this.WattingResponse = obj.WattingResponse;
            }

            public Seria_RequestMTPackage(Seria_MiddlewareTransferPackage parent)
                : base(parent)
            {

            }

            [ProtoMember(1)]
            public bool WattingResponse
            {
                get;
                set;
            }

            [ProtoMember(2)]
            public Seria_C2CRequestPackage C2CNormalTransPackage
            {
                get { return base.C2CMessagePackage as Seria_C2CRequestPackage; }
                set { base.C2CMessagePackage = value; }
            }
        }

        [ProtoContract]
        public class Seria_ReplyMTPackage : Seria_MiddlewareTransferPackage
        {
            public Seria_ReplyMTPackage() : base() { }

            public Seria_ReplyMTPackage(Seria_ReplyMTPackage obj)
                : base(obj as Seria_MiddlewareTransferPackage) 
            {
                this.C2CReplyPackage = obj.C2CReplyPackage;
                this.RepliedMessageId = obj.RepliedMessageId;
            }

            public Seria_ReplyMTPackage(Seria_MiddlewareTransferPackage parent)
                : base(parent)
            {

            }

            [ProtoMember(1)]
            public Seria_C2CReplyPackage C2CReplyPackage
            {
                get { return base.C2CMessagePackage as Seria_C2CReplyPackage; }
                set { base.C2CMessagePackage = value; }
            }

            [ProtoMember(2)]
            public string RepliedMessageId
            {
                get;
                set;
            }
        }
    }
}
