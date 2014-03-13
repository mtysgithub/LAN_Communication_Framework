using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NIPlayRoomNetServer.ServerClient;

namespace NIPlayRoomNetServer.Interface
{
    interface IServer : IGroupMannager
    {
        AppErrorInfo Init(int tcpPort);
        void Stop();
        AppErrorInfo Send(string srcToken, string dstToken, byte[] content, out byte oprFaildCodec);
        AppErrorInfo ClientCancle(BaseServerClient client);
    }
}
