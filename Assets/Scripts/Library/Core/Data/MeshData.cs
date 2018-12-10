using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace huqiang.Data
{
    public struct Coordinate
    {
        public Vector3 pos;
        public Vector3 scale;
        public Quaternion quat;
    }
    public class MeshData
    {
        enum DataType
        {
            Coordinate = -1,
            Name = 0,
            Vertices = 1,
            Normals = 2,
            UV = 3,
            Triangles = 4,
            Child = 5,
            Type = 6
        }
        public static byte[] Zreo = new byte[4];
        public MeshData[] child;
        public int Type;
        public string name;
        public Vector3[] vertex;
        public Vector2[] uv;
        public Vector3[] normals;
        public Int32[] tris;
        public Coordinate coordinate;
        public MeshData()
        {
            coordinate.scale = Vector3.one;
            coordinate.quat.x = 0;
            coordinate.quat.y = 0;
            coordinate.quat.z = 0;
            coordinate.quat.w = 1;
        }

        public unsafe byte* LoadFromBytes(byte* bp)
        {
            int seg = *(Int32*)bp;
            bp += 4;
            for (int i = 0; i < seg; i++)
            {
                int tag = *(Int32*)bp;
                bp += 4;
                switch ((DataType)tag)
                {
                    case DataType.Type:
                        Type = *(Int32*)bp;
                        bp += 4;
                        break;
                    case DataType.Coordinate:
                        unsafe
                        {
                            fixed (Coordinate* coor = &coordinate)
                            { ReadCoordinate(bp, coor); }
                        }
                        bp += 40;
                        break;
                    case DataType.Name:
                        int c = *(Int32*)bp;
                        bp += 4;
                        name = Encoding.UTF8.GetString(Tool.GetByteArray(bp, c));
                        bp += c;
                        break;
                    case DataType.Vertices:
                        vertex = ReadVectors(bp);
                        if (vertex != null)
                            bp += vertex.Length * 12;
                        bp += 4;
                        break;
                    case DataType.Normals:
                        normals = ReadVectors(bp);
                        if (normals != null)
                            bp += normals.Length * 12;
                        bp += 4;
                        break;
                    case DataType.UV:
                        uv = ReadUV(bp);
                        if (uv != null)
                            bp += uv.Length * 8;
                        bp += 4;
                        break;
                    case DataType.Triangles:
                        tris = ReadTri(bp);
                        if (tris != null)
                            bp += tris.Length * 4;
                        bp += 4;
                        break;
                    case DataType.Child:
                        bp = ReadChild(bp, this);
                        break;
                }
            }
            return bp;
        }
        unsafe static Vector3[] ReadVectors(byte* bp)
        {
            int len = *(Int32*)bp;
            bp += 4;
            if (len > 0)
            {
                len /= 12;
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
                len /= 8;
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
        unsafe static void ReadCoordinate(byte* bp, Coordinate* coordinate)
        {
            float* tp = (float*)bp;
            float* fp = (float*)coordinate;
            for (int i = 0; i < 10; i++)
            {
                *fp = *tp;
                fp++;
                tp++;
            }
        }
        unsafe static int[] ReadTri(byte* bp)
        {
            var len = *(Int32*)bp;
            bp += 4;
            if (len > 0)
            {
                len /= 4;
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
        unsafe static byte* ReadChild(byte* bp, MeshData mesh)
        {
            var len = *(Int32*)bp;
            bp += 4;
            if (len > 0)
            {
                mesh.child = new MeshData[len];
                for (int i = 0; i < len; i++)
                {
                    MeshData sub = new MeshData();
                    mesh.child[i] = sub;
                    bp = sub.LoadFromBytes(bp);
                }
            }
            return bp;
        }

        int GetSegment()
        {
            int seg = 2;
            if (name != null)
                seg++;
            if (vertex != null)
                seg++;
            if (uv != null)
                seg++;
            if (normals != null)
                seg++;
            if (tris != null)
                seg++;
            if (child != null)
                seg++;
            return seg;
        }
        public void WriteToStream(Stream stream)
        {
            Int32 seg = GetSegment();
            var tmp = seg.ToBytes();
            stream.Write(tmp, 0, 4);
            stream.Write(((Int32)DataType.Type).ToBytes(), 0, 4);
            stream.Write(Type.ToBytes(), 0, 4);
            unsafe
            {
                fixed (Coordinate* coor = &coordinate)
                {
                    WriteCoordinate(stream, coor);
                }
            }
            if (name != null)
                WriteName(stream, name);
            if (vertex != null)
            {
                tmp = ((Int32)DataType.Vertices).ToBytes();
                stream.Write(tmp, 0, 4);
                WriteVectors(stream, vertex);
            }
            if (normals != null)
            {
                tmp = ((Int32)DataType.Normals).ToBytes();
                stream.Write(tmp, 0, 4);
                WriteVectors(stream, normals);
            }
            if (uv != null)
                WriteUV(stream, uv);
            if (tris != null)
                WriteTri(stream, tris);
            if (child != null)
                WriteChild(stream, child);
        }
        public void WriteToFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
            var fs = File.Create(path);
            WriteToStream(fs);
            fs.Dispose();
        }
        unsafe static void WriteCoordinate(Stream stream, Coordinate* coordinate)
        {
            var tmp = ((Int32)DataType.Coordinate).ToBytes();
            stream.Write(tmp, 0, 4);
            byte[] buf = new byte[40];
            byte* fp = (byte*)coordinate;
            for (int i = 0; i < 40; i++)
            {
                buf[i] = *fp;
                fp++;
            }
            stream.Write(buf, 0, 40);
        }
        static void WriteName(Stream stream, string name)
        {
            var tmp = ((Int32)DataType.Name).ToBytes();
            stream.Write(tmp, 0, 4);
            var buf = Encoding.UTF8.GetBytes(name);
            Int32 len = buf.Length;
            stream.Write(len.ToBytes(), 0, 4);
            stream.Write(buf, 0, len);
        }
        static void WriteVectors(Stream stream, Vector3[] vectors)
        {
            int len = vectors.Length * 12;
            IntPtr v = Marshal.UnsafeAddrOfPinnedArrayElement(vectors, 0);
            var buf = new byte[len];
            Marshal.Copy(v, buf, 0, len);
            stream.Write(len.ToBytes(), 0, 4);
            stream.Write(buf, 0, buf.Length);
        }
        static void WriteUV(Stream stream, Vector2[] uvs)
        {
            var tmp = ((Int32)DataType.UV).ToBytes();
            stream.Write(tmp, 0, 4);
            int len = uvs.Length * 8;
            IntPtr v = Marshal.UnsafeAddrOfPinnedArrayElement(uvs, 0);
            var buf = new byte[len];
            Marshal.Copy(v, buf, 0, len);
            stream.Write(len.ToBytes(), 0, 4);
            stream.Write(buf, 0, buf.Length);
        }
        static void WriteTri(Stream stream, int[] tri)
        {
            var tmp = ((Int32)DataType.Triangles).ToBytes();
            stream.Write(tmp, 0, 4);
            int len = tri.Length * 4;
            IntPtr v = Marshal.UnsafeAddrOfPinnedArrayElement(tri, 0);
            var buf = new byte[len];
            Marshal.Copy(v, buf, 0, len);
            stream.Write(len.ToBytes(), 0, 4);
            stream.Write(buf, 0, buf.Length);
        }
        static void WriteChild(Stream stream, MeshData[] meshes)
        {
            var tmp = ((Int32)DataType.Child).ToBytes();
            stream.Write(tmp, 0, 4);
            int len = meshes.Length;
            tmp = len.ToBytes();
            stream.Write(tmp, 0, 4);
            for (int i = 0; i < len; i++)
            {
                meshes[i].WriteToStream(stream);
            }
        }

        public GameObject CreateGameObject()
        {
            GameObject game = new GameObject(name);
            if (vertex != null)
            {
                game.AddComponent<MeshRenderer>();
                var mf = game.AddComponent<MeshFilter>();
                var ms = mf.mesh;
                ms.vertices = vertex;
                if (normals != null)
                    ms.normals = normals;
                if (tris != null)
                    ms.triangles = tris;
                else
                {
                    tris = new int[vertex.Length];
                    for (int i = 0; i < tris.Length; i++)
                        tris[i] = i;
                    ms.triangles = tris;
                }
            }
            if (child != null)
                for (int i = 0; i < child.Length; i++)
                {
                    var ch = child[i];
                    var son = ch.CreateGameObject();
                    var tran = son.transform;
                    tran.SetParent(game.transform);
                    tran.localPosition = ch.coordinate.pos;
                    tran.localScale = ch.coordinate.scale;
                    tran.localRotation = ch.coordinate.quat;
                }
            return game;
        }
        public void CreateNormal()
        {
            if (vertex != null)
                if (tris != null)
                {
                    normals = new Vector3[vertex.Length];
                    int c = tris.Length / 3;
                    for (int i = 0; i < c; i++)
                    {
                        int s = i * 3;
                        var nor = MathH.GetTriangleNormal(vertex[tris[s]], vertex[tris[s + 1]], vertex[tris[s + 2]]);
                        normals[s] = nor;
                        normals[s + 1] = nor;
                        normals[s + 2] = nor;
                    }
                }
        }

        public static MeshData LoadFromGameObject(Transform game)
        {
            var mesh = new MeshData();
            mesh.name = game.name;
            var mf = game.GetComponent<MeshFilter>();
            if (mf != null)
            {
                mesh.Type = 1;
                if (Application.isPlaying)
                {
                    mesh.vertex = mf.mesh.vertices;
                    mesh.tris = mf.mesh.triangles;
                }
                else
                {
                    mesh.vertex = mf.sharedMesh.vertices;
                    mesh.tris = mf.sharedMesh.triangles;
                }
            }
            else
            {
                mesh.Type = 0;
            }
            var t = game.transform;
            mesh.coordinate.pos = t.localPosition;
            mesh.coordinate.scale = t.localScale;
            mesh.coordinate.quat = t.localRotation;
            int c = t.childCount;
            if (c > 0)
            {
                mesh.child = new MeshData[c];
                for (int i = 0; i < c; i++)
                    mesh.child[i] = LoadFromGameObject(t.GetChild(i));
            }
            return mesh;
        }
    }
}
