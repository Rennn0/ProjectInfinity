using System.Diagnostics;
using Infinite.White.Src.Networking.Client;
using Infinite.White.Src.Networking.Server;
using Infinite.White.Src.Networking.Shared;

public static class RpcExample
{
    public class Req
    {
        public string? Request { get; set; }
    }

    public class Res
    {
        public string? Response { get; set; }
    }
    public class TestServer : RpcServer<Req, Res>
    {
        public TestServer(RpcServerCreationOptions options) : base(options)
        { }
    }
    public static void RunServer()
    {
        Task.Factory.StartNew(() =>
        {
            Console.WriteLine("running server");
            TestServer server = new TestServer(new RpcServerCreationOptions("localhost", 5555));
            server.MessageReceived += (object? sender, RpcMessage<Req> message) =>
            {
                Console.WriteLine("[RunServer] req received, iden {0}, payload {1}", message.IdentityFrame, message.PayloadFrame);
            };
        });
    }

    public static void RunClinet()
    {
        Task.Factory.StartNew(() =>
        {
            Console.WriteLine("running client");
            RpcClient client = new RpcClient("tcp://localhost:5555");
        });
    }

}