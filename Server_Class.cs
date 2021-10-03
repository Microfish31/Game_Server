using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Unity_Data;

namespace Server_Connecter
{
    public class ServerClass
    {
        //建立伺服端
        public Socket server;
        public Socket client;

        // msg type = string
        public string get_message;
        public string send_message;

        public int get_byte_length;

        // 用來接客戶端的資料 type  = byte
        public byte[] get_byte_data;

        public byte[] get_byte_header;

        public byte[] get_byte_innner;

        //用來送客戶端的資料 type  = byte
        public byte[] send_byte_data;

        public byte[] send_byte_header;

        public byte[] send_byte_innner;

        // 定義集合，存客戶端資料
        public Dictionary<string, Socket> clients_data;
        public Dictionary<string, UnityClientStruct> clients_data_struct;

        // 定義集合，存取物件資料
        public Dictionary<string, UnityObjectData> object_data;

        public ServerClass(int data_size)
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            get_message = null;
            send_message = null;

            get_byte_length = 0;

            get_byte_data = new byte[data_size];

            get_byte_header = new byte[3];

            get_byte_innner = new byte[data_size-3];

            send_byte_data = new byte[data_size];

            send_byte_header = new byte[3];

            send_byte_innner = new byte[data_size - 3];

            clients_data = new Dictionary<string, Socket> { };
            clients_data_struct = new Dictionary<string, UnityClientStruct> { };

            object_data = new Dictionary<string, UnityObjectData> { };
        }
        
        public int Server_Set(String ip_address, int port)
        {
            try
            {
                IPAddress ip = IPAddress.Parse(ip_address);
                EndPoint ep = new IPEndPoint(ip, port);

                server.Bind(ep);
                server.Listen(10);
                server.ReceiveBufferSize = 65536;
                server.SendBufferSize = 65536;

                // set timeout 10 sec
                // server.ReceiveTimeout = 10000;

                Console.WriteLine("Sever Set Ok !!");
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

        public int Server_Get()   
        {
            try
            {
                Array.Clear(get_byte_data, 0, get_byte_data.Length);

                // server or client timeout let wrong.    accept cummulated?
                get_byte_length = client.Receive(get_byte_data);

                if(get_byte_length < 1027) {
                    byte [] a = new byte[1027- get_byte_length];
                    client.Receive(a);
                    Buffer.BlockCopy(a, 0, get_byte_data, get_byte_length, 1027 - get_byte_length);
                }

                //Console.WriteLine("byte_len = " + get_byte_data.Length.ToString());

                if (get_byte_length != 0)
                {
                    // get header label
                    Buffer.BlockCopy(get_byte_data, 0, get_byte_header, 0, 3);

                    Array.Clear(get_byte_innner, 0, get_byte_innner.Length);
                    Buffer.BlockCopy(get_byte_data, 3, get_byte_innner, 0, get_byte_length - 3);
                    return (int)get_byte_header[0] + 1;
                }
                else {
                    Console.WriteLine("客戶端傳送資料為空!!");
                    return 0;
                }
            }
            catch (Exception ex){
                //Console.WriteLine(ex);
                //Console.WriteLine("get ex");
                return 0;
            }
        }

        // send to connected client (string)
        public int Server_Send(String send_msg) 
        {
            try
            {
                // 資訊處理 
                Array.Clear(send_byte_data, 0, send_byte_data.Length);

                // header
                send_byte_header[0] = 1;
                send_byte_header[1] = 0;
                send_byte_header[2] = 0;

                Buffer.BlockCopy(send_byte_header, 0, send_byte_data, 0, 3);

                send_message = send_msg;

                // inner data
                send_byte_innner = Encoding.UTF8.GetBytes(send_message);

                Buffer.BlockCopy(send_byte_innner, 0, send_byte_data, 3, send_byte_innner.Length);

                //傳送資訊 
                client.Send(send_byte_data);

                return 1;
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
                Console.WriteLine("send ex");
                return 0;
            }
        }

        // send to target client (string)
        public int Server_Send(byte work_num,Socket target_client, String send_msg) 
        {
            try
            {
                // 資訊處理 
                Array.Clear(send_byte_data, 0, send_byte_data.Length);

                // header
                send_byte_header[0] = work_num;
                send_byte_header[1] = 0;
                send_byte_header[2] = 0;

                Buffer.BlockCopy(send_byte_header, 0, send_byte_data, 0, 3);

                // inner data
                send_byte_innner = Encoding.UTF8.GetBytes(send_msg);

                Buffer.BlockCopy(send_byte_innner, 0, send_byte_data, 3, send_byte_innner.Length);

                // 資訊傳送
                target_client.Send(send_byte_data);

                return 1;
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
                Console.WriteLine("send ex");
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
                    // 資訊處理 
                    Array.Clear(send_byte_data, 0, send_byte_data.Length);
                    // header
                    send_byte_header[0] = work_num;
                    send_byte_header[1] = 0;
                    send_byte_header[2] = 0;

                    Buffer.BlockCopy(send_byte_header, 0, send_byte_data, 0, 3);

                    // inner data
                    send_byte_innner = Encoding.UTF8.GetBytes(send_msg);

                    Buffer.BlockCopy(send_byte_innner, 0, send_byte_data, 3, send_byte_innner.Length);

                    // 資訊傳送

                    for (int i = 0; i < length; i++)
                    {
                        target_clients[i].Send(send_byte_data);
                    }

                    return 1;
                }
                else {
                    Console.WriteLine("There is not any client connecting now.");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("send ex");
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
                    // 資訊處理 
                    Array.Clear(send_byte_data, 0, send_byte_data.Length);

                    // header
                    send_byte_header[0] = work_num;
                    send_byte_header[1] = 0;
                    send_byte_header[2] = 0;

                    Buffer.BlockCopy(send_byte_header, 0, send_byte_data, 0, 3);

                    // inner data
                    send_byte_innner = send_msg;

                    Buffer.BlockCopy(send_byte_innner, 0, send_byte_data, 3, send_byte_innner.Length);

                    // 資訊傳送

                    for (int i = 0; i < length; i++)
                    {
                        target_clients[i].Send(send_byte_data);
                    }

                    return 1;
                }
                else
                {
                    Console.WriteLine("There is not any client connecting now.");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("send ex");
                return 0;
            }
        }
    }
}
