using ArCana.Network.Messages;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Utf8Json;

namespace ArCana.Network
{
    public class Surface : IDisposable
    {
        private TcpListener _listener;
        public CancellationTokenSource TokenSource { get; set; }
        public CancellationToken Token { get; }
        private Task _listenTask;

        public event Func<Message, Task> MessageReceived;

        public Surface(CancellationTokenSource cts)
        {
            TokenSource = cts;
            Token = cts.Token;
        }

        public void Start(int port)
        {
            var address = IPAddress.Parse("0.0.0.0");
            var endPoint = new IPEndPoint(address, port);
            _listener = new TcpListener(endPoint);
            _listener.Start();
            _listenTask = ConnectionWaitAsync();
        }

        async Task ConnectionWaitAsync()
        {
            if (_listener is null) return;
            var tcs = new TaskCompletionSource<int>();
            await using (Token.Register(tcs.SetCanceled))
            {
                while (!Token.IsCancellationRequested)
                {
                    var t = _listener.AcceptTcpClientAsync();
                    if ((await Task.WhenAny(t, tcs.Task)).IsCanceled) break;
                    try
                    {
                        using var client = t.Result;
                        var message = await JsonSerializer.DeserializeAsync<Message>(client.GetStream());
                        var endPoint = client.Client.RemoteEndPoint;
                        await (MessageReceived?.Invoke(message) ?? Task.CompletedTask);
                    }
                    catch (SocketException e)
                    {
                    }
                }
            }
            _listener.Stop();
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
