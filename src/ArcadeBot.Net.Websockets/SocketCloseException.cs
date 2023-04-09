using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeBot.Net.WebSockets
{
    public class SocketCloseException : Exception
    {
        public SocketCloseException(string? message): base(message)
        {
            
        }
    }
}