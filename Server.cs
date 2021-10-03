using Server_Connecter;
using Struct_Json;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Unity_Data;

namespace VR_Server
{
    class Program
    {
        // 建立伺服端 1027--> byte size
        static ServerClass mainserver = new ServerClass(1027);

        // UDP
        static Socket udp_server;
        static EndPoint senderRemote;
        static byte[] msg = new Byte[1027];

        static void Main(string[] args)
        {
            UnityObjectInitial();
            string now_path = System.Environment.CurrentDirectory;
            string server_ip = Read_Ip_Fromtxt(now_path);
            int server_port = 3000;
            StructJson js = new StructJson();

            if (mainserver.Server_Set(server_ip, server_port) == 1)
            {
                // Tcp
                Thread thread_watch = new Thread(Watch_Connecting);
                thread_watch.IsBackground = true;
                thread_watch.Start();

                // UDP
                UDP_Ini(server_ip, server_port);
                Thread Udp_Get = new Thread(UDP_Get_Msg);
                Udp_Get.IsBackground = true;
                Udp_Get.Start();

                Console.WriteLine("開啟監聽......");

                Socket[] socket_array;
                UnityClientStruct[] client_array;
                int client_count;
                object lock_clone = new object();

                while (true)
                {

                    // Send data or other works in Unity (Main Thread)
                    lock (lock_clone)
                    {
                        socket_array = mainserver.clients_data.Values.ToArray<Socket>();
                        client_array = mainserver.clients_data_struct.Values.ToArray<UnityClientStruct>();
                        client_count = socket_array.Length;
                    }
                    if (client_count != 0)
                        mainserver.Server_Send(0, socket_array, js.StructToBytes<UnityClientStruct>(client_array), client_count);
                    Thread.Sleep(50);
                }
            }
        }

        static string Read_Ip_Fromtxt(string path) 
        {
            string file_path = path + @"\connection_data.txt";

            if (File.Exists(file_path))
            {
                IPAddress a;

                using (StreamReader sr = File.OpenText(file_path))
                {
                    if (IPAddress.TryParse(sr.ReadLine(), out a))
                    {
                        Console.WriteLine("依據txt file 所載位址 " + a.ToString());
                        return a.ToString();
                    }
                    else
                    {
                        Console.WriteLine("不合法的ip，將重新偵測ipv4");
                    }
                }

                string get_ipv4 = Ipv4_Autodetect();

                using (StreamWriter sw = File.CreateText(file_path))
                {
                    sw.WriteLine(get_ipv4);
                    return get_ipv4;
                }
            }
            else {
                string get_ipv4 = Ipv4_Autodetect();

                using (StreamWriter sw = File.CreateText(file_path))
                {
                    sw.WriteLine(get_ipv4);
                }
                return get_ipv4;
            }
        }

        static string Ipv4_Autodetect()
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());

            // Regex
            string pattern = @"(([\d]{1,3}['.']){3})[\d]{1,3}";
            Regex rgx = new Regex(pattern);

            ArrayList ipv4_addr = new ArrayList();
            int count = 0;

            foreach (IPAddress ipaddress in hostEntry.AddressList)
            {
                if (rgx.IsMatch(ipaddress.ToString()))
                {
                    count++;
                    ipv4_addr.Add(ipaddress);
                }
            }

