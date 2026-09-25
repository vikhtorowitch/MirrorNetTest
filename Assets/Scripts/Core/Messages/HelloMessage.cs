using Mirror;

namespace MirrorNetTest.Core.Messages
{
    public struct HelloMessage: NetworkMessage
    {
        public string Message;
    }
}