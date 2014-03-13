using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ProtoBuf;

using ProtocolLibrary.CCProtocol;
using ProtocolLibrary.CSProtocol;

using Middleware.LayerProcessor;
using Middleware.Communication.CommunicationConfig;
using Middleware.Interface;

namespace Middleware.Device
{
    public class GroupDevice : BaseDevice, ICCSerializeOperat<CCCommunicateClass.Seria_GroupDevice>
    {

        public List<ClientDevice> Members
        {
            get
            {
                if (Joined)
                {
                    if (null != _members)
                    {
                        return _members;
                    }
                    else
                    {
                        try
                        {
                            //耗时操作
                            __refereshGroupMembersInfo();
                        }
                        catch (System.Exception ex)
                        {
                            throw ex;
                        }
                        return _members;
                    }
                }
                else
                {
                    throw new GetOnlineGroupClientExcetion("未成为群组成员");
                }
            }
        }

        public void Referesh()
        {
            if (_Joined)
            {
                try
                {
                    __refereshGroupMembersInfo();
                }
                catch (Exception ex)
                {
                    throw new GetOnlineGroupClientExcetion(ex.ToString());
                }
            }
            //do others.
        }

        public bool Joined
        {
            get { return _Joined; }
            internal set { _Joined = value; }
        }

        internal GroupDevice() 
        {
            Joined = false;
            Ready = false;

            _canOperation = false;
            _corelogicProcessor = null;
        }

        internal GroupDevice(MiddlewareCorelogicLayer corelogicProcessor)
        {
            Joined = false;
            Ready = false;
            SetupLocalOperator(corelogicProcessor);
        }

        internal void Specific(GroupDevice obj)
        {
            base.Detail = obj.Detail;
            base.Token = obj.Token;

            Joined = obj.Joined;
            Ready = obj.Ready;
            try
            {
                _members = obj.Members;
            }
            catch (Exception ex)
            {
                _members = null;
            }

        }

        internal void Join(GroupMemberRole role)
        {
            if (false == _Joined)
            {
                GroupCommunicateLayer gclprocessor = _corelogicProcessor.GroupCommunicateProcessor;
                try
                {
                    gclprocessor.JoinGroup(this, role);
                    _Joined = true;
                    __refereshGroupMembersInfo();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        internal void Exit()
        {
            if (true == _Joined)
            {
                GroupCommunicateLayer gclprocessor = _corelogicProcessor.GroupCommunicateProcessor;
                try
                {
                    gclprocessor.ExitGroup(this);
                    _Joined = false;

                    _members.Clear();
                    _members = null;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        internal static void Parse(CSCommunicateClass.GroupInfo item, GroupDevice gropInst)
        {
            gropInst.Detail = item.Detail;
            gropInst.Token = item.Token;
            gropInst.Ready = true;
        }

        internal void SetupLocalOperator(MiddlewareCorelogicLayer corelogicProcessor)
        {
            _corelogicProcessor = corelogicProcessor;
            if (null != _corelogicProcessor)
            {
                _canOperation = true;
            }
        }

        internal bool CanOperation
        {
            get { return _canOperation; }
        }

        private void __refereshGroupMembersInfo()
        {
            if (_Joined)
            {
                List<CSCommunicateClass.ClientInfo> _newBaseDeviceInfoList = null;
                GroupCommunicateLayer gclprocessor = _corelogicProcessor.GroupCommunicateProcessor;
                try
                {
                    gclprocessor.GetGroupMembers(this, out _newBaseDeviceInfoList);
                }
                catch (System.Exception ex)
                {
                    _members = null;
                    throw new GetOnlineGroupClientExcetion(ex.ToString());
                }
                List<ClientDevice> newMembersList = new List<ClientDevice>();
                foreach (CSCommunicateClass.ClientInfo item in _newBaseDeviceInfoList)
                {
                    ClientDevice cd = new ClientDevice();
                    ClientDevice.Parse(item, cd);
                    newMembersList.Add(cd);
                }
                _members = newMembersList;
            }
            else
            {
                throw new GetOnlineGroupClientExcetion("未加入该群组");
            }
        }

        #region ICCSerializeOperat<CCCommunicateClass.Seria_GroupDevice>
        public void ParseSerializeData(CCCommunicateClass.Seria_GroupDevice obj)
        {
            base.ParseSerializeData(obj as CCCommunicateClass.Seria_Device);
        }

        public new CCCommunicateClass.Seria_GroupDevice ExportSerializeData()
        {
            CCCommunicateClass.Seria_GroupDevice ret = new CCCommunicateClass.Seria_GroupDevice(base.ExportSerializeData());
            return ret;
        }
        #endregion

        public static GroupDevice Empty
        {
            get
            {
                return new GroupDevice();
            }
        }

        public bool Ready = false;
        public GroupOperatErrorExcetion excetion = null;
        private bool _canOperation = false;
        private bool _Joined = false;
        private List<ClientDevice> _members = new List<ClientDevice>();
        private MiddlewareCorelogicLayer _corelogicProcessor = null;
    }
}
