namespace Infinite.White.Src.Networking.Exceptions
{
    public class RpcServerBadFramesException : Exception
    {
        public RpcServerBadFramesException(int actual) :
            base(string.Format("bad frames. expected at least 3 frames, got actual {0} frames ", actual))
        { }
    }
}