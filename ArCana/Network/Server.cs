using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArCana.Network.Messages;
using Utf8Json;

namespace ArCana.Network
{
    public class Server : IDisposable
    {
        private TcpListener _listener;
        public CancellationTokenSource TokenSource { get; set; }
        public CancellationToken Token { get; }
        private Task _listenTask;

        public bool IsOpen { get; private set; } = false;

        public event Func<Message, IPEndPoint, Task> MessageReceived;

        public Server(CancellationTokenSource tokenSource)
        {
            TokenSource = tokenSource;
            Token = tokenSource.Token;
        }

        public async Task StartAsync(int port)
        {
            var address = IPAddress.Parse("0.0.0.0");
            var endPoint = new IPEndPoint(address, port);
            _listener = new TcpListener(endPoint);
            _listener.Start();
            //await (NewConnection?.Invoke(endPoint, MessageType.HandShake) ?? Task.CompletedTask);
            _listenTask = ConnectionWaitAsync();
        }

        async Task ConnectionWaitAsync()
        {
            if (_listener is null) return;
            var tcs = new TaskCompletionSource<int>();
            await using (Token.Register(tcs.SetCanceled))
            {
                IsOpen = true;
                while (!Token.IsCancellationRequested)
                {
                    var t = _listener.AcceptTcpClientAsync();
                    if ((await Task.WhenAny(t, tcs.Task)).IsCanceled) break;
                    try
                    {
                        using var client = t.Result;
                        if(!(client.Client.RemoteEndPoint is IPEndPoint endPoint)) return;
                        //if (endPoint.Address.ToString() == "127.0.0.1") continue;
                        var message = await JsonSerializer.DeserializeAsync<Message>(client.GetStream());
                        var serverEndPoint = new IPEndPoint(endPoint.Address, message.Port);
                        await (MessageReceived?.Invoke(message, serverEndPoint) ?? Task.CompletedTask);
                    }
                    catch (SocketException)
                    {
                    }
                }
            }
            _listener.Stop();
            IsOpen = false;
        }

        public void Dispose()
        {
            if (TokenSource is null) return;
            TokenSource.Cancel();
            TokenSource.Dispose();
            TokenSource = null;
        }
    }
}
