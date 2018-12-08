﻿using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang
{
    public static class Extand
    {
        static byte[] Zreo = new byte[4];
        static string hex = "0123456789abcdef";
        public static Color32 HexToColor(this string value)
        {
            if (value == null)
                return new Color32();
            byte[] tmp = new byte[8];

            for (int i = 0; i < value.Length; i++)
            {
                tmp[i] = (byte)hex.IndexOf(value[i]);
                if (tmp[i] < 0)
                    tmp[i] = 0;
            }
            for (int i = 0; i < 8; i += 2)
            {
                tmp[i] *= 16;
                tmp[i] += tmp[i + 1];
            }
            return new Color32(tmp[0], tmp[2], tmp[4], tmp[6]);
        }
        public unsafe static Color32 ToColor(this Int32 value)
        {
            byte* b = (byte*)&value;
            return new Color32(*(b + 3), *(b + 2), *(b + 1), *b);
        }
        public unsafe static byte[] ToBytes(this Int16 value)
        {
            byte[] buff = new byte[2];
            fixed (byte* bp = &buff[0])
                *(Int16*)bp = value;
            return buff;
        }
        public unsafe static byte[] ToBytes(this Int32 value)
        {
            byte[] buff = new byte[4];
            fixed (byte* bp = &buff[0])
                *(Int32*)bp = value;
            return buff;
        }
        public unsafe static byte[] ToBytes(this Single value)
        {
            byte[] buff = new byte[4];
            fixed (byte* bp = &buff[0])
                *(Single*)bp = value;
            return buff;
        }
        public unsafe static byte[] ToBytes(this Int64 value)
        {
            byte[] buff = new byte[8];
            fixed (byte* bp = &buff[0])
                *(Int64*)bp = value;
            return buff;
        }
        public unsafe static byte[] ToBytes(this Double value)
        {
            byte[] buff = new byte[8];
            fixed (byte* bp = &buff[0])
                *(Double*)bp = value;
            return buff;
        }
        public static Color32 ToColor(this UInt32 value)
        {
            unsafe
            {
                byte* b = (byte*)&value;
                return new Color32(*(b + 3), *(b + 2), *(b + 1), *b);
            }
        }
        public static void SetSprite(this RawImage raw, Sprite sprite)
        {
            if (sprite == null)
            {
                raw.texture = null;
                return;
            }
            var r = sprite.rect;
            raw.texture = sprite.texture;
            float w = sprite.texture.width;
            float h = sprite.texture.height;
            raw.uvRect = new Rect(r.x / w, r.y / h, r.width / w, r.height / h);
            raw.SetNativeSize();
        }
        public static void SetRect(this RawImage raw, Rect rect)
        {
            if (raw.texture != null)
            {
                float w = raw.texture.width;
                float h = raw.texture.height;
                raw.uvRect = new Rect(rect.x / w, rect.y / h, rect.width / w, rect.height / h);
                raw.SetNativeSize();
            }
        }
        public static T Clone<T>(this T obj) where T : class, new()
        {
            if (obj != null)
            {
                try
                {
                    Type type = obj.GetType();
                    var tmp = Activator.CreateInstance(type);
                    var fields = type.GetFields();
                    if (fields != null)
                    {
                        for (int i = 0; i < fields.Length; i++)
                        {
                            var f = fields[i];
                            var ft = f.FieldType;
                            if (ft.IsClass)
                            {
                                if (ft == typeof(string))
                                {
                                    f.SetValue(tmp, f.GetValue(obj));
                                }
                                else
                                {
                                    f.SetValue(tmp, f.GetValue(obj).Clone());
                                }
                            }
                            else
                            {
                                f.SetValue(tmp, f.GetValue(obj));
                            }
                        }
                    }
                    return tmp as T;
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.StackTrace);
                }
            }
            return null;
        }
        public static void WriteString(this Stream stream, string str)
        {
            if (str == null)
            {
                stream.Write(Zreo, 0, 4);
            }
            else if (str.Length == 0)
            {
                stream.Write(Zreo, 0, 4);
            }
            else
            {
                var buf = Encoding.UTF8.GetBytes(str);
                stream.Write(buf.Length.ToBytes(), 0, 4);
                stream.Write(buf, 0, buf.Length);
            }
        }
        public unsafe static void Write(this Stream stream, byte* p, int size)
        {
            for (int i = 0; i < size; i++)
            { stream.WriteByte(*p); p++; }
        }
        public unsafe static Int32 ReadInt32(this byte[] buff, Int32 offset)
        {
            fixed (byte* bp = &buff[0])
                return *(Int32*)(bp + offset);
        }
        public unsafe static void Read(this byte[] buff, void* p, int offset, int size)
        {
            byte* bp = (byte*)p;
            for (int i = 0; i < size; i++)
            {
                *bp = buff[offset];
                bp++;
                offset++;
            }
        }
    }
}