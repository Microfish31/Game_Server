namespace Unity_Data
{
    public class Position
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public void Position_Set(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }

    public class Rotation
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public void Rotation_Set(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

    }

    public class UnityClientStruct
    {

        public string Objname { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }

        public Position Position { get; set; }

        public Rotation Rotation { get; set; }
        public string Color { get; set; }
    }

    public class UnityClientData
    {
        public UnityClientStruct Data = new UnityClientStruct();
        private Position pos = new Position();
        private Rotation rot = new Rotation();
        public string File_path = null;

        public void Data_ini(string objname,string ip,int port)
        {
            pos.Position_Set(0, 0, 0);
            rot.Rotation_Set(0, 0, 0, 0);

            Data.Objname = objname;
            Data.Ip = ip;
            Data.Port = port;
            Data.Position = pos;
            Data.Rotation = rot;
            Data.Color = "#2828FF";
        }
    }

    public class UnityObjectStruct
    {
        public string Objname { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }

        public Position Position { get; set; }

        public Rotation Rotation { get; set; }
        public string Color { get; set; }

        public bool Enable { get; set; }
    }

    public class UnityObjectData
    {
        public UnityObjectStruct Data = new UnityObjectStruct();
        private Position pos = new Position();
        private Rotation rot = new Rotation();
        public string File_path = null;

        public void Data_ini(string objname, string ip, int port)
        {
            pos.Position_Set(0, 0, 0);
            rot.Rotation_Set(0, 0, 0, 0);

            Data.Objname = objname;
            Data.Ip = ip;
            Data.Port = port;
            Data.Position = pos;
            Data.Rotation = rot;
            Data.Color = "#2828FF";
            Data.Enable = false;
        }
    }

    public class Msg
    {
        public string msg { get; set; }
        public int port { get; set; }
    }
}
