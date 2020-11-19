using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArCana.Network.Messages;

namespace ArCana.Network
{
    public class SurfaceManager
    {
        private IPEndPoint _serverEndPoint;
        private Surface _surface;
        private Timer _timer;
        private readonly CancellationToken _token;
        public int Port { get; set; }

        public SurfaceManager(CancellationToken token)
        {
            _token = token;
            var tokenSource = new CancellationTokenSource();
            _surface = new Surface(tokenSource);
            _surface.MessageReceived += MessageHandle;
            _token.Register(_surface.Dispose);
        }

        public void StartSurface(int port)
        {
            Port = port;
            _surface.Start(port);
        }

        async Task ConnectionCheckAsync()
        {
            try
            {
                await new Ping().ToMessage().SendAsync(_serverEndPoint, Port);
            }
            catch (SocketException)
            {
                _serverEndPoint = null;
            }
        }

        Task MessageHandle(Message msg)
        {
            return Task.CompletedTask;
        }
        public async Task ConnectServerAsync(IPEndPoint serverEndPoint)
        {
            try
            {
                await new SurfaceHandShake().ToMessage().SendAsync(serverEndPoint, Port);
                _serverEndPoint = serverEndPoint;
                _timer = new Timer(async _ => await ConnectionCheckAsync(), null,
                    TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
                _token.Register(_timer.Dispose);
            }
            catch (SocketException)
            {
            }
        }
    }
}
