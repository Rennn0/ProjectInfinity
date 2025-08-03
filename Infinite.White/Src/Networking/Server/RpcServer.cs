using System.Diagnostics;
using System.Text;
using Infinite.White.Src.Networking.Exceptions;
using Infinite.White.Src.Networking.Shared;
using MessagePack;
using NetMQ;
using NetMQ.Sockets;

namespace Infinite.White.Src.Networking.Server
{
    public abstract class RpcServer<TRequest, TResponse>
        where TRequest : new()
        where TResponse : new()
    {
        public EventHandler<RpcMessage<TRequest>>? MessageReceived;
        private readonly ThreadLocal<ResponseSocket> socket;
        private ulong msgCounter;
        protected RpcServer(RpcServerCreationOptions options)
        {
            this.socket = new ThreadLocal<ResponseSocket>(() => GetSocket(ref options), false);
            this.msgCounter = 0;
            if (options.RunImmediate)
                StartServer();
        }
        public void Start() => Task.Factory.StartNew(StartServer);
        protected abstract TResponse HandleRequest(RpcMessage<TRequest> message);
        protected virtual void StartServer()
        {
            ArgumentNullException.ThrowIfNull(socket.Value);
            while (true)
            {
                try
                {
                    List<byte[]> frames = this.socket.Value.ReceiveMultipartBytes();
                    if (frames.Count < 3) throw new RpcServerBadFramesException(frames.Count);
                    RpcMessage<TRequest> message = GetMessage(frames);
                    MessageReceived?.Invoke(this, message);
                    SendResponse(message);
                }
                catch (System.Exception e)
                {
                    Console.WriteLine(e.Message);
                    Thread.Sleep(10000);
                }
            }
        }
        protected virtual void SendResponse(RpcMessage<TRequest> message)
        {
            ArgumentNullException.ThrowIfNull(this.socket.Value);
            ArgumentNullException.ThrowIfNull(message.IdentityFrame);

            Interlocked.Increment(ref this.msgCounter);

            NetMQMessage response = new NetMQMessage();
            TResponse payload = HandleRequest(message);
            byte[] payloadFrame = MessagePackSerializer.Serialize(payload);
            response.Append(message.IdentityFrame);
            response.AppendEmptyFrame();
            response.Append(payloadFrame);
            this.socket.Value.SendMultipartMessage(response);
        }
        protected virtual string GetConnectionString(ref RpcServerCreationOptions options)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("tcp://");
            builder.Append(options.Address);
            builder.Append(':');
            builder.Append(options.Port);
            return builder.ToString();
        }
        protected virtual ResponseSocket GetSocket(ref RpcServerCreationOptions options)
        {
            ResponseSocket socket = new ResponseSocket();
            socket.Bind(GetConnectionString(ref options));
            return socket;
        }

        protected RpcMessage<TRequest> GetMessage(List<byte[]> frames)
        {
            RpcMessage<TRequest> message = new RpcMessage<TRequest>
            {
                IdentityFrame = Encoding.UTF8.GetString(frames[0]),
                PayloadFrame = MessagePackSerializer.Deserialize<TRequest>(frames[2])
            };
            return message;
        }
    }
}