using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections;

namespace IP_Detect
{
    public class IpDetect
    {
        public string Read_Ip_Fromtxt(string path)
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
            else
            {
                string get_ipv4 = Ipv4_Autodetect();

                using (StreamWriter sw = File.CreateText(file_path))
                {
                    sw.WriteLine(get_ipv4);
                }
                return get_ipv4;
            }
        }

        private string Ipv4_Autodetect()
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
                    Console.WriteLine(string.Format("({0}) ", i) + ipv4_addr[i]);
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
    }
}
