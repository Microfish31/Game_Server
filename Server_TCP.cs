using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace Server_TCP
{
    public class ServerTCP
    {
        public Socket server;
        public Socket client;

        public int latency;

        public byte[] get_byte_data;

        public byte[] get_byte_header;

        public byte[] get_byte_innner;

        public byte[] send_byte_header;

        private List<byte[]> sendd;

        private int send_ms;

        public ServerTCP(int data_size)
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            latency = -1;

            get_byte_data = new byte[data_size];

            get_byte_header = new byte[3];

            get_byte_innner = new byte[data_size-3];

            send_byte_header = new byte[3];

            sendd = new List<byte[]>();

            send_ms = -1;
        }
        
        public int TcpINI(String ip_address, int port)
        {
            try
            {
                IPAddress ip = IPAddress.Parse(ip_address);
                EndPoint ep = new IPEndPoint(ip, port);

                server.Bind(ep);
                server.Listen(10);
                server.ReceiveBufferSize = 65536;
                server.SendBufferSize = 65536;

                Console.WriteLine("伺服端...運行中...");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sever Set wrong !!");
                Console.WriteLine("可能原因 : Ipv4 位址錯誤。 ( 請於執行檔所在資料夾位置中 connection_data.txt 更改位址)");
                Console.WriteLine(ex);
                return 0;
            }
        }

        public void TCPReceiveStart(ThreadStart Func)
        {
            Thread thread_recieve = new Thread(Func);
            thread_recieve.IsBackground = true;
            thread_recieve.Start();
        }

        public void TCPReceiveStart(ParameterizedThreadStart Func,Object socket)
        {
            Thread thread_recieve = new Thread(Func);
            thread_recieve.IsBackground = true;
            thread_recieve.Start(socket);
        }

        public int Server_Get()   
        {
            try
            {
                Array.Clear(get_byte_data, 0, get_byte_data.Length);
                ReceiveSize(2);
                UInt16 DataLen = FindDataLen(get_byte_data);

                if (DataLen != 0)
                {
                    Array.Clear(get_byte_data, 0, get_byte_data.Length);
                    ReceiveSize(DataLen);

                    Buffer.BlockCopy(get_byte_data, 0, get_byte_header, 0, 3);
                    latency = int.Parse(DateTime.Now.ToString("ffff")) - ((int)get_byte_header[2] * 100 + (int)get_byte_header[1]);

                    get_byte_innner = new byte[DataLen - 3];
                    Buffer.BlockCopy(get_byte_data, 3, get_byte_innner, 0, DataLen - 3);
                    return (int)get_byte_header[0] + 1;
                }
                else {
                    Console.WriteLine("客戶端傳送資料為空!!");
                    return 0;
                }
            }
            catch {
                return 0;
            }
        }

        public void ReceiveSize(int len) {
            int get_len;
            int count = 0;

            while (len > 0)
            {
               byte[] a = new byte[len];
               get_len = client.Receive(a, len, 0);
               Buffer.BlockCopy(a, 0, get_byte_data, count, a.Length);
               len = len - get_len;
               count = count + get_len;
            }
        }

        public UInt16 FindDataLen(byte [] getData) {
            return BitConverter.ToUInt16(getData, 0);
        }

        // send to connected client (string)
        public int Server_Send(String send_msg) 
        {
            try
            {
                sendd.Add(Encoding.UTF8.GetBytes(send_msg));

                UInt16 dataLen = (UInt16)(3 + sendd[0].Length);

                sendd.Add(BitConverter.GetBytes(dataLen));

                send_byte_header[0] = 1;
                send_ms = int.Parse(DateTime.Now.ToString("ffff"));
                send_byte_header[1] = (byte) (send_ms % 100);
                send_byte_header[2] = (byte) (send_ms/100);
                sendd.Add(send_byte_header);

                byte[] ss = new byte[2+dataLen];
                Buffer.BlockCopy(sendd[1], 0, ss, 0, 2);
                Buffer.BlockCopy(sendd[2], 0, ss, 2, 3);
                Buffer.BlockCopy(sendd[0], 0, ss, 5, dataLen-3);
                client.Send(ss);
                sendd.Clear();

                return 1;
            }
            catch 
            {
                return 0;
            }
        }

        // send to target client (string)
        public int Server_Send(byte work_num,Socket target_client, String send_msg) 
        {
            try
            {
                sendd.Add(Encoding.UTF8.GetBytes(send_msg));

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
                target_client.Send(ss);
                sendd.Clear();
                return 1;
            }
            catch
            { 
                return 0;
            }
        }

        // send to  all clients (string)
        public int Server_Send(byte work_num,Socket[] target_clients, String send_msg,int length) 
        {
            try
            {
                if (target_clients.Length != 0)
                {
                    sendd.Add(Encoding.UTF8.GetBytes(send_msg));

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

                    for (int i = 0; i < length; i++)
                    {
                        target_clients[i].Send(ss);
                    }
                    sendd.Clear();
                    return 1;
                }
                else {
                    Console.WriteLine("There is not any client connecting now.");
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }


        // send to all clients (byte[])
        public int Server_Send(byte work_num,Socket[] target_clients, byte[] send_msg, int length)
        {
            try
            {
                if (target_clients.Length != 0)
                {
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

                    for (int i = 0; i < length; i++)
                    {
                        target_clients[i].Send(ss);
                    }
                    sendd.Clear();
                    return 1;
                }
                else
                {
                    Console.WriteLine("There is not any client connecting now.");
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}
