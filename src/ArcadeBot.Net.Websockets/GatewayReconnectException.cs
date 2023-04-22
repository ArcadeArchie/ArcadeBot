namespace ArcadeBot.Net.WebSockets
{
    internal class GatewayReconnectException : SocketCloseException
    {

        public GatewayReconnectException(string reason) : base(reason)
        {
            
        }
    }
}