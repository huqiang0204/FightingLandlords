using System;
using System.Net;
using UnityEngine;
using System.Text;
using huqiang;
using huqiang.Data;
using System.Collections.Generic;

namespace huqiang.Data
{
    public class KcpDataControll
    {
        class KcpData
        {
            public byte[] dat;
            public byte tag;
        }
        class KcpSocket : KcpLink
        {
            public Queue<KcpData> datas;
            public KcpSocket(KcpServer server) : base(server)
            {
                datas = new Queue<KcpData>();
            }
            public override void Dispatch(byte[] dat, byte tag)
            {
                KcpData data = new KcpData();
                data.dat = dat;
                data.tag = tag;
                lock (datas)
                    datas.Enqueue(data);
            }
            public override void Disconnect()
            {
               
            }
        }
        static KcpDataControll ins;
        public static KcpDataControll Instance { get { if (ins == null) ins = new KcpDataControll(); return ins; } }
        string UniId;
        KcpSocket link;
        public void Connection(string ip,int port)
        {
            UniId = SystemInfo.deviceUniqueIdentifier;
            var address = IPAddress.Parse(ip);
            KcpServer.CreateLink = (o) => { return new KcpSocket(o); };
            var kcp = new KcpServer(0,0,1);
            link = kcp.CreateNewLink(new IPEndPoint(address,9998)) as KcpSocket;
        }
        public void Login()
        {
            DataBuffer db = new DataBuffer();
            var fake = new FakeStruct(db, Req.Length);
            fake[Req.Cmd] = 0;
            fake[Req.Type] = MessageType.Rpc;
            fake.SetData(Req.Args, UniId);
            db.fakeStruct = fake;

            SendAesStream(db);
        }
        public int pin;
        public int userId;
        public void FailedConnect()
        {
           
        }
        public void Close()
        {
            KcpServer.Instance.Dispose();
        }
        public void DispatchMessage()
        {
            try
            {
                if (link != null)
                {
                    lock (link.datas)
                    {
                        int c = link.datas.Count;
                        for (int i = 0; i < c; i++)
                        {
                            var dat = link.datas.Dequeue();
                            DispatchEx(dat.dat, dat.tag);
                        }
                    }
                }
            }
            catch
            {
            }
        }
        float Time;
        void DispatchEx(byte[] data, byte tag)
        {
            byte type = tag;
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

        }
        void DispatchStream(DataBuffer buffer)
        {
            Page.CurrentPage.Cmd(buffer);
        }
        void SendAesJson(byte[] dat )
        {
            link.Send(dat, EnvelopeType.AesJson); 
        }
        public void SendStream(DataBuffer db)
        {
             link.Send(db.ToBytes(), EnvelopeType.DataBuffer);
        }
        public void SendAesStream(DataBuffer db)
        {
            link.Send(AES.Instance.Encrypt(db.ToBytes()),EnvelopeType.AesDataBuffer);
        }
        public void SendObject(string cmd, object p, string type = "def")
        {
            reqs r = new reqs();
            r.type = type;
            r.cmd = cmd;
            r.pin = pin;
            r.userId = userId;
            r.args = JsonUtility.ToJson(p);
            //r.lan = Local.Instance.set.Language;
            string str = JsonUtility.ToJson(r);
            SendAesJson(JavaAES.Instance.Encrypt(str));
        }
        public void SendString(string cmd, string str, string type = "def")
        {
            reqs r = new reqs();
            r.type = type;
            r.cmd = cmd;
            r.pin = pin;
            r.userId = userId;
            r.args = str;
            //r.lan = Local.Instance.set.Language;
            str = JsonUtility.ToJson(r);
            SendAesJson(JavaAES.Instance.Encrypt(str));
        }

        public void SendNull(string cmd,string type="def")
        {
            req r = new req();
            r.type = type;
            r.cmd = cmd;
            r.pin = pin;
            r.userId = userId;
            //r.lan = Local.Instance.set.Language;
            string str = JsonUtility.ToJson(r);
            SendAesJson(JavaAES.Instance.Encrypt(str));
        }
        public void Send(req r, string cmd, string type = "def")
        {
            r.type = type;
            r.cmd = cmd;
            r.pin = pin;
            r.userId = userId;
            //r.lan = Local.Instance.set.Language;
            string str = JsonUtility.ToJson(r);
            SendAesJson(JavaAES.Instance.Encrypt(str));
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