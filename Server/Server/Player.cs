using Server.Function;
using Server.Room;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;



namespace Server
{
    public enum ConnectionType { TCP, UDP };

    class Player
    {
        private TcpClient tcp;
        private UdpClient udp;

        public IPEndPoint ipep;
        public string[] gameInfo;

        private StreamWriter writer;

        private Thread readThread;
        private bool isOpen = true;
        private string ip;
        private int port;
        public string name;
        public int avatar;

        public Player(TcpClient tcpClient, UdpClient udpClient)
        {
            this.tcp = tcpClient;
            this.udp = udpClient;
            this.ipep = (IPEndPoint) tcpClient.Client.RemoteEndPoint;
            var tmp = tcp.Client.RemoteEndPoint.ToString().Split(':');
            this.ip = tmp[0];
            this.port = int.Parse(tmp[1]);

            this.writer = new StreamWriter(tcp.GetStream());
            writer.AutoFlush = true;

            SendPackage("UDP", new List<string>() { port.ToString(), ipep.ToString() }, ConnectionType.TCP);

            this.readThread = new Thread(new ThreadStart(() => {
                NetworkStream stream = tcp.GetStream();
                int i;
                byte[] bytes = new byte[1024];
                string data = null;
                try
                {
                    while (isOpen && (i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);
                        ReceivePackage(data);
                    }
                    tcp.Close();
                    RoomManager.instance.RemovePlayer(ipep);
                    ServerCore.players.Remove(this);
                    Console.WriteLine("Player disconneted");
                }

                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        tcp.Close();
                        
                    }
            }));
            readThread.Start();
            
        }

        public bool match(IPEndPoint ipep)
        {
            return ipep.Address.ToString() == ip && ipep.Port == port;
        }

        public void ReceivePackage(string package)
        {
            var splits = package.Split("#");
            string type = splits[0];
            string[] param = splits.Length > 1 ? splits[1].Split("|") : null;
            FunctionManager.instance.MatchFunction(this, type, param);
        }

        public void SendPackage(string type, List<string> param, ConnectionType connectionType)
        {
            var package = type + "#";
            if(param != null)
            {
                foreach (string i in param)
                    package += (i + '|');
            }
            package = package.Substring(0, package.Length - 1);
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(package);
            switch (connectionType)
            {
                case ConnectionType.TCP:
                    writer.WriteLine(package);
                    break;
                case ConnectionType.UDP:
                    udp.Send(bytes, bytes.Length, ip, port);
                    break;
            }
        } 

        public void Close()
        {
            isOpen = false;
            readThread.Abort();
            tcp.Close();
        }
    }
}
