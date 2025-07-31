using Infinite.White.Src.Networking.Server;

public static class RpcExample
{
    public static void StartServer(string address)
    {
        TestServer ts = new TestServer(address);
    }

    public static Action<string> StartClient(string address)
    {
        RpcClient client = new RpcClient(address, "lukito");

        return (frame) => client.Send(frame);
    }
}