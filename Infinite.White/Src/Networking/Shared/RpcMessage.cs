using MessagePack;

namespace Infinite.White.Src.Networking.Shared
{
    public class RpcMessage<TMessage>
    {
        [Key(0)]
        public string? IdentityFrame { get; set; }

        [Key(2)]
        public TMessage? PayloadFrame { get; set; }
    }
}