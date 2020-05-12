using System;
using System.Net;
using UnityEngine;
using System.Text;
using huqiang;
using huqiang.Data;

namespace DataControll
{
    public class Req
    {
        public const Int32 Cmd = 0;
        public const Int32 Type = 1;
        public const Int32 Error = 2;
        public const Int32 Args = 3;
        public const Int32 Length = 4;
    }
    public class MessageType
    {
        public const Int32 Background = -1;
        public const Int32 Def = 0;
        public const Int32 Rpc = 1;
        public const Int32 Query = 2;
        public const Int32 Game = 3;
    }
    [Serializable]
    public class LogData
    {
        public string condition;
        public string stackTrace;
        public LogType type;
    }
    public class KcpDataControll
    {
        public class KcpData
        {
            public byte[] dat;
            public byte tag;
        }
        public class KcpSocket : KcpLink
        {
            public QueueBuffer<KcpData> datas;
            public KcpSocket()
            {
                datas = new QueueBuffer<KcpData>();
            }
            public override void Dispatch(byte[] dat, byte tag)
            {
                KcpData data = new KcpData();
                data.dat = dat;
                data.tag = tag;
                datas.Enqueue(data);
            }
            public override void Disconnect()
            {
            }
            public override void ConnectionOK()
            {
                //LoginTable login = new LoginTable();
                //login.user = "hujianhai";
                //login.pass = "123456";
                //Instance.SendObject<LoginTable>(DefCmd.Login,MessageType.Def,login);
            }
        }
        static KcpDataControll ins;
        public static KcpDataControll Instance { get { if (ins == null) ins = new KcpDataControll(); return ins; } }
        public bool Connected { get { if (link == null)
                    return false;
                return link.Connected;
            } }
        string UniId;
        KcpSocket link;
        public void Connection(string ip,int port)
        {
            UniId = SystemInfo.deviceUniqueIdentifier;
            var address = IPAddress.Parse(ip);
            var kcp = new KcpServer<KcpSocket>(0);
            kcp.Run(1);
            link = kcp.FindOrCreateLink(new IPEndPoint(address,port));
            link.Send(new byte[1], 0);
        }
        public int pin;
        public int userId;
        public void FailedConnect()
        {
        }
        public void OpenLog()
        {
            Application.logMessageReceived += Log;
        }
        public void Log(string condtion, string stack, LogType type)
        {
            LogData log = new LogData();
            log.condition = condtion;
            log.stackTrace = stack;
            log.type = type;
            var str = JsonUtility.ToJson(log);
            link.Send(Encoding.UTF8.GetBytes(str), EnvelopeType.String);
        }
        public void Close()
        {
            if(KcpListener.Instance!=null)
            KcpListener.Instance.Dispose();
            Application.logMessageReceived -= Log;
            if (link != null)
                link.Dispose();
            link = null;
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
            catch(Exception e)
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
                    if (dec!= null)
                        DispatchStream(new DataBuffer(dec));
                    break;
                case EnvelopeType.DataBuffer:

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
            //var j = JsonUtility.FromJson<reqs>(json);
        }
        void DispatchStream(DataBuffer buffer)
        {
            var fake = buffer.fakeStruct;
           
            if (fake != null)
            {
                if (fake[Req.Error] > 0)
                {
                    switch (fake[Req.Type])
                    {
                        case MessageType.Def:
                            //DefaultErrorControll.Dispatch(buffer);
                            break;
                        case MessageType.Rpc:
                            //RpcErrorControll.Dispatch(buffer);
                            break;
                        case MessageType.Query:
                            //QueryData.Dispatch(linker, buffer);
                            break;
                    }
                }
                else
                {
                    switch (fake[Req.Type])
                    {
                        case MessageType.Def:
                            //DefaultDataControll.Dispatch(buffer);
                            break;
                        case MessageType.Game:
                            //GameDataControll.Dispatch(buffer);
                            break;
                        case MessageType.Rpc:
                            //RpcDataControll.Dispatch(buffer);
                            break;
                        case MessageType.Query:
                            //QueryData.Dispatch(linker, buffer);
                            break;
                    }
                }
            }
        }
        void SendAesJson(byte[] dat )
        {
            link.Send(dat, EnvelopeType.AesJson); 
        }
        public void SendAesStream(DataBuffer db)
        {
            var dat = db.ToBytes();
            dat = AES.Instance.Encrypt(dat);
            link.Send(dat, EnvelopeType.AesDataBuffer);
        }
        public void SendString(Int32 cmd, Int32 type, string obj)
        {
            DataBuffer db = new DataBuffer(4);
            var fs = db.fakeStruct = new FakeStruct(db, Req.Length);
            fs[Req.Cmd] = cmd;
            fs[Req.Type] = type;
            fs.SetData(Req.Args, obj);
            var dat = db.ToBytes();
            dat = AES.Instance.Encrypt(dat);
            link.Send(dat, EnvelopeType.AesDataBuffer);
        }
        public void SendNull(Int32 cmd, Int32 type)
        {
            DataBuffer db = new DataBuffer(4);
            var fs = db.fakeStruct = new FakeStruct(db, Req.Length);
            fs[Req.Cmd] = cmd;
            fs[Req.Type] = type;
            var dat = db.ToBytes();
            dat = AES.Instance.Encrypt(dat);
            link.Send(dat, EnvelopeType.AesDataBuffer);
        }
        public void SendInt(Int32 cmd, Int32 type, int args)
        {
            DataBuffer db = new DataBuffer(4);
            var fs = db.fakeStruct = new FakeStruct(db, Req.Length);
            fs[Req.Cmd] = cmd;
            fs[Req.Type] = type;
            fs[Req.Args]= args;
            var dat = db.ToBytes();
            dat = AES.Instance.Encrypt(dat);
            link.Send(dat, EnvelopeType.AesDataBuffer);
        }
    }
}