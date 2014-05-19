using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NIPlayRoomNetServer;
using NIPlayRoomNetServer.ServerClient;

namespace NIPlayRoomNetServer.Interface
{
    interface IGroupMannager
    {
        AppErrorInfo CreateGroup(string detail, out string token, out byte retFaildCodec);
        AppErrorInfo JoinGroup(BaseServerClient client, string groupToken, byte listOrRadioCodec, out byte oprFaildCodec);
        AppErrorInfo ExitGroup(BaseServerClient client, string groupToken, out byte oprFaildCodec);
        AppErrorInfo GetGroup(string token, out Group gropInst);
        AppErrorInfo GropClientCancle(BaseServerClient client);

        //考虑设计为多种通信模式（全局，管道，过滤器）
        AppErrorInfo SendRadioMessage(BaseServerClient client, string groupToken, byte[] content, out byte oprFaildCodec);

        List<Group> Groups { get; }

        void GroupManagerDisable();
    }
}
