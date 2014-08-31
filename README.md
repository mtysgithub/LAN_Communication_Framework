LAN_Communication_Framework
===========================

一个基于Tcp协议封装的局域网通讯框架

适用于快速搭建局域网内的星型拓扑联机程序，客户端程序使用 Middleware API创建出一个网络映像，不同的网络映像统一使用 Server 分发消息，API 封装了 Server 的可见性，使得应用程序可以以点对点传输方式使用API

基于.Net 20 编译，使用Unity 进行联机游戏开发是可行的

跨平台特性：使用Google protocol 封装了对象序列化协议，消除了Mono .net 与 Microsoft .net 之间二进制序列化的差异，现在使用Unity直接向非Unity应用程序共享对象序列化数据是可行的
