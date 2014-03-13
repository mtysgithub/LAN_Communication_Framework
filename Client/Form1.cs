using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Middleware;
using Middleware.Device;
using Middleware.Communication;
using Middleware.Communication.CommunicationConfig;
using Middleware.Communication.Package;
using Middleware.Communication.Tcp;

using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using System.Threading.Tasks;
using System.Xml;
using Middleware.Communication.EndPoint.Tcp;

namespace Client
{

    public partial class Form1 : Form
    {
        private MiddlewareDevice localDevice = new MiddlewareDevice();
        public const string ipAddress = "192.168.3.125";
        public const int port = 10086;

        #region  opRulesList
        public const string FsmListChange = "FsmListChange";
        public const string FsmStateChange = "FsmStateChange";
        public const string EventButtonChange = "EventButtonChange";
        public const string FsmList = "FsmList";
        public const string FsmState = "FsmState";
        public const string EventButtonList = "EventButtonList";
        public const string Log = "Log";
        #endregion

        #region opredRulesList
        public const string RequestFsmList = "RequestFsmList";
        public const string RequestFsmState = "RequestFsmState";
        public const string RequestEventButton = "RequestEventButton";
        public const string FsmLoad = "FsmLoad";
        public const string FsmUnload = "FsmUnload";
        public const string FsmStart = "FsmStart";
        public const string FsmPause = "FsmPause";
        public const string FsmReset = "FsmReset";
        public const string FsmEvent = "FsmEvent";
        #endregion

        List<string> opRulesList = new List<string>();
        List<string> opredRulesList = new List<string>();

        Dictionary<string, Type> sendTypeTable = new Dictionary<string, Type>();
        Dictionary<string, byte[]> sendDefaultValues = new Dictionary<string, byte[]>();
        Dictionary<string, string> sendDescriptions = new Dictionary<string, string>();

        Dictionary<string, Type> receivedTypeTable = new Dictionary<string, Type>();
        Dictionary<string, object> receivedDefaultValues = new Dictionary<string, object>();
        Dictionary<string, string> receivedDescriptions = new Dictionary<string, string>();

        string initDetail = "";
        string getGroupDetail = "";
        string createGroupDetail = "";
        string messageSended = "";
        List<GroupDevice> groupDeviceList = new List<GroupDevice>();
        List<ClientDevice> clientDeviceList = new List<ClientDevice>();
        ParamPackage paramPackage = null;
        GroupDevice joinedGroupDevice = null;
        RequestPackage requestPackage = null;
        AsynReponseHandler asynReponseHandler = null;

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += __FormClosing;
        }

