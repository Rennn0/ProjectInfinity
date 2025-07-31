
using System.Text;
using NetMQ;
using NetMQ.Sockets;

namespace Infinite.White.Src.Networking.Server
{
    public abstract class RpcServer
    {
        private readonly string address;
        private readonly CancellationToken cancellationToken;
        protected RpcServer(string address, CancellationToken? cancellationToken = null)
        {
            this.address = address;
            this.cancellationToken = cancellationToken ?? CancellationToken.None;
            StartServerAsync();
        }

        protected virtual void StartServerAsync()
        {
            try
            {
                using RouterSocket router = new RouterSocket();
                router.Bind(address);
                int counter = 0;
                while (!this.cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine(counter++);
                    string clientId = router.ReceiveFrameString();
                    router.ReceiveFrameString();
                    string requestFrame = router.ReceiveFrameString();

                    string reply = HandleRequest(requestFrame).Result;
                    router.SendMoreFrame(clientId).SendMoreFrame("").SendFrame(reply);
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        protected abstract Task<string> HandleRequest(string frame);
    }

    public class RpcClient
    {
        private readonly DealerSocket socket;
        public RpcClient(string address, string clientId)
        {
            this.socket = new DealerSocket();
            this.socket.Options.Identity = Encoding.UTF8.GetBytes(clientId);
            this.socket.Connect(address);
        }

        public string Send(string frame)
        {
            // Console.WriteLine("ClientSending {0}", frame);
            this.socket.SendMoreFrame("").SendFrame(frame);

            string empty = this.socket.ReceiveFrameString();
            string reply = this.socket.ReceiveFrameString();
            return reply;
        }
    }

    public class TestServer : RpcServer
    {
        public TestServer(string address, CancellationToken? cancellationToken = null) : base(address, cancellationToken)
        { }

        protected override Task<string> HandleRequest(string frame)
        {
            Console.WriteLine(frame);
            return Task.FromResult(DateTime.Now.ToShortDateString());
        }
    }
}