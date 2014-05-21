using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Middleware.Device;
using Middleware.Communication;
using Middleware.Communication.Message;

namespace Middleware.LayerProcessor.Interfcace
{
    interface IMiddlewareMessenger
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="coreLogicProcessor"></param>
        /// <param name="groupCommunicateLayer"></param>
        /// <param name="middlewareCommunicateLayer"></param>
        void Initialize(MiddlewareCorelogicLayer coreLogicProcessor,
                                GroupCommunicateLayer groupCommunicateLayer,
                                MiddlewareCommunicateLayer middlewareCommunicateLayer, MessageRecivedHandler handler);

        /// <summary>
        /// 释放模块
        /// </summary>
        void Release();

        /// <summary>
        /// 监听一种设备消息
        /// </summary>
        /// <param name="messenger">目标设备</param>
        /// <param name="typMsg">消息定义</param>
        void Listen(ClientDevice messenger, BaseMessageType typMsg);

        /// <summary>
        /// 注册本设备支持的消息
        /// </summary>
        /// <param name="typMsg">消息定义</param>
        void RegistMessage(BaseMessageType typMsg, Type t_Msg);

        /// <summary>
        /// 创建一条消息
        /// </summary>
        /// <param name="typMsg">消息定义</param>
        /// <returns></returns>
        BaseMessage CreateMessage(BaseMessageType typMsg);

        /// <summary>
        /// 发送一条消息
        /// </summary>
        /// <param name="msg">消息包</param>
        void SendMessage(BaseMessage msg);

        /// <summary>
        /// 事件接收回调
        /// </summary>
        event MessageRecivedHandler MessageRecived;
    }
}
