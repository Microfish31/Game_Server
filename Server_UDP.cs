using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace Server_UDP
{
    public class ServerUDP
    {
        public Socket UdpServer;
        public EndPoint ep;
        public int local_port;
        public int get_latency;

        private byte[] get_byte_data;
        private byte[] get_byte_header;
        public byte[] get_byte_innner;
        private byte[] send_byte_header;
        private List<byte[]> sendd;

        private int send_ms;
        private string Server_Ip;
        private EndPoint senderRemote;

        public ServerUDP(int transdata_size)
        {
            get_latency = -3;

            get_byte_data = new byte[transdata_size];
            get_byte_header = new byte[3];

            send_byte_header = new byte[3];
            sendd = new List<byte[]>();

            send_ms = -1;
            local_port = -1;
            Server_Ip = "";

            //UdpServer.ReceiveBufferSize = transdata_size;
            //UdpServer.SendBufferSize = transdata_size;
        }


        public void UdpInI(string server_ip, int server_port)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(server_ip), server_port);
            UdpServer = new Socket(endPoint.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            UdpServer.Bind(endPoint);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            senderRemote = (EndPoint)sender;
            Server_Ip = server_ip;

            GetLocalPort();
        }

        public void GetLocalPort()
        {
            local_port = int.Parse((UdpServer.LocalEndPoint as IPEndPoint).Port.ToString());
        }

        public void UdpReceiveStart(ThreadStart Func)
        {
            Thread thread_recieve = new Thread(Func);
            thread_recieve.IsBackground = true;
            thread_recieve.Start();
        }

        public int Server_Get()
        {
            try
            {
                Array.Clear(get_byte_data, 0, get_byte_data.Length);
                int DataLen = UdpServer.ReceiveFrom(get_byte_data, ref senderRemote);

                if (DataLen != 0)
                {
                    Buffer.BlockCopy(get_byte_data, 2, get_byte_header, 0, 3);

                    get_byte_innner = new byte[DataLen - 5];
                    Buffer.BlockCopy(get_byte_data, 5, get_byte_innner, 0, DataLen - 5);
                    return (int)get_byte_header[0] + 1;
                }
                else
                {
                    Console.WriteLine("客戶端傳送資料為空!!");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 0;
            }
        }

        public void PortDirection(int client_port)
        {
            //傳送資訊準備工作
            ep = new IPEndPoint(IPAddress.Parse(Server_Ip), client_port);
        }

        public UInt16 SendToClient(byte work_num, byte[] send_msg, int client_port)
        {
            try
            {
                PortDirection(client_port);

                sendd.Add(send_msg);

                UInt16 dataLen = (UInt16)(3 + sendd[0].Length);

                sendd.Add(BitConverter.GetBytes(dataLen));

                send_byte_header[0] = work_num;
                send_ms = int.Parse(DateTime.Now.ToString("ffff"));
                send_byte_header[1] = (byte)(send_ms % 100);
                send_byte_header[2] = (byte)(send_ms / 100);
                sendd.Add(send_byte_header);

                byte[] ss = new byte[2 + dataLen];
                Buffer.BlockCopy(sendd[1], 0, ss, 0, 2);
                Buffer.BlockCopy(sendd[2], 0, ss, 2, 3);
                Buffer.BlockCopy(sendd[0], 0, ss, 5, dataLen - 3);
                UdpServer.SendTo(ss, ep);
                sendd.Clear();
                return 1;
            }
            catch
            {
                return 0;
            }
        }
    }
}
