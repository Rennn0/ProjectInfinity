using Infinite.White.Src.Networking.Client;
using Infinite.White.Src.Networking.Server;
using Infinite.White.Src.Networking.Shared;
using MessagePack;

public static class RpcExample
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class Req
    {
        public string? Request { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class Res
    {
        public string? Response { get; set; }
    }
    public class TestServer : RpcServer<Req, Res>
    {
        public TestServer(RpcServerCreationOptions options) : base(options)
        { }

        protected override Res HandleRequest(RpcMessage<Req> message)
        {
            return new Res { Response = message.PayloadFrame + DateTime.Now.ToString() };
        }
    }
    public static void RunServer()
    {
        Console.WriteLine("running server");
        TestServer server = new TestServer(new RpcServerCreationOptions("localhost", 5555, false));
        // server.MessageReceived += (object? sender, RpcMessage<Req> message) =>
        // {
        //     Console.WriteLine("[RunServer] req received, iden {0}, payload {1}", message.IdentityFrame, message.PayloadFrame?.Request);
        // };
        server.Start();
    }

    public static void RunClinet()
    {
        Task.Factory.StartNew(() =>
        {
            Console.WriteLine("running client");
            RpcClient<Req, Res> client = new RpcClient<Req, Res>("tcp://localhost:5555", new Req { Request = "zz" });
        });
    }

}