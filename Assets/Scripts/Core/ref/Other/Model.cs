using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace huqiang
{
    public interface IView
    {
    }
    public class Model
    {
        public struct ConeData
        {
            public Vector3[] Vertex;
            public int[] Tri;
        }
        /// <summary>
        /// 创建一个圆锥，返回顶点和三角形
        /// </summary>
        /// <param name="r">半径</param>
        /// <param name="h">高度</param>
        /// <param name="arc">三角形弧度，越小精度越高，范围0-360取整</param>
        /// <returns>顶点，三角形</returns>
        public static ConeData CreateCone(float r, float h, float arc)
        {
            int a = (int)arc;
            int c = 360 / a;
            int vc = c + 2;
            Vector3[] vertex = new Vector3[vc];
            int[] tri = new int[c * 6];
            int o = c;
            int s = 0;
            int i = 0;
            for (; i < c; i++)
            {
                vertex[i].x = -MathH.Sin(s) * r; 
                vertex[i].z = MathH.Cos(s) * r;
                tri[i * 3] = i + 1;
                tri[i * 3 + 1] = c;
                tri[i * 3 + 2] = i;
                tri[o * 3] = i;
                tri[o * 3 + 1] = c + 1;
                tri[o * 3 + 2] = i + 1;
                o++;
                s += a;
            }
            i--;
            o--;
            vertex[c + 1].y = h;
            tri[i * 3] = 0;
            tri[o * 3 + 2] = 0;
            ConeData cd = new ConeData();
            cd.Vertex = vertex;
            cd.Tri = tri;
            return cd;
        }
        public static void ViewToClass(IView v, Transform trans)
        {
            if (!(v is IView))
                return;
            var member = v.GetType().GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < member.Length; i++)
            {
                var t = member[i].FieldType;
                if(t==typeof(IView))
                {
                    char[] name = t.FullName.ToCharArray();
                    int len = name.Length - 2;
                    if (name[len] == '[')
                        if (name[len + 1] == ']')
                        {
                            char[] buff = new char[len];
                            for (int b = 0; b < len; b++)
                                buff[b] = name[b];
                            name = buff;
                        }
                    string fullname = new string(name);
                    var o = t.Assembly.CreateInstance(fullname);
                    ViewToClass(o as IView,trans.Find(member[i].Name));
                    member[i].SetValue(v, o);
                }
                else if(t==typeof(MonoBehaviour))
                {
                    member[i].SetValue(v, trans.GetComponent(t));
                }
            }
        }
    }
}
