using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

//Console.WriteLine(Encoding.UTF8.GetString(get_byte_data));
//Console.WriteLine((senderRemote as IPEndPoint).Port.ToString());
//SendMessageToClient(int.Parse((senderRemote as IPEndPoint).Port.ToString()), "收到");

namespace Server_UDP
{
    public class Msg
    {
        public string msg { get; set; }
        public int port { get; set; }
    }

    public class ServerUDP
    {
        public Socket UdpServer;
        public EndPoint ep;
        public int local_port;
        public int get_latency;

        private byte[] get_byte_data;
        private byte[] get_byte_header;
        public byte[] get_byte_innner;
        private byte[] send_byte_data;
        private byte[] send_byte_header;
        private byte[] send_byte_innner;

        private string Server_Ip;
        private int get_byte_length;
        private EndPoint senderRemote;

        public ServerUDP(int transdata_size)
        {
            get_latency = -3;

            get_byte_data = new byte[transdata_size];
            get_byte_header = new byte[3];
            get_byte_innner = new byte[transdata_size - 3];
            send_byte_data = new byte[transdata_size];
            send_byte_header = new byte[3];
            send_byte_innner = new byte[transdata_size - 3];

            get_byte_length = 0;

            local_port = -1;
            Server_Ip = "";
        }


        public void UdpInI(string server_ip, int server_port)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(server_ip), server_port);

            UdpServer = new Socket(endPoint.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            // Binding is required with ReceiveFrom calls.
            UdpServer.Bind(endPoint);

            // Creates an IPEndPoint to capture the identity of the sending host.
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            senderRemote = (EndPoint)sender;

            Server_Ip = server_ip;

            GetLocalPort();
        }

        public void GetLocalPort()
        {
            local_port = int.Parse((UdpServer.LocalEndPoint as IPEndPoint).Port.ToString());
        }

        public void PortDirection(int client_port) {
            //傳送資訊準備工作
            ep = new IPEndPoint(IPAddress.Parse(Server_Ip), client_port);
        }

        public void Server_Send(int client_port)
        {
            PortDirection(client_port);
            UdpServer.SendTo(send_byte_data, ep);
        }

        public void Server_Send(int client_port,string str)
        {
            PortDirection(client_port);
            send_byte_data = Encoding.UTF8.GetBytes(str);
            UdpServer.SendTo(send_byte_data, ep);
        }

        public void UdpReceiveStart(ThreadStart Func)
        {
            Thread thread_recieve = new Thread(Func);
            thread_recieve.IsBackground = true;
            thread_recieve.Start();
        }

        /*
        public void UdpReceiveStart(ThreadStart<> Func)
        {
            Thread thread_recieve = new Thread(Func);
            thread_recieve.IsBackground = true;
            thread_recieve.Start();
        }
        */

        public int Server_Get()
        {
            try
            {
                Array.Clear(get_byte_data, 0, get_byte_data.Length);
                UdpServer.ReceiveFrom(get_byte_data, ref senderRemote);

                get_byte_length = get_byte_data.Length;

                //Console.WriteLine("byte_len = {0}" , get_byte_length);

                if (get_byte_length < 1027)
                {
                    byte[] a = new byte[1027 - get_byte_length];
                    UdpServer.ReceiveFrom(a, ref senderRemote);
                    Buffer.BlockCopy(a, 0, get_byte_data, get_byte_length, 1027 - get_byte_length);
                }

                //Console.WriteLine("byte_len = " + get_byte_data.Length.ToString());

                if (get_byte_length != 0)
                {
                    // get header label
                    Buffer.BlockCopy(get_byte_data, 0, get_byte_header, 0, 3);

                    // latency 高出 bug
                    get_latency = int.Parse(DateTime.Now.ToString("ffff")) - ((int)get_byte_header[2] * 100 + (int)get_byte_header[1]);
                    //Console.WriteLine(get_latency);

                    Array.Clear(get_byte_innner, 0, get_byte_innner.Length);
                    Buffer.BlockCopy(get_byte_data, 3, get_byte_innner, 0, get_byte_length - 3);

                    return (int)get_byte_header[0] + 1;
                }
                else
                {
                    Console.WriteLine("UDP客戶端傳送資料為空!!");
                    return 0;
                }
            }
            catch //(Exception ex)
            {
                //Console.WriteLine(ex);
                Console.WriteLine("get ex");
                UdpServer.Close();
                return 0;
            }
        }
    }
}
