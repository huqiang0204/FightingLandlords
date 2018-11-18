using System;
using System.Net;
using UnityEngine;
using System.Text;

namespace huqiang.Data
{
    public class Req
    {
        public const Int32 Cmd = 0;
        public const Int32 Type = 1;
        public const Int32 Args = 2;
        public const Int32 Length = 3;
    }
    public class MessageType
    {
        public const Int32 Def = 0;
        public const Int32 Rpc = 1;
        public const Int32 Query = 2;
    }

    public class UdpDataControll
    {
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
        static UdpDataControll ins;
        public static UdpDataControll Instance { get { if (ins == null) ins = new UdpDataControll(); return ins; } }
        UdpSocket socket;
        IPEndPoint remote;
        
        public void Connection(int port, IPAddress address,int rPort)
        {
            remote = new IPEndPoint(address,rPort);
            socket = new UdpSocket(port,remote,false,PackType.Total);
            socket.MainDispatch = DispatchEx;
        }
        float time;
        bool Connet=false;
        public void DispatchMessage()
        {
            if(socket!=null)
            {
                socket.Dispatch();
            }
        }
        void DispatchEx(byte[] data, UInt32 tag, IPEndPoint ip)
        {
            var s = tag;
            byte type = (byte)tag;
            switch (type)
            {
                case EnvelopeType.Mate:
                    DispatchMetaData(data,ip);
                    break;
                case EnvelopeType.AesJson:
                    byte[] dec = JavaAES.Instance.Decrypt(data, 0, data.Length);
                    var json = Encoding.UTF8.GetString(dec);
                    DispatchJson(json,ip);
                    break;
                case EnvelopeType.Json:
                    json = Encoding.UTF8.GetString(data);
                    DispatchJson(json,ip);
                    break;
                case EnvelopeType.AesDataBuffer:
                    dec = JavaAES.Instance.Decrypt(data, 0, data.Length);
                    DataBuffer buff = new DataBuffer(dec);
                    DispatchStream(buff,ip);
                    break;
                case EnvelopeType.DataBuffer:
                    buff = new DataBuffer(data);
                    DispatchStream(buff,ip);
                    break;
                case EnvelopeType.String:
                    json = Encoding.UTF8.GetString(data);
                    DispatchString(json,ip);
                    break;
            }
        }
        void DispatchMetaData(byte[] data,IPEndPoint ip)
        {

        }
        void DispatchString(string json, IPEndPoint ip)
        {
    
        }
        void DispatchJson(string json, IPEndPoint ip)
        {

        }
        void DispatchStream(DataBuffer buffer, IPEndPoint ip)
        {
            var fake = buffer.fakeStruct;
            if(fake!=null)
            {
                switch(fake[Req.Type])
                {
                    case MessageType.Def:
                        break;
                    case MessageType.Rpc:
                        break;
                    case MessageType.Query:
                        break;
                }
            }
        }
        public void SendString(string dat, IPEndPoint ip)
        {
        }

        public void Send(byte[] dat, IPEndPoint ip)
        {
            socket.Send(dat, remote, (byte)EnvelopeType.DataBuffer);
        }
        public void Close()
        {
            if(socket!=null)
            socket.Close();
        }
    }
}