            if (count == 1)
            {
                Console.WriteLine("偵測到位址 " + ipv4_addr[0].ToString());
                return ipv4_addr[0].ToString();
            }
            else if (count > 1)
            {
                Console.WriteLine("偵測到多個主機位址");
                for (int i = 0; i < ipv4_addr.Count; i++) 
                {
                    Console.WriteLine(string.Format("({0}) ",i) + ipv4_addr[i]);
                }
                Console.WriteLine("請打入數字選擇:");

                string choosed_ipv4 = ipv4_addr[int.Parse(Console.ReadLine())].ToString();
                Console.WriteLine("所選位址 " + choosed_ipv4);
                return choosed_ipv4;
            }
            else
            {
                // 執行不到??
                Console.WriteLine("No Web Connection!!");
                return null;
            }
        }

        static void Watch_Connecting()
        {
            int client_count;
            string remoteEndPoint;
            object locker = new Object();

            while (true)
            {
                try
                {
                    mainserver.client = mainserver.server.Accept();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }

                // change to Asynchronous + threadpool 
                Thread thread_connect = new Thread(Client_Receive);
                thread_connect.IsBackground = true;
                thread_connect.Start(mainserver.client);

                // 客戶端 ip:port
                remoteEndPoint = mainserver.client.RemoteEndPoint.ToString();

                // lock?
                client_count = mainserver.clients_data.Count + 1;

                // 涉及 thread 順序
                // send initial position then client send
                mainserver.Server_Send(3, mainserver.client, (client_count - 1).ToString());

                // if add start to send  but if role not establish may wrong  need to check
                mainserver.clients_data[remoteEndPoint] = mainserver.client;

                // print client the condition of the connection 
                Console.WriteLine("[ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ff") + " ] " + "( " + remoteEndPoint + " ) : Connection Success！ #" + client_count + "\n");
            }
        }

        static void Client_Receive(object client_para)
        {
            ServerClass connecter = new ServerClass(1027);
            connecter.client = client_para as Socket;
            connecter.client.ReceiveBufferSize = 65536;
            connecter.client.SendBufferSize = 65536;

            int work_num;
            string addr = connecter.client.RemoteEndPoint.ToString();
            string port = (connecter.client.RemoteEndPoint as IPEndPoint).Port.ToString();

            Socket[] socket_array;
            UnityClientStruct unity_client_struct = new UnityClientStruct();
            UnityObjectStruct unity_obj_struct = new UnityObjectStruct();
            UnityClientStruct[] client_array;
            StructJson js = new StructJson();
            object locker = new object();

            while (true)
            {
                work_num = connecter.Server_Get();

                if (work_num != 0)
                {
                    switch (work_num)
                    {
                        case 1:
                            //Console.WriteLine("工作代號 0: 請求更新玩家資料");
                            unity_client_struct = js.BytesToStruct<UnityClientStruct>(connecter.get_byte_innner);
                            
                            lock (locker) 
                            {
                                mainserver.clients_data_struct[addr] = unity_client_struct;
                            }
                            break;
                        case 2 :
                            // not yet
                            //Console.WriteLine("工作代號 1: 請求發送訊息給所有客戶端");
                            //GetString(Byte[], Int32, Int32)  /包含要解碼之位元組序列的位元組陣列。/要解碼的第一個位元組索引。/要解碼的位元組數。/
                            //connecter.get_message = Encoding.UTF8.GetString(connecter.get_byte_innner, 0, connecter.get_byte_innner.Length);
                            //Console.WriteLine("[ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ff") + " ] \n>> " + connecter.get_message + "\n");
                            break;
                        case 3:
                            //Console.WriteLine("工作代號 2: 請求生成角色");  客戶端已經初始化
                            unity_client_struct = js.BytesToStruct<UnityClientStruct>(connecter.get_byte_innner);

                            lock (locker)
                            {
                                mainserver.clients_data_struct[addr] = unity_client_struct;
                                socket_array = mainserver.clients_data.Values.ToArray<Socket>();
                            }
                            connecter.Server_Send(4, socket_array, js.StructToBytes<UnityClientStruct>(unity_client_struct), socket_array.Length);
                            break;
                        case 4:
                            //Console.WriteLine("工作代號 3: 請求加載其他客戶端資料");
                            lock (locker)
                            {
                                socket_array = mainserver.clients_data.Values.ToArray<Socket>();
                                client_array = mainserver.clients_data_struct.Values.ToArray<UnityClientStruct>();
                            }
                            connecter.Server_Send(5,socket_array, js.StructToBytes<UnityClientStruct>(client_array), socket_array.Length);
                            break;
                        case 5:
                            //Console.WriteLine("工作代號 4: 請求更新其他客戶端物件資料");
                            unity_obj_struct = js.BytesToStruct<UnityObjectStruct>(connecter.get_byte_innner);
                            mainserver.object_data[unity_obj_struct.Objname].Data = unity_obj_struct;

                            lock (locker)
                            {
                                socket_array = mainserver.clients_data.Values.ToArray<Socket>();
                            }
                            connecter.Server_Send(6,socket_array, connecter.get_byte_innner, socket_array.Length);
                            break;
                    }
                }    
                else
                {
                    // close the error client
                    connecter.client.Close();

                    // remove RemoteEndPoint
                    mainserver.clients_data.Remove(addr);
                    mainserver.clients_data_struct.Remove(addr);

                    // print connect error
                    Console.WriteLine("[ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ff") + " ] ( " + addr + " ) : Has been disconnected!！ #" + mainserver.clients_data.Count + "\n");

                    lock (locker)
                    {
                        socket_array = mainserver.clients_data.Values.ToArray<Socket>();
                    }

                    // send to client to remove data
                    mainserver.Server_Send(2, socket_array, port, socket_array.Length);
                    
                    break;
                }
            }
        }

        static void UDP_Ini(string server_ip, int server_port)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(server_ip), server_port);

            udp_server = new Socket(endPoint.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            // Binding is required with ReceiveFrom calls.
            udp_server.Bind(endPoint);

            // Creates an IPEndPoint to capture the identity of the sending host.
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            senderRemote = (EndPoint)sender;
        }

        static void UDP_Get_Msg()
        {
            while (true)
            {
                Array.Clear(msg, 0, msg.Length);

                try
                {
                    udp_server.ReceiveFrom(msg, ref senderRemote);
                    Console.WriteLine(Encoding.UTF8.GetString(msg));
                    Console.WriteLine((senderRemote as IPEndPoint).Port.ToString());
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(ex);
                    break;
                }

            }
        }

        static void UnityObjectInitial()
        {
            UnityObjectData obj = new UnityObjectData();

            obj.Data_ini("Cover", null, -1);
            mainserver.object_data[obj.Data.Objname] = obj;

            obj.Data_ini("Shaft", null, -1);
            mainserver.object_data[obj.Data.Objname] = obj;
        }
    }
}
