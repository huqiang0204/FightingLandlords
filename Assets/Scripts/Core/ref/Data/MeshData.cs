using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace huqiang
{
    public class MeshData
    {
        public static byte[] Zreo = new byte[4];
        public string name;
        public Vector3[] vertex;
        public Vector3[] normals;
        public Vector2[] uv;
        public Int32[] tris;
        public unsafe void LoadFromBytes(byte* bp)
        {
            int len = *(Int32*)bp;
            bp += 4;
            for (int i = 0; i < len; i++)
            {
                int tag = *(Int32*)bp;
                bp += 4;
                switch (tag)
                {
                    case 0:
                        int c = *(Int32*)bp;
                        bp += 4;
                        name = Encoding.UTF8.GetString(Tool.GetByteArray(bp, c));
                        bp += c;
                        break;
                    case 1:
                        vertex = ReadVectors(bp);
                        if (vertex != null)
                            bp += vertex.Length * 12;
                        bp += 4;
                        break;
                    case 2:
                        normals = ReadVectors(bp);
                        if (normals != null)
                            bp += normals.Length * 12;
                        bp += 4;
                        break;
                    case 3:
                        uv = ReadUV(bp);
                        if (uv != null)
                            bp += uv.Length * 8;
                        bp += 4;
                        break;
                    case 4:
                        tris = ReadTri(bp);
                        if (tris != null)
                            bp += tris.Length * 4;
                        bp += 4;
                        break;
                }
            }
        }
        unsafe static Vector3[] ReadVectors(byte* bp)
        {
            int len = *(Int32*)bp;
            bp += 4;
            if (len > 0)
            {
                Vector3* p = (Vector3*)bp;
                var buf = new Vector3[len];
                for (int i = 0; i < len; i++)
                {
                    buf[i] = *p;
                    p++;
                }
                return buf;
            }
            return null;
        }
        unsafe static Vector2[] ReadUV(byte* bp)
        {
            int len = *(Int32*)bp;
            bp += 4;
            if (len > 0)
            {
                Vector2* p = (Vector2*)bp;
                var buf = new Vector2[len];
                for (int i = 0; i < len; i++)
                {
                    buf[i] = *p;
                    p++;
                }
                return buf;
            }
            return null;
        }
        unsafe static int[] ReadTri(byte* bp)
        {
            var len = *(Int32*)bp;
            bp += 4;
            if (len > 0)
            {
                Int32* p = (Int32*)bp;
                var buf = new int[len];
                for (int i = 0; i < len; i++)
                {
                    buf[i] = *p;
                    p++;
                }
                return buf;
            }
            return null;
        }
  
        public bool IntersectTriangle(ref Vector3 orig, ref Vector3 dir, ref Vector3 result)
        {
            int c = tris.Length;
            c /= 3;
            int index = 0;
            for (int i = 0; i < c; i++)
            {
                if (MathH.IntersectTriangle(ref orig, ref dir, ref vertex[tris[index]], 
                    ref vertex[tris[index + 1]], ref vertex[tris[index + 2]], ref result))
                    return true;
                index += 3;
            }
            return false;
        }
    }
    public class ObjectData
    {
        class Mission
        {
            public string path;
            public Action<object> callback;
        }
        public static void LoadAsync(string path,Action<object> callback)
        {
            Mission m = new Mission();
            m.path = path;
            m.callback = callback;
            ThreadPool.AddMission(Load,m);
        }
        static void Load(object o)
        {
            Mission m = o as Mission;
            if (File.Exists(m.path))
            {
                var fs = File.Open(m.path, FileMode.Open);
                byte[] dat = new byte[fs.Length];
                fs.Read(dat, 0, dat.Length);
                fs.Dispose();
                var os = LoadToGame(dat);
                if (m.callback != null)
                {
                    ThreadPool.InvokeToMain(m.callback,os);
                }
            }
        }
        public static unsafe ObjectData[] LoadToGame(byte[] dat)
        {
            fixed(byte* bp=&dat[0])
            {
                byte* add = bp;
                int len = *(Int32*)add;
                add += 4;
                ObjectData[] games = new ObjectData[len];
                for(int i=0;i<len;i++)
                {
                    int gl = *(Int32*)add;
                    add += 4;
                    games[i] = new ObjectData();
                    games[i].LoadFromBytes(add);
                    add += gl;
                }
                return games;
            }
        }
        static byte[] Zreo = new byte[4];
        public string name;
        //public Vector3[] vertex;
        //public int[] tri;
        public MeshData[] child;
        public unsafe void LoadFromBytes(byte* bp)
        {
            int len = *(Int32*)bp;
            bp += 4;
            if (len>0)
            {
                name = Encoding.UTF8.GetString(Tool.GetByteArray(bp,len));
                bp += len;
            }
            len = *(Int32*)bp;
            bp += 4;
            if (len>0)
            {
                child = new MeshData[len];
                for(int i=0;i<len;i++)
                {
                    child[i] = new MeshData();
                    int ml = *(Int32*)bp;
                    bp += 4;
                    child[i].LoadFromBytes(bp);
                    bp += ml;
                }
            }
        }
        public static GameObject ToGameObject(ObjectData data)
        {
            GameObject go = new GameObject();
            go.name = data.name;
            int c = data.child.Length;
            Shader shader = Shader.Find("Standard");
            for (int i=0;i<c;i++)
            {
                GameObject t = new GameObject();
                var mr= t.AddComponent<MeshRenderer>();
                mr.material = new Material(shader);
                var mf = t.AddComponent<MeshFilter>();
                var ms = mf.mesh;
                var s = data.child[i];
                if(s.vertex!=null)
                {
                    ms.vertices = s.vertex;
                    ms.normals = s.normals;
                    ms.triangles = s.tris;
                }
                t.name = s.name;
                t.transform.SetParent(go.transform);
            }
            return go;
        }
    }
}
