using System.Text;
using NetMQ;
using NetMQ.Sockets;

namespace Infinite.White.Src.Networking.Client
{
    public class RpcClient
    {
        public RpcClient(string address)
        {
            Thread.Sleep(2000);
            RequestSocket socket = new RequestSocket();
            socket.Connect(address);
            int counter = 0;
            do
            {
                NetMQMessage message = new NetMQMessage();
                message.Append(Encoding.UTF8.GetBytes("luka"));
                message.AppendEmptyFrame();
                message.Append(Encoding.UTF8.GetBytes("zoroo"));
                Console.WriteLine("sending {0}", message.FrameCount);

                socket.SendMultipartMessage(message);
                NetMQMessage xx = socket.ReceiveMultipartMessage();
                Console.WriteLine(xx.FrameCount);
            } while (++counter < 5);
        }
    }
}