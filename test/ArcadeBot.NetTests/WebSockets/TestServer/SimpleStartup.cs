using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text;
using ArcadeBot.Net.Websockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ArcadeBot.NetTests.WebSockets.TestServer;

public class SimpleStartup
{
    [SuppressMessage("Microsoft.Reliability", "IDE0060", Justification = "Required boilerplate")]
    public void ConfigureServices(IServiceCollection services)
    {

    }
    public void Configure(IApplicationBuilder app)
    {
        app.UseWebSockets();
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await SendResponse(webSocket,
                        SocketResponse.TextMessage($"Hello, you are connected to '{nameof(SimpleStartup)}'"));
                    await HandleRequest(webSocket, context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await next();
            }
        });
    }

    protected virtual async Task HandleRequest(WebSocket webSocket, HttpContext context)
    {
        while (true)
        {
            var request = await ReadRequest(webSocket);
            var result = request.result;
            if (result.CloseStatus.HasValue)
            {
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                return;
            }

            if (request.message!.MessageType == WebSocketMessageType.Text)
                await HandleTextRequest(webSocket, context, request.message);

            if (request.message.MessageType == WebSocketMessageType.Binary)
                await HandleBinaryRequest(webSocket, context, request.message);
        }
    }

    protected virtual Task HandleTextRequest(WebSocket webSocket, HttpContext context, SocketResponse request)
    {
        var msg = (request.Text ?? string.Empty).Trim().ToLower();

        return msg switch
        {
            "ping" => SendResponse(webSocket, SocketResponse.TextMessage("pong")),
            not null when msg.StartsWith("echo_fast") => SendEcho(webSocket, request.Text!, false),
            not null when msg.StartsWith("echo") => SendEcho(webSocket, request.Text!, true),
            "close-me" => webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "normal closure", CancellationToken.None),
            _ => throw new NotSupportedException($"Request: '{msg}' is not supported"),
        };
    }

    protected virtual Task HandleBinaryRequest(WebSocket webSocket, HttpContext context, SocketResponse request)
    {
        return SendEcho(webSocket, request.Binary!);
    }

    protected virtual async Task<(WebSocketReceiveResult result, SocketResponse? message)> ReadRequest(WebSocket webSocket)
    {
        var buffer = new ArraySegment<byte>(new byte[8192]);

        using var ms = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
            if (result.CloseStatus.HasValue)
                return (result, null);

            if (buffer.Array != null)
                ms.Write(buffer.Array, buffer.Offset, result.Count);
        } while (!result.EndOfMessage);

        ms.Seek(0, SeekOrigin.Begin);

        SocketResponse message;
        if (result.MessageType == WebSocketMessageType.Text)
        {
            var data = GetEncoding().GetString(ms.ToArray());
            message = SocketResponse.TextMessage(data);
        }
        else
        {
            var data = ms.ToArray();
            message = SocketResponse.BinaryMessage(data);
        }

        return (result, message);
    }

    protected virtual async Task SendResponse(WebSocket webSocket, SocketResponse message)
    {
        if (message.MessageType == WebSocketMessageType.Binary)
        {
            await webSocket.SendAsync(
                new ArraySegment<byte>(message.Binary!, 0, message.Binary!.Length),
                message.MessageType,
                true,
                CancellationToken.None);
            return;
        }

        if (message.MessageType == WebSocketMessageType.Text)
        {
            var encoding = GetEncoding();
            var bytes = encoding.GetBytes(message.Text!);
            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes, 0, bytes.Length),
                message.MessageType,
                true,
                CancellationToken.None);
        }
    }

    protected virtual Encoding GetEncoding()
    {
        return Encoding.UTF8;
    }


    private async Task SendEcho(WebSocket webSocket, string msg, bool slowdown)
    {
        if (slowdown)
            await Task.Delay(100);
        await SendResponse(webSocket, SocketResponse.TextMessage(msg));
    }

    private async Task SendEcho(WebSocket webSocket, byte[] msg)
    {
        await SendResponse(webSocket, SocketResponse.BinaryMessage(msg));
    }
}