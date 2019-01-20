using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace huqiang
{
    /// <summary>
    /// 控制位置和旋转的动画类
    /// </summary>
    public class Animat : AnimatBase, AnimatInterface
    {
        /// <summary>
        /// 设置一个抛物线的中间点，x的值在0-1范围内,返回一个抛物线一般表达式的a，b，c的值，
        /// </summary>
        /// <param name="ani"></param>
        /// <param name="point">参数不能为(1,1)</param>
        public static Animat SetParabola(Animat ani, Vector2 point)
        {
            var v = MathH.Parabola(Vector2.zero, point, new Vector2(1, 1));
            ani.DataCache = new float[3];
            ani.DataCache[0] = v.x;
            ani.DataCache[1] = v.y;
            ani.DataCache[2] = v.z;
            ani.Linear = Parabola;
            return ani;
        }
        static float Parabola(AnimatBase ani, float a)
        {
            var temp = ani.DataCache;
            return temp[0] * a * a + a * temp[1] + temp[2];
        }
        /// <summary>
        /// 设置一个单弧线
        /// </summary>
        /// <param name="ani"></param>
        /// <param name="r">弧度0-180</param>
        public static Animat SetArc(Animat ani, float r)
        {
            ani.DataCache = new float[3];
            ani.DataCache[0] = r;//弧度
            ani.DataCache[1] = 270 - r * 0.5f;//起始弧度
            ani.DataCache[2] = MathH.Cos(270 + r * 0.5f);//弧度总长
            ani.Linear = Arc;
            return ani;
        }
        static float Arc(AnimatBase ani, float a)
        {
            float s = ani.DataCache[1] + a * ani.DataCache[0];
            return 0.5f + 0.5f * MathH.Cos(s) / ani.DataCache[2];//180-360 -1到1之间
        }
        /// <summary>
        /// 设置一个S行曲线
        /// </summary>
        /// <param name="ani"></param>
        /// <param name="x">0-0.5f</param>
        /// <param name="y">0-0.5f</param>
        public static Animat SetSArc(Animat ani, float x = 0.4f, float y = 0.1f)
        {
            var tmp = new Vector3[5];
            tmp[1].x = x;
            tmp[1].y = y;
            tmp[2] = new Vector3(0.5f, 0.5f, 0);
            tmp[3].x = 0.5f + y;
            tmp[3].y = 0.5f + x;
            tmp[4] = new Vector3(1, 1, 0);
            ani.points = tmp;
            ani.Linear = SArc;
            return ani;
        }
        static float SArc(AnimatBase ani, float a)
        {
            var tmp = ani.points;
            if (tmp == null)
                return a;
            if (a < 0.5f)
            {
                a *= 2;
                return MathH.BezierPoint(a, ref tmp[0], ref tmp[1], ref tmp[2]).y;
            }
            else
            {
                a = (a - 0.5f) * 2;
                return MathH.BezierPoint(a, ref tmp[2], ref tmp[3], ref tmp[4]).y;
            }

        }
        public Animat(Transform t)
        {
            Target = t;
            AnimationManage.Manage.AddAnimat(this);
        }
        public Transform Target;
        public PlayStyle playstyle;
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public Vector3 StartAngle;
        public Vector3 EndAngle;
        public Vector3 StartScale;
        public Vector3 EndScale;
        public Action<Animat> PlayStart;
        public Action<Animat> PlayOver;
        public bool ReleaseOnOver;
        public void Update(float timeslice)
        {
            if (playing)
            {
                if (Delay > 0)
                {
                    Delay -= timeslice;
                    if (Delay <= 0)
                    {
                        if (PlayStart != null)
                            PlayStart(this);
                        c_time = -Delay;
                    }
                }
                else
                {
                    c_time += timeslice;
                    if (!Loop & c_time >= m_time)
                    {
                        playing = false;
                        if ((playstyle & PlayStyle.Position) > 0)
                        {
                            Target.localPosition = EndPosition;
                        }
                        if ((playstyle & PlayStyle.Angle) > 0)
                        {
                            Target.localEulerAngles = EndAngle;
                        }
                        if (PlayOver != null)
                        {
                            PlayOver(this);
                        }
                        if (ReleaseOnOver)
                            Dispose();
                    }
                    else
                    {
                        if (c_time >= m_time)
                            c_time -= m_time;
                        float r = c_time / m_time;
                        if (Linear != null)
                            r = Linear(this, r);
                        if ((playstyle & PlayStyle.Position) > 0)
                        {
                            Vector3 v = EndPosition - StartPosition;
                            Target.localPosition = StartPosition + v * r;
                        }
                        if ((playstyle & PlayStyle.Angle) > 0)
                        {
                            Vector3 v = EndAngle - StartAngle;
                            Target.localEulerAngles = StartAngle + v * r;
                        }
                    }
                }
            }
        }
        public void Dispose()
        {
            AnimationManage.Manage.ReleaseAnimat(this);
        }
    }
}
