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
using static Utf8Json.JsonSerializer;

namespace ArCana.Network
{
    public class NetworkManager
    {
        private Server _server;
        public List<IPEndPoint> ConnectServers { get; } = new List<IPEndPoint>();
        public List<IPEndPoint> ConnectSurfaces { get; } = new List<IPEndPoint>();
        public BlockchainManager BlockchainManager { get; set; }
        public int Port { get; private set; }

        public NetworkManager(CancellationToken token) : this(token, new BlockchainManager())
        {
            
        }

        public NetworkManager(CancellationToken token, BlockchainManager bm)
        {
            var tokenSource = new CancellationTokenSource();
            _server = new Server(tokenSource);
            BlockchainManager = bm;
            token.Register(Dispose);
        }

        public async Task StartServerAsync(int port)
        {
            Port = port;
            await(_server?.StartAsync(port) ?? Task.CompletedTask);
        }

        public async Task ConnectAsync(IPEndPoint endPoint)
        {
            if(!_server.IsOpen) throw new SocketException();
            await new HandShake()
                {
                    KnowIpEndPoints = new List<string>()
                }
                .ToMessage().SendAsync(endPoint, Port);
        }

        Task MessageHandle(IPEndPoint endPoint, Message msg)
        {
            return msg.Type switch
            {
                MessageType.HandShake => HandShakeHandle(endPoint,Deserialize<HandShake>(msg.Payload)),
                MessageType.Addr => AddrHandle(Deserialize<AddrPayload>(msg.Payload)),
                MessageType.Inventory => Task.CompletedTask,
                MessageType.NewTransaction => BlockchainManager.NewTransactionHandle(Deserialize<NewTransaction>(msg.Payload)),
                MessageType.NewBlock => BlockchainManager.NewBlockHandle(Deserialize<NewBlock>(msg.Payload), endPoint, Port),
                MessageType.FullChain => BlockchainManager.ReceiveFullChain(msg, endPoint),
                MessageType.RequestFullChain => BlockchainManager.SendFullChain(endPoint, Port),
                MessageType.Notice => Task.CompletedTask,
                MessageType.Ping => Task.CompletedTask,
                MessageType.SurfaceHandShake => Task.CompletedTask,
                _ => Task.CompletedTask
            };
        }

        async Task HandShakeHandle(IPEndPoint endPoint, HandShake msg)
        {
            if(ConnectServers.Contains(endPoint)) return;
            //相手の endpoint を除外して送信する。
            await new AddrPayload()
                {
                    KnownIpEndPoints = ConnectServers.Where(x => !x.Equals(endPoint)).Select(x => x.ToString()).ToList()
                }
                .ToMessage().SendAsync(endPoint, Port);
            
            var ipEndPoints = msg.KnowIpEndPoints.Select(CreateIPEndPoint).Except(ConnectServers);
            lock (ConnectServers)
            {
                if (!ConnectServers.Contains(endPoint)) ConnectServers.Add(endPoint);
                ConnectServers.AddRange(ipEndPoints);
            }
        }

        async Task AddrHandle(AddrPayload msg)
        {
            var ipEndPoints = 
                msg.KnownIpEndPoints
                    .Select(CreateIPEndPoint)
                    .Except(ConnectServers)
                    .ToList();
            await BroadCastIPEndPoints(ipEndPoints);
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
                    await msg.ToMessage().SendAsync(ep, Port);
                }
                catch (SocketException)
                {
                    dcList.Add(ep);
                }
            }
            lock (ConnectServers)
            {
                ConnectServers.RemoveAll(dcList.Contains);
            }
        }

        async Task BroadCastMessageAsync(Message msg)
        {
            var dcList = new List<IPEndPoint>();
            foreach (var ep in ConnectServers)
            {
                try
                {
                    await msg.SendAsync(ep, Port);
                }
                catch (SocketException)
                {
                    dcList.Add(ep);
                }
            }

            lock (ConnectServers)
            {
                ConnectServers.RemoveAll(dcList.Contains);
            }
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
