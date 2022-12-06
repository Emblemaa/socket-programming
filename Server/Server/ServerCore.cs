using Server.Managers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class ServerCore
    {
        public static List<Player> players = new List<Player>();

        public static void Main(string[] args)
        {
            string defaultAddress = "127.0.0.1";
            if (args.Length > 0) defaultAddress = args[0];
            IPAddress address = IPAddress.Parse(defaultAddress);
            TcpListener tcpListener = new TcpListener(address, 8080);
            GameManager manager = GameManager.instance;
            //UDP
            UdpClient udpClient = new UdpClient(8081);
            
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 8081);
            Thread listeningThreadUDP = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    byte[] receiveBytes = udpClient.Receive(ref ipep);
                    string returnData = Encoding.ASCII.GetString(receiveBytes);
                    var matchedPlayer = players.Find((player) => { return player.match(ipep); });
                    matchedPlayer.ReceivePackage(returnData);
                }
            }));
            listeningThreadUDP.Start();
            
            tcpListener.Start();
            Console.WriteLine("Server running on " + defaultAddress);
            while (true)
            { 
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Console.WriteLine("New player connected");
                players.Add(new Player(tcpClient, udpClient));
            }
        }
    }
}
