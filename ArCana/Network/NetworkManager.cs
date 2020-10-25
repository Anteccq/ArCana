using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArCana.Blockchain;
using ArCana.Network.Messages;

namespace ArCana.Network
{
    public class NetworkManager
    {
        private Server _server;
        public List<IPEndPoint> ConnectServers { get; } = new List<IPEndPoint>();
        public List<IPEndPoint> ConnectSurfaces { get; } = new List<IPEndPoint>();
        private Blockchain.Blockchain _blockchain = Blockchain.Blockchain.Instance;
        private Miner _miner = Miner.Instance;

        public NetworkManager(CancellationToken token)
        {
            var tokenSource = new CancellationTokenSource();
            _server = new Server(tokenSource);

            token.Register(Dispose);
        }

        public async Task StartServerAsync(int port) => await (_server?.StartAsync(port) ?? Task.CompletedTask);

        async Task MessageHandle(IPEndPoint endPoint, Message msg)
        {

        }

        async Task HandShakeHandle(IPEndPoint endPoint, HandShake msg)
        {
            var serverEndPoint = new IPEndPoint(endPoint.Address, msg.Port);
            if(ConnectServers.Contains(serverEndPoint)) return;
            //相手の endpoint を除外して送信する。
            await new AddrPayload()
                {
                    KnownIpEndPoints = ConnectServers.Where(x => !x.Equals(serverEndPoint)).Select(x => x.ToString()).ToList()
                }
                .ToMessage().SendAsync(serverEndPoint);
            
            var ipEndPoints = msg.KnowIpEndPoints.Select(CreateIPEndPoint).Except(ConnectServers);
            lock (ConnectServers)
            {
                if (!ConnectServers.Contains(serverEndPoint)) ConnectServers.Add(serverEndPoint);
                ConnectServers.AddRange(ipEndPoints);
            }
        }

        async Task AddrHandle(AddrPayload msg)
        {
            var ipEndPoints = msg.KnownIpEndPoints.Select(CreateIPEndPoint).Except(ConnectServers);
            lock (ConnectServers)
            {
                ConnectServers.AddRange(ipEndPoints);
            }
        }

        async Task BroadCastIPEndPoints(IReadOnlyCollection<IPEndPoint> endPointList)
        {
            var msg = new AddrPayload() { };
            var dcList = new List<IPEndPoint>();
            foreach (var ep in ConnectServers)
            {
                msg.KnownIpEndPoints = endPointList.Where(x => !x.Equals(ep)).Select(x => x.ToString()).ToList();
                try
                {
                    await msg.ToMessage().SendAsync(ep);
                }
                catch (SocketException)
                {
                    dcList.Add(ep);
                }
            }
            ConnectServers.RemoveAll(dcList.Contains);
        }

        public static IPEndPoint CreateIPEndPoint(string endPoint)
        {
            var ep = endPoint.Split(':');
            if(ep.Length<2) throw new FormatException();
            //only ipv4
            if(IPAddress.TryParse(ep[0],out var addr)) throw new FormatException();
            if(int.TryParse(ep[1], out var port)) throw new FormatException();
            return new IPEndPoint(addr, port);
        }

        public void Dispose()
        {
            _server.Dispose();
            ConnectSurfaces?.Clear();
            ConnectServers?.Clear();
        }
    }
}
