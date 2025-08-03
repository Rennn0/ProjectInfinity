using System.Text;
using MessagePack;
using NetMQ;
using NetMQ.Sockets;

namespace Infinite.White.Src.Networking.Client
{
    public class RpcClient<TRequest, TResponse>
    {
        public RpcClient(string address, TRequest req)
        {
            Thread.Sleep(2000);
            RequestSocket socket = new RequestSocket();
            socket.Connect(address);
            int counter = 0;
            do
            {
                NetMQMessage message = new NetMQMessage();
                string identityFrame = "luka";
                byte[] payloadFrame = MessagePackSerializer.Serialize(req);
                message.Append(identityFrame);
                message.AppendEmptyFrame();
                message.Append(payloadFrame);
                Console.WriteLine("[RpcClient] sending {0}", message.FrameCount);

                socket.SendMultipartMessage(message);
                NetMQMessage xx = socket.ReceiveMultipartMessage();
                string iden = Encoding.UTF8.GetString(xx[0].Buffer);
                string payload = Encoding.UTF8.GetString(xx[2].Buffer);
                Console.WriteLine("[RpcClient] received {0}", xx.FrameCount);
                Console.WriteLine("[RpcClient] received indentity {0}", iden);
                Console.WriteLine("[RpcClient] received payload {0}", payload);
                TResponse response = MessagePackSerializer.Deserialize<TResponse>(xx[2].Buffer);

            } while (++counter < 500_000);
        }
    }
}