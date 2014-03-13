using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolLibrary.CSProtocol.CommonConfig
{
    public class ConstData
    {
        // IP Address.
        public const int tokenLength = 32 + 4;
        public const int tokenStringLength = 32 + 4;
        public const string emptyGuidString = "00000000-0000-0000-0000-000000000000";
    }
    public enum ServerOprCodec
    {
        Error = 0x00000000,
        Initialize,
        P2pTransport,
        CreateGroup,
        JoinGroup,
        ExitGroup,
        RadioTransport,
        GroupOnlineList,
        GroupClientOnlineList,
        ClientCancle
    }
    namespace ClientMsgCodecSpace
    {
        public enum ClientHeadCodec
        {
            Transport = 0x00000000,
            GroupTransport = 0x00000001,

            Init_ReplySucess = 0x00000002,                      
            Init_ReplyFaild,
      
            P2pTransport_ReplySucess,
            P2pTransport_ReplyFaild,

            CreateGroup_ReplySucess,
            CreateGroup_ReplyFaild,

            JoinGroup_ReplySucess,
            JoinGroup_ReplyFaild,

            ExitGroup_ReplySucess,
            ExitGroup_ReplyFaild,

            RadioTransport_ReplySucess,
            RadioTransport_ReplyFaild,

            GetOnlineGroup_ReplySucess,
            GetOnlineGroup_ReplyFaild,

            GetGroupClient_ReplySucess,
            GetGroupClient_ReplyFaild,

            ClientCancle_ReplySucess,
            ClientCancle_ReplyFaild
        }
        public enum InitFaildCodec
        {
            Detail_REPEAT = 0x00000003,      //注册的身份重复
            Relink_FORBIDDEN,       //非法的重连操作
            Initialize_UNFINISH       //未初始化的情况下执行操作
        }
        public enum P2pTransportFaildCodec
        {
            Token_UNPARSE = 0x00000003,     //无法识别的目标Token
            Target_UNCONNECT,      //无法链接到目标客户端
        }
        public enum CreateGroupFaildCodec
        {
            Detail_REPEAT = 0x00000003,         //注册的身份重复
        }
        public enum JoinGroupFaildCodec
        {
            Group_INEXISTANCE = 0x00000003,  //群组不存在
            Client_REPEAT,               //客户端已存在
        }
        public enum ExitGroupFaildCodec
        {
            Group_INEXISTANCE = 0x00000003,  //群组不存在
            Client_UNBELONG,            //客户端不属于该群组
        }
        public enum RadioTransportFaildCodec
        {
            Trans_FORBIDDEN = 0x00000003, //没有分发权限
            Group_INEXISTANCE,        //群组不存在
            Client_UNBELONG,            //客户端不属于该群组
        }
        public enum GetOnlineGroupsFaildCodec
        {
        }
        public enum GetGoupClientFaildCodec
        {
            Group_INEXISTENCE = 0x00000003,    //群组不存在
            Client_UNBELONG,             //客户端不属于该群组
        }
        public enum ClientCancleFaildCodec
        {
            // N / A
        }
    }
}
