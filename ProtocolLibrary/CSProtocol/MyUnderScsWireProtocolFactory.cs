using Hik.Communication.Scs.Communication.Protocols;

namespace ProtocolLibrary.CSProtocol
{
    public class MyUnderScsWireProtocolFactory : IScsWireProtocolFactory
    {
        public IScsWireProtocol CreateWireProtocol()
        {
            return new MyUnderScsWireProtocol();
        }
    }
}