        private void __FormClosing(object sender, FormClosingEventArgs e)
        {
            if (localDevice.Online)
            {
                localDevice.Dispose();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            opRulesList.Add(FsmListChange);
            opRulesList.Add(FsmStateChange);
            opRulesList.Add(EventButtonChange);
            opRulesList.Add(FsmList);
            opRulesList.Add(FsmState);
            opRulesList.Add(EventButtonList);
            opRulesList.Add(Log);

            opredRulesList.Add(RequestFsmList);
            opredRulesList.Add(RequestFsmState);
            opredRulesList.Add(RequestEventButton);
            opredRulesList.Add(FsmLoad);
            opredRulesList.Add(FsmUnload);
            opredRulesList.Add(FsmStart);
            opredRulesList.Add(FsmPause);
            opredRulesList.Add(FsmReset);
            opredRulesList.Add(FsmEvent);

            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            var communicationType = new System.Collections.ArrayList(Enum.GetValues(typeof(CommunicatType))).ToArray();
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.AddRange(communicationType);
            this.comboBox1.SelectedIndex = 1;
            this.comboBox2.Items.Clear();
            this.comboBox2.Items.AddRange(communicationType);
            this.comboBox2.SelectedIndex = 1;
            this.comboBox5.Items.Clear();
            this.comboBox5.Items.AddRange(communicationType);
            this.comboBox5.SelectedIndex = 1;
            this.comboBox6.Items.Clear();
            this.comboBox6.Items.AddRange(communicationType);
            this.comboBox6.SelectedIndex = 1;
            this.comboBox10.Items.Clear();
            this.comboBox10.Items.AddRange(communicationType);
            this.comboBox10.SelectedIndex = 1;
            this.comboBox11.Items.Clear();
            this.comboBox11.Items.AddRange(communicationType);
            this.comboBox11.SelectedIndex = 1;

            var groupMemberRole = new System.Collections.ArrayList(Enum.GetValues(typeof(GroupMemberRole))).ToArray();
            this.comboBox4.Items.Clear();
            this.comboBox4.Items.AddRange(groupMemberRole);
            this.comboBox4.SelectedIndex = 2;

            var opRules = new System.Collections.ArrayList(opRulesList).ToArray();
            var opredRules = new System.Collections.ArrayList(opredRulesList).ToArray();
            this.comboBox8.Items.Clear();
            this.comboBox8.Items.AddRange(opRules);
            this.comboBox8.Items.AddRange(opredRules);
            this.comboBox8.SelectedIndex = 0;
            this.comboBox9.Items.Clear();
            this.comboBox9.Items.AddRange(opRules);
            this.comboBox9.Items.AddRange(opredRules);
            this.comboBox9.SelectedIndex = 0;

            var reply_info = new System.Collections.ArrayList(Enum.GetValues(typeof(ReplyPackage.Middleware_ReplyInfo))).ToArray();
            this.comboBox13.Items.Clear();
            this.comboBox13.Items.AddRange(reply_info);
            this.comboBox13.SelectedIndex = 0;

            localDevice.RemotReqtRecived += new RemotReqtRecivedHandler(RequestPackage_Received);
            localDevice.RemotRadioRecived += new RemotRadioRecivedHandler(RadioPackage_Received);
            //localDevice.AsynReponseRecived += new AsynReponseHandler(CallBack_MessageReceived);
            asynReponseHandler += new AsynReponseHandler(CallBack_Received);
        }

        ParamPackage PackMessage()
        {
            sendTypeTable.Clear();
            sendDefaultValues.Clear();
            sendDescriptions.Clear();

            List<string> _wilSendList = new List<string>();
            _wilSendList.Add(messageSended);
            sendDefaultValues.Add("value", Encoding.UTF8.GetBytes(messageSended));

            //System.Xml.XmlDocument wilSendInfo = new System.Xml.XmlDocument();
            //sendDefaultValues.Add("xml", wilSendInfo);

            //List<object> _wilSendList2 = new List<object>();
            //_wilSendList2.Add(wilSendInfo);

            //sendDefaultValues.Add("objects", _wilSendList2);

            //sendTypeTable.Add("value", typeof(int));
            //sendDefaultValues.Add("value", 123);
            paramPackage = new ParamPackage("param", sendDefaultValues);
            return paramPackage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                localDevice.Initialization(new MiddlewareTcpEndPoint(ipAddress, port), initDetail, opRulesList, opredRulesList);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());      	
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (this.textBox1.Text != "")
            {
                initDetail = this.textBox1.Text;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (this.textBox3.Text != "")
            {
                getGroupDetail = this.textBox3.Text;
            }
        }

        //获取Group
        private void button2_Click(object sender, EventArgs e)
        {
            GroupDevice groupDevice = localDevice.GetGroup(getGroupDetail, (CommunicatType)comboBox1.SelectedItem);
            if (groupDevice != null)
            {
                groupDeviceList.Add(groupDevice);
                if (!comboBox3.Items.Contains(groupDevice.Detail))
                {
                    comboBox3.Items.Add(groupDevice.Detail);
                }
                if (!comboBox7.Items.Contains(groupDevice.Detail))
                {
                    comboBox7.Items.Add(groupDevice.Detail);
                }
                if (!comboBox14.Items.Contains(groupDevice.Detail))
                {
                    comboBox14.Items.Add(groupDevice.Detail);
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (this.textBox2.Text != "")
            {
                createGroupDetail = this.textBox2.Text;
            }
        }

        //创建Group
        private void button3_Click(object sender, EventArgs e)
        {
            GroupDevice groupDevice = localDevice.CreateGroup(createGroupDetail, (CommunicatType)comboBox2.SelectedItem);
            if (groupDevice != null)
            {
                groupDeviceList.Add(groupDevice);
                if (!comboBox3.Items.Contains(groupDevice.Detail))
                {
                    comboBox3.Items.Add(groupDevice.Detail);
                }
                if (!comboBox7.Items.Contains(groupDevice.Detail))
                {
                    comboBox7.Items.Add(groupDevice.Detail);
                }
                if (!comboBox14.Items.Contains(groupDevice.Detail))
                {
                    comboBox14.Items.Add(groupDevice.Detail);
                }
            }
        }


        //加入Group
        private void button4_Click(object sender, EventArgs e)
        {
            GroupDevice targetGroup = null;
            foreach (GroupDevice item in groupDeviceList)
            {
                if (item.Detail == comboBox3.SelectedItem.ToString())
                {
                    targetGroup = item;
                }
            }
            if (targetGroup != null)
            {
                localDevice.JoinGroup(targetGroup, (GroupMemberRole)comboBox4.SelectedItem, (CommunicatType)comboBox5.SelectedItem);
                joinedGroupDevice = targetGroup;
            }
        }

        //退出Group
        private void button5_Click(object sender, EventArgs e)
        {
            GroupDevice targetGroup = null;
            foreach (GroupDevice item in groupDeviceList)
            {
                if (item.Detail == comboBox7.SelectedItem.ToString())
                {
                    targetGroup = item;
                }
            }
            if (targetGroup != null)
            {
                localDevice.ExitGroup(targetGroup, (CommunicatType)comboBox6.SelectedItem);
                joinedGroupDevice = null;
            }
        }

        //以下三个函数是获取界面上三个“ParamPackage”后面输入框中字符，把获得的字符打到ParamPackage中
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            messageSended = this.textBox5.Text;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            messageSended = this.textBox6.Text;
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            messageSended = this.textBox7.Text;
        }


        //创建组通讯包并且广播
        private void button8_Click(object sender, EventArgs e)
        {
            ParamPackage packageSended = PackMessage();
            GroupDevice targetGroup = null;
            foreach (GroupDevice item in groupDeviceList)
            {
                if (item.Detail == comboBox14.SelectedItem.ToString())
                {
                    targetGroup = item;
                }
            }
            string radioOp = this.comboBox8.SelectedItem.ToString();
            //GroupComunicatePackage groupComunicatePackage = localDevice.CreateGroupCommunicatePackage(radioOp, packageSended, targetGroup, (CommunicatType)comboBox10.SelectedItem);
            GroupComunicatePackage groupComunicatePackage = localDevice.CreateRadioCommunicatePackage(radioOp, packageSended, targetGroup);
            localDevice.Radio(groupComunicatePackage);
        }

        private void comboBox12_Click(object sender, EventArgs e)
        {
            if (joinedGroupDevice != null && joinedGroupDevice.Members.Count > 0)
            {
                joinedGroupDevice.Referesh();
                clientDeviceList = joinedGroupDevice.Members;
                comboBox12.Items.Clear();
                for (int i = 0; i < clientDeviceList.Count; i++)
                {
                    comboBox12.Items.Add(clientDeviceList[i].Detail);
                }
            }
        }

        //AsynSendRequest
        private void button6_Click(object sender, EventArgs e)
        {
            ParamPackage packageSended = PackMessage();
            ClientDevice targetClient = null;
            foreach (ClientDevice item in clientDeviceList)
            {
                if (item.Detail == comboBox12.SelectedItem.ToString())
                {
                    targetClient = item;
                }
            }
            string radioOp = this.comboBox8.SelectedItem.ToString();
            string commuicateName = this.comboBox9.SelectedItem.ToString();
            RequestCommunicatePackage requestCommunicatePackage = localDevice.CreateRequestCommunicatePackage(commuicateName, (CommunicatType)comboBox11.SelectedItem, packageSended, targetClient, checkBox1.Checked, asynReponseHandler);
            localDevice.AsynSendRequest(requestCommunicatePackage);
        }


        //SynSendRequest
        private void button7_Click(object sender, EventArgs e)
        {
            ParamPackage packageSended = PackMessage();
            ClientDevice targetClient = null;
            foreach (ClientDevice item in clientDeviceList)
            {
                if (item.Detail == comboBox12.SelectedItem.ToString())
                {
                    targetClient = item;
                }
            }
            string commuicateName = this.comboBox9.SelectedItem.ToString();
            RequestCommunicatePackage requestCommunicatePackage = localDevice.CreateRequestCommunicatePackage(commuicateName, (CommunicatType)comboBox11.SelectedItem, packageSended, targetClient, checkBox1.Checked, asynReponseHandler);
            try
            {
                localDevice.SynSendRequest(requestCommunicatePackage, int.Parse(this.textBox4.Text));
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            ReplyPackage replyPkg = requestCommunicatePackage.ResponsePackage;
            if (null != replyPkg)
            {
                //List<string> list = replyPkg.Values<string>("value");
                string replyString = replyPkg.ReplyState.ToString();
                Console.WriteLine(replyString.ToString());
            }
        }

        //AsynFeedback
        private void button10_Click(object sender, EventArgs e)
        {
            ParamPackage packageSended = PackMessage();
            ReplyCommunicatePackage replyCommunicatePackage = localDevice.CreateReplyCommunicatePackage(requestPackage, (ReplyPackage.Middleware_ReplyInfo)comboBox13.SelectedItem, packageSended);
            localDevice.AsynFeedbackCommunicateReplyMessage(replyCommunicatePackage);
        }

        //SynFeedback
        private void button9_Click(object sender, EventArgs e)
        {
            //             ParamPackage packageSended = PackMessage();
            //             ReplyCommunicatePackage replyCommunicatePackage = localDevice.CreateReplyCommunicatePackage(requestPackage, (ReplyPackage.Middleware_ReplyInfo)comboBox13.SelectedItem, packageSended);
            //             localDevice.SynFeedbackCommunicateReplyMessage(replyCommunicatePackage, int.Parse(this.textBox8.Text));
        }

        private void RequestPackage_Received(RequestPackage p)
        {
            //requestPackage = p;
            //string[] receivedDetails = new string[p.ParamDefalutValues.Values.Count];

            ////List<System.Int64> retList = p.Values<System.Int64>("value");
            ////Console.WriteLine(retList.ToString());

            //p.ParamDefalutValues.Values.CopyTo(receivedDetails, 0);
            //string log = "";
            //log = "From device: " + p.SourDevice.Detail + "\n" + "OperatName: " + p.OperatName + "\n" + "Message: " + receivedDetails[0];
            //this.richTextBox1.Text = log;

            ParamPackage parm = new ParamPackage("x86-reply", new Dictionary<string, byte[]>());
            parm.ParamDefalutValues.Add("value", Encoding.UTF8.GetBytes("x86-reply"));
            ReplyCommunicatePackage replyPkg = localDevice.CreateReplyCommunicatePackage(p, ReplyPackage.Middleware_ReplyInfo.S_OK, parm);
            localDevice.AsynFeedbackCommunicateReplyMessage(replyPkg);
        }

        private void RadioPackage_Received(RadioPackage p)
        {
            Console.WriteLine("Radio: " + p.RadioName);
            //List<string> receivedDetails = new List<string>();
            ////receivedDetails = p.Values<string>("value");
            //string receivedStringInfo = p.Value<string>("value");
            //receivedDetails.Add(receivedStringInfo);

            ////XmlDocument caonima = p.Value<XmlDocument>("xml");

            ////List<XmlDocument> caonima2 = p.Values<XmlDocument>("objects");

            //string log = "";
            //log = "TargetGroup: " + p.GroupInfo.Detail + "\n" + "RadioName: " + p.RadioName + "\n" + "Message: " + ((receivedDetails.Count != 0)?(receivedDetails[0]):(""));
            //this.richTextBox1.Text = log;

            //GroupDevice _group = localDevice.GetGroup("g1",
            //Middleware.Communication.CommunicationConfig.CommunicatType.Synchronization);

            //_group.Referesh();
            //List<ClientDevice> _clients = _group.Members;
            //foreach(ClientDevice x862 in _clients)
            //{
            //    if (x862.Detail.Equals("x862"))
            //    {
            //        Dictionary<string, object> dicr = new Dictionary<string, object>();
            //        dicr.Add("value", "unity");
            //        RequestCommunicatePackage reqtPkg = localDevice.CreateRequestCommunicatePackage("opr",
            //            CommunicatType.Synchronization, new ParamPackage("unity-request", dicr), x862, true,
            //            null);

            //        localDevice.SynSendRequest(reqtPkg, 1000);
            //        Console.WriteLine(reqtPkg.ResponsePackage.ReplyState);
            //    }
            //}
        }

        private void CallBack_Received(CommunicatePackage sender, AsynResponseEventArg evtAr)
        {
            byte[][] receivedDetails = new byte[evtAr.RemotDeviceReplyPackage.ParamDefalutValues.Values.Count][];
            evtAr.RemotDeviceReplyPackage.ParamDefalutValues.Values.CopyTo(receivedDetails, 0);
            string log = "";

            log = "From device: " + sender.TargetDevice.Detail + "\n" + "CommunicationName" + sender.CommunicationName + "\n" + "ReplyState: " +
                evtAr.RemotDeviceReplyPackage.ReplyState.ToString() + "\n" + "Message: " +
                Encoding.UTF8.GetString(receivedDetails[0]);

            this.richTextBox1.Text = log;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (localDevice.Online)
            {
                localDevice.Dispose();
            }
        }

        private void comboBox14_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox12_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        #region 测试
        private Hik.Threading.Timer _perRaido = null;
        private void button12_Click(object sender, EventArgs e)
        {
            if (null != _perRaido)
            {
                _perRaido.WaitToStop();
                _perRaido = null;
            }
            else
            {
                _perRaido = new Hik.Threading.Timer(100);
                _perRaido.Elapsed += __PerRadio_Elapsed;
                _perRaido.Start();
            }
        }
        private void __PerRadio_Elapsed(object sender, EventArgs e)
        {
            ParamPackage packageSended = PackMessage();
            GroupDevice targetGroup = null;
            foreach (GroupDevice item in groupDeviceList)
            {
                if (item.Detail == comboBox14.SelectedItem.ToString())
                {
                    targetGroup = item;
                }
            }
            string radioOp = this.comboBox8.SelectedItem.ToString();
            GroupComunicatePackage groupComunicatePackage = localDevice.CreateRadioCommunicatePackage(radioOp, packageSended, targetGroup);
            localDevice.Radio(groupComunicatePackage);
        }
        #endregion
    }
}
