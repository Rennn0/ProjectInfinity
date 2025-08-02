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
            new Timer(
                _ => Console.WriteLine(this.msgCounter),
                null,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5));
            StartServer();
        }

        protected virtual void StartServer()
        {
            ArgumentNullException.ThrowIfNull(socket.Value);
            while (true)
            {
                try
                {
                    List<byte[]> frames = this.socket.Value.ReceiveMultipartBytes();
                    Console.WriteLine("[StartServer] received {0} frame", frames.Count);
                    if (frames.Count < 3) throw new RpcServerBadFramesException(frames.Count);
                    string identity = Encoding.UTF8.GetString(frames[0]);
                    string payload = Encoding.UTF8.GetString(frames[2]);
                    // RpcMessage<TRequest> message = GetMessage(frames);
                    Console.WriteLine("[StartServer] has message, {0}, {1}", identity, payload);
                    MessageReceived?.Invoke(this, new RpcMessage<TRequest> { IdentityFrame = identity, PayloadFrame = default });
                    Console.WriteLine("[StartServer] invoked event");
                    SendResponse(new RpcMessage<TRequest> { IdentityFrame = identity, PayloadFrame = default });
                    Console.WriteLine("[StartServer] responding");
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

            Interlocked.Increment(ref this.msgCounter);
            Console.WriteLine("[SendResponse] empty handling {0} msg", this.msgCounter);

            TResponse payload = Activator.CreateInstance<TResponse>();

            NetMQMessage response = new NetMQMessage();
            // byte[] identityFrame = MessagePackSerializer.Serialize(message.IdentityFrame);
            // byte[] payloadFrame = MessagePackSerializer.Serialize(payload);
            // response.Append(identityFrame);
            // response.AppendEmptyFrame();
            // response.Append(payloadFrame);
            response.Append(Encoding.UTF8.GetBytes(message.IdentityFrame!));
            response.AppendEmptyFrame();
            response.Append(Encoding.UTF8.GetBytes("xui tebe"));

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
                IdentityFrame = MessagePackSerializer.Deserialize<string>(frames[0]),
                PayloadFrame = default
                // PayloadFrame = MessagePackSerializer.Deserialize<TRequest>(frames[2])
            };

            return message;
        }
    }
}