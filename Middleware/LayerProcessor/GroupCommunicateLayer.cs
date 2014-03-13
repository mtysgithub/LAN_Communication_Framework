using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ProtocolLibrary.CSProtocol;

using Middleware.Device;
using Middleware.Interface.ServerProcotolOperator.Request;
using Middleware.Interface.ServerProcotolOperator.Reply;
using Middleware.Communication.CommunicationConfig;
using Middleware.Communication;
using Middleware.Communication.Package;
using Middleware.Communication.Package.Internal;

namespace Middleware.LayerProcessor
{
    #region 群组通讯层

    public class GroupOperatErrorExcetion : Exception
    {
        public GroupOperatErrorExcetion(string exceInfo)
            : base(exceInfo)
        { _exceDetail = exceInfo; }
        protected string _exceDetail = string.Empty;
    }
    public class CreateGroupExcetion : GroupOperatErrorExcetion
    {
        public CreateGroupExcetion(string exceInfo)
            : base(exceInfo)
        {

        }
    }
    public class ExitGroupExcetion : GroupOperatErrorExcetion
    {
        public ExitGroupExcetion(string exceInfo)
            : base(exceInfo)
        {

        }
    }
    public class RadioGroupExcetion : GroupOperatErrorExcetion
    {
        public RadioGroupExcetion(string exceInfo)
            : base(exceInfo)
        {

        }
    }
    public class GetOnlineGroupExcetion : GroupOperatErrorExcetion
    {
        public GetOnlineGroupExcetion(string exceInfo)
            : base(exceInfo)
        {

        }
    }
    public class GetOnlineGroupClientExcetion : GroupOperatErrorExcetion
    {
        public GetOnlineGroupClientExcetion(string exceInfo)
            : base(exceInfo)
        {

        }
    }
    public class JoinGroupExcetion : GroupOperatErrorExcetion
    {
        public JoinGroupExcetion(string exInfo)
            : base(exInfo)
        {

        }
    }
    public class RefereshGroupInfo : GroupOperatErrorExcetion
    {
        public RefereshGroupInfo(string exInfo)
            : base(exInfo)
        {

        }
    }

    internal class GroupCommunicateLayer
    {
        public GroupCommunicateLayer(MiddlewareCorelogicLayer mclLayerProcessor)
        {
            _mclLayerProcessor = mclLayerProcessor;
        }

        internal void Initialization() { }
        internal void Start() { }
        internal void Stop() { }
        internal void Dispose() { }

