using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;
using ProtoBuf;

namespace ProtocolLibrary.CSProtocol
{
    [ProtoContract]
    public class CSCommunicateClass
    {
        public static List<Type> CreateSpecializationSerializeTypeSet()
        {
            List<Type> typList = new List<Type>();
            typList.Add(typeof(List<CSCommunicateClass.GroupInfo>));
            typList.Add(typeof(List<CSCommunicateClass.ClientInfo>));
            return typList;
        }

        [ProtoContract]
        public class GroupInfo
        {
            public GroupInfo() { }
            public GroupInfo(string token, string detail)
            {
                _token = token;
                _detail = detail;
            }
            [ProtoMember(1)]
            public string Token
            {
                get { return _token; }
                set { _token = value; }
            }
            [ProtoMember(2)]
            public string Detail
            {
                get { return _detail; }
                set { _detail = value; }
            }
            public string _token = null;
            private string _detail = null;
        }

        [ProtoContract]
        public class ClientInfo
        {
            public ClientInfo() { }
            public ClientInfo(string token, string detail)
            {
                Token = token;
                Detail = detail;
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
    }
}
