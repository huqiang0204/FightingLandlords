using System;
using System.Net;
using UnityEngine;
using System.Text;
using huqiang;
using huqiang.Data;

namespace huqiang.Data
{
    public class TcpDataControll
    {
        public static string JsonFix(string json)
        {
            var arry = json.ToCharArray();
            arry[0] = '{';
            arry[1] = '\"';
            return new string(arry);
        }
        /// <summary>
        /// 从某一个下标开始，字符s
        /// </summary>
        /// <param name="buff">给定的字符串</param>
        /// <param name="s">起止字符</param>
        /// <param name="e">终止字符</param>
        /// <param name="index">起始下表</param>
        /// <returns></returns>
        static int GetLabelEnd(string buff, char s, char e, int index = 0)
        {
            int a = 0;
            int b = 0;
            //Debug.Log(index);
            for (int i = index; i < buff.Length; i++)
            {
                if (buff[i] == s)
                    a++;
                else if (buff[i] == e)
                {
                    b++;
                    if (b == a)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        static string SerializeCustom(req r, string args)
        {
            var str = JsonUtility.ToJson(r);
            var temp = str.ToCharArray();
            temp[temp.Length - 1] = ',';
            str = new string(temp) + "\"args\":{" + args + "}}";//phone:123456789,code:123456
            return str;
        }
        static string Serialize(req r, object args)
        {
            var str = JsonUtility.ToJson(r);
            var temp = str.ToCharArray();

            temp[temp.Length - 1] = ',';
            str = new string(temp) + "\"args\":" + JsonUtility.ToJson(args) + "}";

            return str;
        }
        static string Serialize(req r, string args)
        {
            var str = JsonUtility.ToJson(r);
            var temp = str.ToCharArray();
            temp[temp.Length - 1] = ',';
            str = new string(temp) + "\"args\":" + args + "}";
            return str;
        }
        static TcpDataControll ins;
        public Action<TcpDataControll> legalResult;
        public static TcpDataControll Instance { get { if (ins == null) ins = new TcpDataControll(); return ins; } }
        TcpSocket socket;
        public bool IsConnected()
        {
            try
            {
                socket.SendMessage(new byte[2], EnvelopeType.Mate);
                return true;
            }catch(Exception ex)
            {
                return false;
            }
        }
        public void Connection(string ip,int port)
        {
            var address = IPAddress.Parse(ip);
            socket = new TcpSocket(262144,PackType.Part);
            socket.Connected = () =>
            {
            };
            socket.ConnectServer(address, port);

            socket.SetDispatchMethod(DispatchEx, false, 32);
            socket.ConnectFaild = (o) => {
                Debug.Log("连接失败");
            };
        }
    
        public int pin;
        public int userId;
        public void DispatchMessage()
        {
            if(socket!=null)
            {
                socket.Dispatch();
                Time += UserAction.TimeSlice;
                if (Time > 1000)
                {
                    Time -= 1000;
                    socket.SendMessage(new byte[2], EnvelopeType.Mate);
                }
            }
        }
        float Time;
        void DispatchEx(byte[] data, UInt32 tag, object obj)
        {
            //tag >>= 24;
            byte type = (byte)tag;
            switch (type)
            {
                case EnvelopeType.Mate:
                    DispatchMetaData(data);
                    break;
                case EnvelopeType.AesJson:
                    byte[] dec = AES.Instance.Decrypt(data, 0, data.Length);
                    var json = Encoding.UTF8.GetString(dec);
                    DispatchJson(json);
                    break;
                case EnvelopeType.Json:
                    json = Encoding.UTF8.GetString(data);
                    DispatchJson(json);
                    break;
                case EnvelopeType.AesDataBuffer:
                    dec = AES.Instance.Decrypt(data, 0, data.Length);
                    DataBuffer buff = new DataBuffer(dec);
                    DispatchStream(buff);
                    break;
                case EnvelopeType.DataBuffer:
                    buff = new DataBuffer(data);
                    DispatchStream(buff);
                    break;
                case EnvelopeType.String:
                    json = Encoding.UTF8.GetString(data);
                    DispatchString(json);
                    break;
            }
        }
        void DispatchMetaData(byte[] data)
        {

        }
        void DispatchString(string json)
        {

        }
        void DispatchJson(string json)
        {
            var j = JsonUtility.FromJson<reqs>(json);
            if (j.args == null | j.args == "")
            {
                int index = json.IndexOf("args\":");
                if (index > -1)
                {
                    index += 5;
                    int end = GetLabelEnd(json, '{', '}', index);
                    index += 1;
                    int len = end - index + 1;
                    if (len > 0)
                    {
                        j.args = json.Substring(index, len);
                    }
                }
            }

        }
        void DispatchStream(DataBuffer buffer)
        {
            Page.CurrentPage.Cmd(buffer);
        }
        bool SendAesJson(byte[] dat )
        {
            return socket.SendMessage(dat, EnvelopeType.AesJson); 
        }
        public bool SendStream(DataBuffer db)
        {
            return socket.SendMessage(db.ToBytes(), EnvelopeType.DataBuffer);
        }
        public bool SendAesStream(DataBuffer db)
        {
            return socket.SendMessage(AES.Instance.Encrypt(db.ToBytes()),EnvelopeType.AesDataBuffer);
        }
        public void Close()
        {
            if(socket!=null)
            socket.Close();
        }
        public bool SendCustom(string cmd, string contnet, string type = "def")
        {
            req r = new req();
            r.type = type;
            r.cmd = cmd;
            r.pin = pin;
            r.userId = userId;
            //r.lan = Local.Instance.set.Language;
            string str = SerializeCustom(r, contnet);
            return SendAesJson(JavaAES.Instance.Encrypt(str));
        }
        public bool SendObject(string cmd, object p, string type = "def")
        {
            req r = new req();
            r.type = type;
            r.cmd = cmd;
            r.pin = pin;
            r.userId = userId;
            //r.lan = Local.Instance.set.Language;
            string str = Serialize(r, p);
            return SendAesJson(JavaAES.Instance.Encrypt(str));
        }
        public bool SendObject(string cmd, object p, string liten, string type = "def")
        {
            req r = new req();
            r.type = type;
            r.cmd = cmd;
            r.pin = pin;
            r.userId = userId;
            string str = Serialize(r, p);
            //r.lan = Local.Instance.set.Language;
            return SendAesJson(JavaAES.Instance.Encrypt(str));
        }
        public bool SendString(string cmd, string str, string type = "def")
        {
            reqs r = new reqs();
            r.type = type;
            r.cmd = cmd;
            r.pin = pin;
            r.userId = userId;
            r.args = str;
            //r.lan = Local.Instance.set.Language;
            str = JsonUtility.ToJson(r);
            return SendAesJson(JavaAES.Instance.Encrypt(str));
        }
        public bool SendCustomA(string cmd, string contnet, string type = "def")
        {
            req r = new req();
            r.type = type;
            r.cmd = cmd;
            r.pin = pin;
            r.userId = userId;
            //r.lan = Local.Instance.set.Language;
            string str = Serialize(r, contnet);
            return SendAesJson(JavaAES.Instance.Encrypt(str));
        }
        public bool SendDouble(string cmd, double v, string type = "def")
        {
            reqd r = new reqd();
            r.type = type;
            r.cmd = cmd;
            r.pin = pin;
            r.userId = userId;
            r.args = v;
            //r.lan = Local.Instance.set.Language;
            var str = JsonUtility.ToJson(r);
            return SendAesJson(JavaAES.Instance.Encrypt(str));
        }
        public bool SendBool(string cmd, bool b, string type = "def")
        {
            reqb r = new reqb();
            r.type = type;
            r.cmd = cmd;
            r.pin = pin;
            r.userId = userId;
            r.args = b;
            //r.lan = Local.Instance.set.Language;
            var str = JsonUtility.ToJson(r);
            return SendAesJson(JavaAES.Instance.Encrypt(str));
        }
        public bool SendNull(string cmd,string type="def")
        {
            req r = new req();
            r.type = type;
            r.cmd = cmd;
            r.pin = pin;
            r.userId = userId;
            //r.lan = Local.Instance.set.Language;
            string str = JsonUtility.ToJson(r);
            return SendAesJson(JavaAES.Instance.Encrypt(str));
        }
        public bool Send(req r, string cmd, string type = "def")
        {
            r.type = type;
            r.cmd = cmd;
            r.pin = pin;
            r.userId = userId;
            //r.lan = Local.Instance.set.Language;
            string str = JsonUtility.ToJson(r);
            return SendAesJson(JavaAES.Instance.Encrypt(str));
        }

        [Serializable]
        class reqi : req
        {
            public int args;
        }
        [Serializable]
        class reqd : req
        {
            public double args;
        }
        [Serializable]
        class reqb : req
        {
            public bool args;
        }
    }
    [Serializable]
    class rpc
    {
        public int pin;
        public string cmd;
        public string type;
        public string args;
    }
    [Serializable]
    class PlatFrom
    {
        public int platform;
    }
    public class DataCode
    {
        public const string Type_Rpc = "rpc";
        public const string Type_Game = "game";
        public const string Type_Empty = "def";
        public const string Type_Query = "query";
    }

}