        internal void Radio(C2CRadioPackage radioPkg)
        {
            //配置请求包
            ServerRequestPackage reqtPkg = new ServerRequestPackage(true);
            IRadioMessageRequest reqtInterface = reqtPkg.Active_RadioMessageRequest();
            reqtInterface.RadioMessage(radioPkg.Group.Token, radioPkg);

            BinTransferLayer binProcessor = _mclLayerProcessor.BinTransferProcessor;
            ServerReplyPackage replyPkg = null;
            try
            {
                replyPkg = binProcessor.SynSendMessage(reqtPkg, 100000);
            }
            catch (System.Exception ex)
            {
                throw new RadioGroupExcetion(ex.ToString());
            }
            IRadioReply replyInterface = replyPkg.Active_RadioGroupReply();
            bool ret;
            string errorDetail;
            replyInterface.RadioRet(out ret, out errorDetail);
            if (true == ret)
            {
                //nothing to do
            }
            else
            {
                throw new RadioGroupExcetion(errorDetail);
            }
        }
        internal GroupDevice CreateGroup(string detail)
        {
            //配置请求包
            ServerRequestPackage reqtPkg = new ServerRequestPackage(true);
            ICreateGroupRequest reqtInterface = reqtPkg.Active_CreateGroupRequest();
            reqtInterface.CreateNewGroup(detail);

            BinTransferLayer binprocessor = _mclLayerProcessor.BinTransferProcessor;
            ServerReplyPackage replyPkg = null;
            try
            {
                replyPkg = binprocessor.SynSendMessage(reqtPkg, 100000);
            }
            catch (System.Exception ex)
            {
                //throw timeout excetion
                throw new CreateGroupExcetion(ex.ToString());
            }
            ICreateGroupReply replyInterface = replyPkg.Active_CreateGroupReply();
            bool ret;
            string detailOrToken;
            replyInterface.CreateGroupRet(out ret, out detailOrToken);
            if (true == ret)
            {
                GroupDevice retGrop = new GroupDevice(_mclLayerProcessor);
                retGrop.Token = detailOrToken;
                retGrop.Detail = detail;
                retGrop.Ready = true;
                return retGrop;
            }
            else
            {
                throw new CreateGroupExcetion(detailOrToken);
            }
        }
        internal GroupDevice GetGroup(string detail)
        {
            //配置请求包
            ServerRequestPackage reqtPkg = new ServerRequestPackage(true);
            IOnlineGroupListRequest reqtInterface = reqtPkg.Active_OnlineGroupListRequest();
            reqtInterface.OnlineGroupRequest();

            BinTransferLayer binprocessor = _mclLayerProcessor.BinTransferProcessor;
            ServerReplyPackage replyPkg = null;
            try
            {
                replyPkg = binprocessor.SynSendMessage(reqtPkg, 100000);
            }
            catch (Exception ex)
            {
                throw new GetOnlineGroupExcetion(ex.ToString());
            }
            IOnlineGroupReply replyInterface = replyPkg.Active_OnlineGroupReply();
            bool ret;
            string errorDetail;
            List<CSCommunicateClass.GroupInfo> onlineGroupInfo;
            replyInterface.OnlineGroupRet(out ret, out errorDetail, out onlineGroupInfo);
            if (true == ret)
            {
                foreach (CSCommunicateClass.GroupInfo item in onlineGroupInfo)
                {
                    if (item.Detail.Equals(detail))
                    {
                        GroupDevice gropInst = new GroupDevice(_mclLayerProcessor);
                        GroupDevice.Parse(item, gropInst);

                        try
                        {
                            //check join statucs
                            List<CSCommunicateClass.ClientInfo> clients = null;
                            GetGroupMembers(gropInst, out clients);
                            if ((null != clients) && (0 < clients.Count))
                            {
                                gropInst.Joined = true;
                            }
                        }
                        catch (System.Exception ex)
                        {
                        	//nothing to do
                        }
                        return gropInst;
                    }
                }
                throw new GetOnlineGroupExcetion("不存在指定名称的群组");
            }
            else
            {
                throw new GetOnlineGroupExcetion(errorDetail);
            }
        }
        internal void JoinGroup(GroupDevice group, GroupMemberRole role)
        {
            //配置请求包
            ServerRequestPackage reqtPkg = new ServerRequestPackage(true);
            IJoinGroupRequest reqtInterface = reqtPkg.Active_JoinGroupRequest();
            if (group.Ready && !string.IsNullOrEmpty(group.Token))
            {
                reqtInterface.JoinGroup(group.Token, role);
            }
            else
            {
                throw new ArgumentNullException("Group token is null");
            }
            BinTransferLayer binprocessor = _mclLayerProcessor.BinTransferProcessor;
            ServerReplyPackage replyPkg = null;
            try
            {
                replyPkg = binprocessor.SynSendMessage(reqtPkg, 100000);
            }
            catch (System.Exception ex)
            {
                throw new JoinGroupExcetion(ex.ToString());
            }
            IJoinGroupReply replyInterface = replyPkg.Active_JoinGroupReply();
            bool ret;
            string detail;
            replyInterface.JoinGroupRet(out ret, out detail);
            if (true == ret)
            {
                //该部分逻辑内聚到Group内部

                ////加入成功
                //group.Joined = true;
                //try
                //{
                //    //从接口的使用习惯上考虑，立即返回成员列表
                //    group.Referesh();
                //}
                //catch (RefereshGroupInfo ex)
                //{
                //    //虽然刷新讯息抛出错误，但仍可以判定客户端已加入
                //    throw ex;
                //}
            }
            else
            {
                throw new JoinGroupExcetion(detail);
            }
        }
        internal void ExitGroup(GroupDevice group)
        {
            if (!group.Ready || string.IsNullOrEmpty(group.Token))
            {
                throw new ExitGroupExcetion((new NullReferenceException("Group token is null")).ToString());
            }
            //配置请求包
            ServerRequestPackage reqtPkg = new ServerRequestPackage(true);
            IExitGroupRequest reqtInterface = reqtPkg.Active_ExitGroupRequest();
            reqtInterface.ExitGroup(group.Token);

            BinTransferLayer binprocessor = _mclLayerProcessor.BinTransferProcessor;
            ServerReplyPackage replyPkg = null;
            try
            {
                replyPkg = binprocessor.SynSendMessage(reqtPkg, 100000);
            }
            catch (Exception ex)
            {
                throw new ExitGroupExcetion(ex.ToString());
            }
            IExitGroupRely replyInterface = replyPkg.Active_ExitGroupReply();
            bool ret;
            string errorDetail;
            replyInterface.ExitGroupRet(out ret, out errorDetail);
            if (true == ret)
            {
                //该部分逻辑内聚到Group内部

                //group.Joined = false;
                //try
                //{
                //    group.Referesh();
                //}catch(RefereshGroupInfo ex)
                //{
                //    throw ex;
                //}
            }
            else
            {
                throw new ExitGroupExcetion(errorDetail);
            }
        }
        internal void GetGroupMembers(GroupDevice gropDevice, out List<CSCommunicateClass.ClientInfo> onlineGroupMembers)
        {
            //配置数据包
            ServerRequestPackage servReqtPkg = new ServerRequestPackage(true);
            IOnlineGroupClientRequest reqtInterface = servReqtPkg.Active_OnlineGroupClientRequest();
            reqtInterface.OnlineGroupClientRequest(gropDevice.Token);

            BinTransferLayer binprocessor = _mclLayerProcessor.BinTransferProcessor;
            ServerReplyPackage servReplyPkg = null;
            try
            {
                servReplyPkg = binprocessor.SynSendMessage(servReqtPkg, 100000);
            }
            catch (System.Exception ex)
            {
                throw new GetOnlineGroupClientExcetion(ex.ToString());
            }
            IOnlineGroupClientReply replyInterface = servReplyPkg.Active_OnlineGroupClientReply();
            bool ret;
            string errorDetail;
            try
            {
                replyInterface.OnlineGroupClientRet(out ret, out errorDetail, out onlineGroupMembers);
            }
            catch (Exception ex)
            {
                throw new GetOnlineGroupClientExcetion(ex.ToString());
            }
            if (true == ret)
            {
                //nothing to do
            }
            else
            {
                throw new GetOnlineGroupClientExcetion(errorDetail);
            }
        }

        #region BinLayer消息通知
        internal void ServerMessageRecived(ServerReplyPackage servReplyPkg)
        {
            IRadioMessageIncomming replyInterface = servReplyPkg.Active_RadioMessageIncomming();
            _mclLayerProcessor.RadioTransferMessageRecived(replyInterface.RadioPkg as C2CRadioPackage);
        }
        internal void ServerMessageRecived(ServerRequestPackage servReqtPkg, ServerReplyPackage servReplyPkg)
        {
            throw new NotImplementedException();
        }
        #endregion

        private MiddlewareCorelogicLayer _mclLayerProcessor = null;
    }
    #endregion
}
