if (args[0] == "--role" && args[1] == "server")
{
    RpcExample.StartServer("tcp://*:5555");
    Console.ReadKey();
}
else
{
    Action<string> send = RpcExample.StartClient("tcp://localhost:5555");
    ulong counter = 0;
    ulong fps = 0;
    var lastTime = DateTime.UtcNow;
    while (true)
    {
        send(DateTime.Now.ToString());
        counter++;
        var now = DateTime.UtcNow;
        if ((now - lastTime).TotalSeconds >= 1.0)
        {
            fps = counter;
            counter = 0;
            lastTime = now;

            Console.WriteLine($"FPS: {fps}");
        }
    }
}
