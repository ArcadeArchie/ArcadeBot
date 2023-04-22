using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ArcadeBot.Core;
using ArcadeBot.DTO;
using Microsoft.Extensions.Logging;

namespace ArcadeBot.Net.WebSockets
{
    internal class DiscordWebsocketClient : IDisposable
    {
        public const int ReceiveChunkSize = 16 * 1024;
        public const int SendChunkSize = 4 * 1024;

        public event Func<SocketFrame, Task> ReceivedGatewayEvent
        {
            add { _receivedGatewayEvent.Add(value); }
            remove { _receivedGatewayEvent.Remove(value); }
        }

        private readonly ILogger<DiscordWebsocketClient> _logger;
        private readonly SemaphoreSlim _lock;
        private readonly AsyncEvent<Func<SocketFrame, Task>> _receivedGatewayEvent = new();
        private readonly Uri _remoteSocket;

        private ClientWebSocket? _clientSocket;
        private bool _isDisposed, _isDisconnecting;
        private Task? _recieveTask;
        private MemoryStream? _compressed;
        private DeflateStream? _decompressor;
        private CancellationTokenSource _disconnectTokenSource;
        private CancellationTokenSource? _cancelTokenSource;
        private CancellationToken _cancelToken;

        public DiscordWebsocketClient(ILogger<DiscordWebsocketClient> logger, Uri socketUri)
        {
            _logger = logger;
            _remoteSocket = socketUri;
            _lock = new SemaphoreSlim(1, 1);
            _disconnectTokenSource = new CancellationTokenSource();
            _cancelToken = CancellationToken.None;
        }
        public async Task ConnectAsync()
        {
            //Ensure clean connection
            await DisconnectInternal(false);
            _isDisconnecting = false;

            await _lock.WaitAsync().ConfigureAwait(false);
            _compressed?.Dispose();
            _decompressor?.Dispose();
            _compressed = new MemoryStream();
            _decompressor = new DeflateStream(_compressed, CompressionMode.Decompress);


            _disconnectTokenSource = new CancellationTokenSource();
            _cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_disconnectTokenSource.Token);
            _cancelToken = _cancelTokenSource.Token;

            _clientSocket = new ClientWebSocket
            {
                Options = {
                    KeepAliveInterval = TimeSpan.Zero
                }
            };
            await _clientSocket.ConnectAsync(_remoteSocket, _cancelToken).ConfigureAwait(false);

            _recieveTask = RecieveLoop(_cancelToken);
            _lock.Release();
        }


        private async Task RecieveLoop(CancellationToken token)
        {
            var recieveBuf = new ArraySegment<byte>(new byte[ReceiveChunkSize]);
            try
            {
                ArgumentNullException.ThrowIfNull(_clientSocket);

                while (!token.IsCancellationRequested)
                {
                    var socketResult = await _clientSocket.ReceiveAsync(recieveBuf, token).ConfigureAwait(false);
                    byte[]? result;
                    int resultCount;
                    if (socketResult.MessageType == WebSocketMessageType.Close)
                        throw new SocketCloseException(socketResult.CloseStatusDescription ?? "The remote host send a close");
                    if (socketResult.EndOfMessage)
                    {
                        result = recieveBuf.Array;
                        resultCount = socketResult.Count;
                    }
                    else
                    {
                        using var ms = new MemoryStream();
                        ms.Write(recieveBuf.Array!, 0, socketResult.Count);
                        do
                        {
                            if (token.IsCancellationRequested) return;
                            socketResult = await _clientSocket.ReceiveAsync(recieveBuf, token).ConfigureAwait(false);
                            ms.Write(recieveBuf.Array!, 0, socketResult.Count);
                        } while (socketResult == null || !socketResult.EndOfMessage);
                        result = ms.TryGetBuffer(out var msBuf) ? msBuf.Array : ms.ToArray();
                        resultCount = (int)ms.Length;
                    }

                    var message = await DecodeSocketMessage(result, resultCount, socketResult.MessageType);

                    await _receivedGatewayEvent.InvokeAsync(message).ConfigureAwait(false);
                }
            }
            catch (SocketCloseException)
            {
                await DisconnectAsync();
                if (!_isDisconnecting)
                    await ConnectAsync();
            }
            catch (Exception ex)
            {
                if (_isDisconnecting) //expected exception
                    return;
                await DisconnectAsync();
                _logger.LogCritical(ex, "An error occrued in the recieve loop, closing connection");
#if DEBUG
                throw;
#endif
            }
        }

        #region DisconnectAsync

        public async Task DisconnectAsync(bool isDisposing = false)
        {
            await DisconnectInternal(isDisposing);
        }

        private async Task DisconnectInternal(bool isDisposing)
        {
            if (_isDisconnecting)
                return;
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                _isDisconnecting = true;
                if (_clientSocket == null)
                    return;
                if (!isDisposing)
                {
                    await _clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", _cancelToken);
                    // await _clientSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", _cancelToken);
                }

                _clientSocket.Dispose();
                _clientSocket = null;

                await (_recieveTask ?? Task.Delay(0)).ConfigureAwait(false);
                _recieveTask = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed closing the socket gracefully");
            }
            finally
            {
                _disconnectTokenSource.Cancel(false);
                _isDisconnecting = false;
                _lock.Release();
            }
        }

        #endregion


        #region SendAsync

        public async Task<bool> SendAsync(byte[] data, int index, int count, bool isText)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_clientSocket == null) return false;
                int frameCount = (int)Math.Ceiling((double)count / SendChunkSize);

                for (int i = 0; i < frameCount; i++, index += SendChunkSize)
                {
                    bool isLast = i == (frameCount - 1);

                    var type = isText ? WebSocketMessageType.Text : WebSocketMessageType.Binary;
                    await _clientSocket.SendAsync(new ArraySegment<byte>(data, index, count), type, isLast, _cancelToken).ConfigureAwait(false);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured trying to send the message");
                return false;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<bool> SendAsync(SocketFrame message)
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(message);
            return await SendAsync(payload, 0, payload.Length, true);
        }

        #endregion


        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    DisconnectAsync(true).GetAwaiter().GetResult();
                    _disconnectTokenSource?.Dispose();
                    _cancelTokenSource?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion


        #region Util
        private async Task<SocketFrame> DecodeSocketMessage(byte[]? result, int resultCount, WebSocketMessageType messageType)
        {
            var message = messageType == WebSocketMessageType.Text ?
            JsonSerializer.Deserialize<SocketFrame>(Encoding.UTF8.GetString(result!, 0, resultCount)) :
            JsonSerializer.Deserialize<SocketFrame>(await DecodeBinMessage(result!, 0, resultCount));
            return message!;
        }
        private async Task<string> DecodeBinMessage(byte[] data, int index, int count)
        {
            if (data == null)
                throw new InvalidOperationException("Data cant be null");
            if (_compressed == null || _decompressor == null)
                throw new InvalidOperationException("The decompression wasnt initialized properly");
            using var decompressed = new MemoryStream();
            if (data[0] == 0x78)
            {
                _compressed.Write(data, index + 2, count - 2);
                _compressed.SetLength(count - 2);
            }
            else
            {
                _compressed.Write(data, index, count);
                _compressed.SetLength(count);
            }

            //Reset positions so we don't run out of memory
            _compressed.Position = 0;
            _decompressor.CopyTo(decompressed);
            _compressed.Position = 0;
            decompressed.Position = 0;

            using var sr = new StreamReader(decompressed);
            return await sr.ReadToEndAsync();
        }

        #endregion
    }
}