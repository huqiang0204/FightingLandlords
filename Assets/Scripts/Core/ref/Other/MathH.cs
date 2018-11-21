using System;
using System.Collections.Generic;
using UnityEngine;

namespace huqiang
{
    public struct FontInfo
    {
        public float Width;
        public float Height;
        public float RenderWidth;
    }
    public  class MathH
    {
        #region angle table  用于二维旋转，-sin(angle) cos(angle)
        /// <summary>
        /// 初始化数据
        /// </summary>
        public static void Inital()
        {
            anglebuff = Properties.Resources.sin9000;
            unsafe
            {
                fixed (byte* bp = &anglebuff[0])
                    ap = (float*)bp;
            }
            fontbuff = Properties.Resources.ARIALUNI;
            unsafe
            {
                fixed (byte* bp = &fontbuff[0])
                    fp = (int*)bp;
            }
        }
        public static unsafe void GetFontInfo(char c,float fontsize,ref FontInfo info)
        {
            fontsize *=0.01f;
            Int32 a = *(fp + c);
            Int32 t = a;
            float f = a & 0xffff;
            f *= 0.01f;
            info.RenderWidth = f*fontsize;
            a >>= 16;
            f = a&0xff;
            info.Height = f*fontsize;
            a >>= 8;
            info.Width = a*fontsize;
        }
        static byte[] fontbuff;
        unsafe static Int32* fp;
        static byte[] anglebuff;
        unsafe static float* ap;
        /// <summary>
        /// 范围为0-360， 精度为0.01
        /// </summary>
        /// <param name="ax"></param>
        /// <returns></returns>
        public unsafe static float Sin(float ax)
        {
            if (anglebuff == null)
                Inital();
            if (ax >= 360)
                ax -= 360;
            if (ax < 0)
                ax += 360;
            if (ax > 270)
            {
                ax = 360 - ax;
                goto label1;
            }
            else if (ax > 180)
            {
                ax -= 180;
                goto label1;
            }
            else if (ax > 90)
            {
                ax = 180 - ax;
            }
            int id = (int)(ax * 100);
            return *(ap + id);
            label1:;
            id = (int)(ax * 100);
            return -*(ap + id);
        }
        /// <summary>
        /// 范围为0-360， 精度为0.01
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float Cos(float angle)
        {
            return Sin(angle + 90);
        }
        /// <summary>
        /// 范围为0-360， 精度为0.01
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float Tan(float angle)
        {
            var a = Sin(angle);
            var b = Cos(angle);
            if (b == 0)
                return 0;
            return a / b;
        }
        #endregion

        #region function
        /// <summary>
        /// 求平方根的快速算法，实际速度比微软自带的慢很多，算法仅供参考
        /// </summary>
        /// <param name="number"></param>
        /// <returns>平方根</returns>
        unsafe public static float SquareRootFloat(float number)
        {
            int i; float x, y;
            const float f = 1.5F;
            x = number * 0.5F;
            y = number;
            i = *(int*)&y;
            i = 0x5f375a86 - (i >> 1); //注意这一行  
            y = *(float*)&i;
            y = y * (f - (x * y * y));
            y = y * (f - (x * y * y));//迭代提高精度
            y = y * (f - (x * y * y));//迭代提高精度
            y *= number;
            if (y < 0)
                y = 0 - y;
            return y;
        }
        /// <summary>
        /// 求平方根的快速算法，实际速度比微软自带的慢很多，算法仅供参考
        /// </summary>
        /// <param name="number"></param>
        /// <returns>平方根</returns>
        unsafe public static double SquareRootDouble(double number)
        {
            long i; double x, y;
            const double f = 1.5F;
            x = number * 0.5F;
            y = number;
            i = *(long*)&y;
            i = 0x5fe6ec85e7de30da - (i >> 1); //注意这一行  
            y = *(double*)&i;
            y = y * (f - x * y * y);
            y = y * (f - x * y * y);//迭代提高精度
            y = y * (f - x * y * y);//迭代提高精度
            y = y * (f - x * y * y);//迭代提高精度
            y *= number;
            if (y < 0)
                y = 0 - y;
            return y;
        }
        /// <summary>
        /// 反正切快速算法， 返回范围为0-360， 精度为0.01
        /// </summary>
        /// <param name="dx">x</param>
        /// <param name="dy">y</param>
        /// <returns>角度</returns>
        public static float atan(float dx, float dy)
        {
            if (dx == 0)
            {
                if (dy < 0)
                    return 180;
                return 0;
            }
            else if (dy == 0)
            {
                if (dx > 0)
                    return 90;
                if (dx == 0)
                    return 0;
                return 270;
            }
            //ax<ay
            float ax = dx < 0 ? -dx : dx, ay = dy < 0 ? -dy : dy;
            float a;
            if (ax < ay) a = ax / ay; else a = ay / ax;
            float s = a * a;
            float r = ((-0.0464964749f * s + 0.15931422f) * s - 0.327622764f) * s * a + a;
            if (ay > ax) r = 1.57079637f - r;
            r *= 5729.5795f;
            if (dx > 0)
            {
                if (dy < 0)
                    r += 9000;
                else r = 9000 - r;
            }
            else
            {
                if (dy < 0)
                    r = 27000 - r;
                else r += 27000;
            }
            r = (int)r;
            r *= 0.01f;
            return r;
        }
        #endregion

        #region collision check
        /// <summary>
        /// 检查点在椭圆里面
        /// </summary>
        /// <param name="ell_location"></param>
        /// <param name="dot"></param>
        /// <param name="xlen"></param>
        /// <param name="ylen"></param>
        /// <returns></returns>
        public static bool DotToEllipse(Vector2 ell_location, Vector2 dot, float xlen, float ylen)
        {
            float x = ell_location.x - dot.x;
            x *= x;
            float y = ell_location.y - dot.y;
            y *= y;
            xlen *= xlen;
            x /= xlen;
            ylen *= ylen;
            y /= ylen;
            return x + y < 1;
        }
        /// <summary>
        ///  检测一个点是否在多边形里面
        /// </summary>
        /// <param name="A">多边形,按顺序连接</param>
        /// <param name="B">点</param>
        /// <returns>在里面返回true，反之返回false</returns>
        public static bool DotToPolygon(Vector2[] A, Vector2 B)
        {
            int count = 0;
            for (int i = 0; i < A.Length; i++)
            {
                Vector2 p1 = A[i];
                Vector2 p2 = i == A.Length - 1 ? A[0] : A[i + 1];
                if (B.y >= p1.y & B.y <= p2.y | B.y >= p2.y & B.y <= p1.y)
                {
                    float t = (B.y - p1.y) / (p2.y - p1.y);
                    float xt = p1.x + t * (p2.x - p1.x);
                    if (B.x == xt)
                        return true;
                    if (B.x < xt)
                        count++;
                }
            }
            return count % 2 > 0 ? true : false;
        }
        /// <summary>
        ///  检测一个点是否在多边形里面
        /// </summary>
        /// <param name="A">多边形,按顺序连接</param>
        /// <param name="B">点</param>
        /// <returns>在里面返回true，反之返回false</returns>
        public static bool DotToPolygon(Vector3[] A, Vector2 B)
        {
            int count = 0;
            for (int i = 0; i < A.Length; i++)
            {
                Vector2 p1 = A[i];
                Vector2 p2 = i == A.Length - 1 ? A[0] : A[i + 1];
                if (B.y >= p1.y & B.y <= p2.y | B.y >= p2.y & B.y <= p1.y)
                {
                    float t = (B.y - p1.y) / (p2.y - p1.y);
                    float xt = p1.x + t * (p2.x - p1.x);
                    if (B.x == xt)
                        return true;
                    if (B.x < xt)
                        count++;
                }
            }
            return count % 2 > 0 ? true : false;
        }
        public static Vector2[] GetPointsOffset(Vector3 location, Vector2[] offsest)
        {
            Vector2[] temp = new Vector2[offsest.Length];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i].x = location.x + offsest[i].x;
                temp[i].y = location.y + offsest[i].y;
            }
            return temp;
        }
        public static bool DotToPolygon(Vector2 origion, Vector2[] A, Vector2 B)//offset
        {
            int count = 0;
            for (int i = 0; i < A.Length; i++)
            {
                Vector2 p1 = A[i];
                p1.x += origion.x;
                p1.y += origion.y;
                Vector2 p2 = i == A.Length - 1 ? A[0] : A[i + 1];
                p2.x += origion.x;
                p2.y += origion.y;
                if (B.y >= p1.y & B.y <= p2.y | B.y >= p2.y & B.y <= p1.y)
                {
                    float t = (B.y - p1.y) / (p2.y - p1.y);
                    float xt = p1.x + t * (p2.x - p1.x);
                    if (B.x == xt)
                        return true;
                    if (B.x < xt)
                        count++;
                }
            }
            return count % 2 > 0 ? true : false;
        }
        //public static bool DotToPolygon(Vector2[] A, Vector2 B)//rotate
        //{
        //    int count = 0;
        //    for (int i = 0; i < A.Length; i++)
        //    {
        //        Vector2 p1 = A[i];
        //        Vector2 p2 = i == A.Length - 1 ? A[0] : A[i + 1];
        //        if (B.y >= p1.y & B.y <= p2.y | B.y >= p2.y & B.y <= p1.y)
        //        {
        //            float t = (B.y - p1.y) / (p2.y - p1.y);
        //            float xt = p1.x + t * (p2.x - p1.x);
        //            if (B.x == xt)
        //                return true;
        //            if (B.x < xt)
        //                count++;
        //        }
        //    }
        //    return count % 2 > 0 ? true : false;
        //}
        public static bool CircleToCircle(Vector2 A, Vector2 B, float radiusA, float radiusB)
        {
            return radiusA + radiusB > Mathf.Sqrt((A.x - B.x) * (A.x - B.x) + (A.y - B.y) * (A.y - B.y));
        }
        public static Vector2 RotatePoint2(ref Vector2 p, ref Vector2 location, float angle)//a=绝对角度 d=直径
        {
            float a = p.x + angle;
            if (a < 0)
                a += 360;
            if (a > 360)
                a -= 360;
            a *= 0.0174533f;//change angle to radin
            float d = p.y;
            Vector2 temp = new Vector2();
            temp.x = location.x - Mathf.Sin(a) * d;
            temp.y = location.y + Mathf.Cos(a) * d;
            return temp;
        }
        public static Vector3 RotateVector3(Vector2 p, ref Vector3 location, float angle)//a=绝对角度 d=直径
        {
            int a = (int)(p.x + angle);
            if (a < 0)
                a += 360;
            if (a > 360)
                a -= 360;
            float d = p.y;
            Vector3 temp = location;
            temp.x = location.x  - Sin(a) * d;
            temp.y = location.y + Cos(a) * d;
            return temp;
        }
        public static void RotatePoint2(ref Vector2 p, ref Vector2 location, float angle, ref Vector3 o)//a=绝对角度 d=直径
        {
            int a = (int)(p.x + angle);
            if (a < 0)
                a += 360;
            if (a > 360)
                a -= 360;
            float d = p.y;
            o.x = location.x - Sin(a) * d;
            o.y = location.y + Cos(a) * d;
        }
        public static Vector2[] RotatePoint2(ref Vector2[] P, Vector2 location, float angle)//p[].x=绝对角度 p[].y=直径
        {
            Vector2[] temp = new Vector2[P.Length];
            for (int i = 0; i < P.Length; i++)
            {
                int a = (int)(P[i].x + angle);//change angle to radin
                if (a < 0)
                    a += 360;
                if (a >= 360)
                    a -= 360;
                temp[i].x = location.x - Sin(a) * P[i].y;
                temp[i].y = location.y + +Cos(a) * P[i].y;
            }
            return temp;
        }
        public static Vector4 RotatePoint(Vector4 P, Vector4 A, float rad, float r, bool isClockwise)//弧度只能表示180°所以用正反转表示
        {
            //点Temp1
            Vector4 Temp1 = new Vector4();
            Temp1.x = P.x - A.x;
            Temp1.y = P.x - A.x;
            //∠T1OX弧度
            float angT1OX = radPOX(Temp1.x, Temp1.y);
            //∠T2OX弧度（T2为T1以O为圆心旋转弧度rad）
            float angT2OX = angT1OX - (isClockwise ? 1 : -1) * rad;
            //点Temp2
            Vector4 Temp2 = new Vector4();
            Temp2.x = r * Mathf.Cos(angT2OX) + A.x;
            Temp2.y = r * Mathf.Sin(angT2OX) + A.y;
            //点Q
            return Temp2;
        }
        public static Vector4[] RotatePoints(ref Vector4[] P, Vector4 origion, float angle)//弧度只能表示180°所以用正反转表示
        {
            if (angle > 180)
                angle += -360;
            angle /= 57.29577951f;
            for (int i = 0; i < P.Length; i++)
            {
                //点Temp1
                Vector4 Temp1 = new Vector4();
                Temp1.x = P[i].x - origion.x;
                Temp1.y = P[i].x - origion.x;
                //∠T1OX弧度
                float angT1OX = radPOX(Temp1.x, Temp1.y);
                //∠T2OX弧度（T2为T1以O为圆心旋转弧度rad）
                float angT2OX = angT1OX - angle;
                //点Temp2

                P[i].x = P[i].z * Mathf.Cos(angT2OX) + origion.x;
                P[i].y = P[i].z * Mathf.Sin(angT2OX) + origion.y;
                //点Q
            }
            return P;
        }
        public static float radPOX(float x, float y)
        {
            //P在(0,0)的情况
            if (x == 0 && y == 0) return 0;

            //P在四个坐标轴上的情况：x正、x负、y正、y负
            if (y == 0 && x > 0) return 0;
            if (y == 0 && x < 0) return Mathf.PI;
            if (x == 0 && y > 0) return Mathf.PI / 2;
            if (x == 0 && y < 0) return Mathf.PI / 2 * 3;

            //点在第一、二、三、四象限时的情况
            if (x > 0 && y > 0) return Mathf.Atan(y / x);
            if (x < 0 && y > 0) return Mathf.PI - Mathf.Atan(y / -x);
            if (x < 0 && y < 0) return Mathf.PI + Mathf.Atan(-y / -x);
            if (x > 0 && y < 0) return Mathf.PI * 2 - Mathf.Atan(-y / x);

            return 0;
        }
        public static bool PToP2(Vector2[] A, Vector2[] B)
        {
            //Cos A=(b²+c²-a²)/2bc
            float min1 = 0, max1 = 0, min2 = 0, max2 = 0;
            int second = 0;
            Vector2 a, b;
            label1:
            for (int i = 0; i < A.Length; i++)
            {
                int id;
                a = A[i];
                if (i == A.Length - 1)
                {
                    b = A[0];
                    id = 1;
                }
                else
                {
                    b = A[i + 1];
                    id = i + 2;
                }
                float x = a.x - b.x;
                float y = a.y - b.y;//向量
                a.x = y;
                a.y = -x;//法线点a
                b.x = -y;
                b.y = x;//法线点b
                        // float ab = (x + x) * (x + x) + (y + y) * (y + y);//b 平方
                        //x = c.x - a.x;
                        //y = c.y - a.y;
                float ac;// = x * x + y * y;//c 平方
                //x = b.x - c.x;
                //y = b.y - c.y;
                float bc;// = x * x + y * y;//a 平方
                //float d = Mathf.Sqrt(bc) + Mathf.Sqrt(ac) - Mathf.Sqrt(ab);
                float d;// = ac - bc;
                //min1 = d;
                //max1 = d;
                for (int o = 0; o < A.Length; o++)
                {
                    float x1 = A[o].x - a.x;
                    x1 *= x1;
                    float y1 = A[o].y - a.y;
                    ac = x1 + y1 * y1;//ac
                    float x2 = b.x - A[o].x;
                    x2 *= x2;
                    float y2 = b.y - A[o].y;
                    bc = x2 + y2 * y2;//bc
                    d = ac - bc;//ab+ac-bc
                    if (o == 0)
                    {
                        min1 = max1 = d;
                    }
                    else
                    {
                        if (d < min1)
                            min1 = d;
                        else if (d > max1)
                            max1 = d;
                    }
                }
                for (int o = 0; o < B.Length; o++)
                {
                    float x1 = B[o].x - a.x;
                    x1 *= x1;
                    float y1 = B[o].y - a.y;
                    ac = x1 + y1 * y1;//ac
                    float x2 = b.x - B[o].x;
                    x2 *= x2;
                    float y2 = b.y - B[o].y;
                    bc = x2 + y2 * y2;//bc
                    d = ac - bc;//ab+ac-bc
                    if (o == 0)
                        max2 = min2 = d;
                    else
                    {
                        if (d < min2)
                            min2 = d;
                        else if (d > max2)
                            max2 = d;
                    }
                }
                if (min2 > max1 | min1 > max2)
                    return false;
            }
            second++;
            if (second < 2)
            {
                Vector2[] temp = A;
                A = B;
                B = temp;
                goto label1;
            }
            return true;
        }
        public static bool PToP2A(Vector2[] A, Vector2[] B, ref Vector3 location)
        {
            //formule
            //A.x+x1*V1.x=B.x+x2*V2.x
            //x2*V2.x=A.x+x1*V1.x-B.x
            //x2=(A.x+x1*V1.x-B.x)/V2.x
            //A.y+x1*V1.y=B.y+x2*V2.y
            //A.y+x1*V1.y=B.y+(A.x+x1*V1.x-B.x)/V2.x*V2.y
            //x1*V1.y=B.y+(A.x-B.x)/V2.x*V2.y-A.y+x1*V1.x/V2.x*V2.y
            //x1*V1.y-x1*V1.x/V2.x*V2.y=B.y+(A.x-B.x)/V2.x*V2.y-A.y
            //x1*(V1.y-V1.x/V2.x*V2.y)=B.y+(A.x-B.x)/V2.x*V2.y-A.y
            //x1=(B.y-A.y+(A.x-B.x)/V2.x*V2.y)/(V1.y-V1.x/V2.x*V2.y)
            //改除为乘防止除0溢出
            //if((V1.y*V2.x-V1.x*V2.y)==0) 平行
            //x1=((B.y-A.y)*V2.x+(A.x-B.x)*V2.y)/(V1.y*V2.x-V1.x*V2.y)
            //x2=(A.x+x1*V1.x-B.x)/V2.x
            //x2=(A.y+x1*V1.y-B.y)/V2.y
            //if(x1>=0&x1<=1 &x2>=0 &x2<=1) cross =true
            //location.x=A.x+x1*V1.x
            //location.y=A.x+x1*V1.y
            Vector2[] VB = new Vector2[B.Length];
            for (int i = 0; i < B.Length; i++)
            {
                if (i == B.Length - 1)
                {
                    VB[i].x = B[0].x - B[i].x;
                    VB[i].y = B[0].y - B[i].y;
                }
                else
                {
                    VB[i].x = B[i + 1].x - B[i].x;
                    VB[i].y = B[i + 1].y - B[i].y;
                }
            }
            for (int i = 0; i < A.Length; i++)
            {
                Vector2 VA = new Vector2();
                if (i == A.Length - 1)
                {
                    VA.x = A[0].x - A[i].x;
                    VA.y = A[0].y - A[i].y;
                }
                else
                {
                    VA.x = A[i + 1].x - A[i].x;
                    VA.y = A[i + 1].y - A[i].y;
                }
                for (int c = 0; c < B.Length; c++)
                {
                    //(V1.y*V2.x-V1.x*V2.y)
                    float y = VA.y * VB[c].x - VA.x * VB[c].y;
                    if (y == 0)
                        break;
                    //((B.y-A.y)*V2.x+(A.x-B.x)*V2.y)
                    float x = (B[c].y - A[i].y) * VB[c].x + (A[i].x - B[c].x) * VB[c].y;
                    float d = x / y;
                    if (d >= 0 & d <= 1)
                    {
                        if (VB[c].x == 0)
                        {
                            //x2=(A.y+x1*V1.y-B.y)/V2.y
                            y = (A[i].y - B[c].y + d * VA.y) / VB[c].y;
                        }
                        else
                        {
                            //x2=(A.x+x1*V1.x-B.x)/V2.x
                            y = (A[i].x - B[c].x + d * VA.x) / VB[c].x;
                        }
                        //location.x=A.x+x1*V1.x
                        //location.y=A.x+x1*V1.y
                        if (y >= 0 & y <= 1)
                        {
                            location.x = A[i].x + d * VA.x;
                            location.y = A[i].y + d * VA.y;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static bool PToP2A(Vector2[] A, Vector2[] B, ref Vector3 la, ref Vector3 lb)
        {
            //formule
            //A.x+x1*V1.x=B.x+x2*V2.x
            //x2*V2.x=A.x+x1*V1.x-B.x
            //x2=(A.x+x1*V1.x-B.x)/V2.x
            //A.y+x1*V1.y=B.y+x2*V2.y
            //A.y+x1*V1.y=B.y+(A.x+x1*V1.x-B.x)/V2.x*V2.y
            //x1*V1.y=B.y+(A.x-B.x)/V2.x*V2.y-A.y+x1*V1.x/V2.x*V2.y
            //x1*V1.y-x1*V1.x/V2.x*V2.y=B.y+(A.x-B.x)/V2.x*V2.y-A.y
            //x1*(V1.y-V1.x/V2.x*V2.y)=B.y+(A.x-B.x)/V2.x*V2.y-A.y
            //x1=(B.y-A.y+(A.x-B.x)/V2.x*V2.y)/(V1.y-V1.x/V2.x*V2.y)
            //改除为乘防止除0溢出
            //if((V1.y*V2.x-V1.x*V2.y)==0) 平行
            //x1=((B.y-A.y)*V2.x+(A.x-B.x)*V2.y)/(V1.y*V2.x-V1.x*V2.y)
            //x2=(A.x+x1*V1.x-B.x)/V2.x
            //x2=(A.y+x1*V1.y-B.y)/V2.y
            //if(x1>=0&x1<=1 &x2>=0 &x2<=1) cross =true
            //location.x=A.x+x1*V1.x
            //location.y=A.x+x1*V1.y
            bool re = false;
            Vector2[] VB = new Vector2[B.Length];
            for (int i = 0; i < B.Length; i++)
            {
                if (i == B.Length - 1)
                {
                    VB[i].x = B[0].x - B[i].x;
                    VB[i].y = B[0].y - B[i].y;
                }
                else
                {
                    VB[i].x = B[i + 1].x - B[i].x;
                    VB[i].y = B[i + 1].y - B[i].y;
                }
            }
            for (int i = 0; i < A.Length; i++)
            {
                Vector2 VA = new Vector2();
                if (i == A.Length - 1)
                {
                    VA.x = A[0].x - A[i].x;
                    VA.y = A[0].y - A[i].y;
                }
                else
                {
                    VA.x = A[i + 1].x - A[i].x;
                    VA.y = A[i + 1].y - A[i].y;
                }
                for (int c = 0; c < B.Length; c++)
                {
                    //(V1.y*V2.x-V1.x*V2.y)
                    float y = VA.y * VB[c].x - VA.x * VB[c].y;
                    if (y == 0)
                        break;
                    //((B.y-A.y)*V2.x+(A.x-B.x)*V2.y)
                    float x = (B[c].y - A[i].y) * VB[c].x + (A[i].x - B[c].x) * VB[c].y;
                    float d = x / y;
                    if (d >= 0 & d <= 1)
                    {
                        if (VB[c].x == 0)
                        {
                            //x2=(A.y+x1*V1.y-B.y)/V2.y
                            y = (A[i].y - B[c].y + d * VA.y) / VB[c].y;
                        }
                        else
                        {
                            //x2=(A.x+x1*V1.x-B.x)/V2.x
                            y = (A[i].x - B[c].x + d * VA.x) / VB[c].x;
                        }
                        //location.x=A.x+x1*V1.x
                        //location.y=A.x+x1*V1.y
                        if (y >= 0 & y <= 1)
                        {
                            if (re)
                            {
                                lb.x = A[i].x + d * VA.x;
                                lb.y = A[i].y + d * VA.y;
                                return true;
                            }
                            else
                            {
                                la.x = A[i].x + d * VA.x;
                                la.y = A[i].y + d * VA.y;
                                re = true;
                            }
                        }
                    }
                }
            }
            return re;
        }
        public static bool PToP3(Vector3[] A, Vector3[] B)
        {
            //Cos A=(b²+c²-a²)/2bc
            float min1 = 0, max1 = 0, min2 = 0, max2 = 0;
            int second = 0;
            Vector3 a, b;
            label1:
            for (int i = 0; i < A.Length; i++)
            {
                int id;
                a = A[i];
                if (i == A.Length - 1)
                {
                    b = A[0];
                    id = 1;
                }
                else
                {
                    b = A[i + 1];
                    id = i + 2;
                }
                float x = a.x - b.x;
                float y = a.y - b.y;//向量
                a.x = y;
                a.y = -x;//法线点a
                b.x = -y;
                b.y = x;//法线点b
                        // float ab = (x + x) * (x + x) + (y + y) * (y + y);//b 平方
                        //x = c.x - a.x;
                        //y = c.y - a.y;
                float ac;// = x * x + y * y;//c 平方
                //x = b.x - c.x;
                //y = b.y - c.y;
                float bc;// = x * x + y * y;//a 平方
                //float d = Mathf.Sqrt(bc) + Mathf.Sqrt(ac) - Mathf.Sqrt(ab);
                float d;// = ac - bc;
                //min1 = d;
                //max1 = d;
                for (int o = 0; o < A.Length; o++)
                {
                    float x1 = A[o].x - a.x;
                    x1 *= x1;
                    float y1 = A[o].y - a.y;
                    ac = x1 + y1 * y1;//ac
                    float x2 = b.x - A[o].x;
                    x2 *= x2;
                    float y2 = b.y - A[o].y;
                    bc = x2 + y2 * y2;//bc
                    d = ac - bc;//ab+ac-bc
                    if (o == 0)
                    {
                        min1 = max1 = d;
                    }
                    else
                    {
                        if (d < min1)
                            min1 = d;
                        else if (d > max1)
                            max1 = d;
                    }
                }
                for (int o = 0; o < B.Length; o++)
                {
                    float x1 = B[o].x - a.x;
                    x1 *= x1;
                    float y1 = B[o].y - a.y;
                    ac = x1 + y1 * y1;//ac
                    float x2 = b.x - B[o].x;
                    x2 *= x2;
                    float y2 = b.y - B[o].y;
                    bc = x2 + y2 * y2;//bc
                    d = ac - bc;//ab+ac-bc
                    if (o == 0)
                        max2 = min2 = d;
                    else
                    {
                        if (d < min2)
                            min2 = d;
                        else if (d > max2)
                            max2 = d;
                    }
                }
                if (min2 > max1 | min1 > max2)
                    return false;
            }
            second++;
            if (second < 2)
            {
                Vector3[] temp = A;
                A = B;
                B = temp;
                goto label1;
            }
            return true;
        }
        public static bool TriangleToPolygon(Vector2[] A, Vector2[] B)
        {
            Vector2[] a = new Vector2[3]
            {
            new Vector2(A[1].x - A[0].x, A[1].y - A[0].y),
            new Vector2(A[2].x - A[1].x, A[2].y - A[1].y),
            new Vector2(A[0].x - A[2].x, A[0].y - A[2].y)
            };
            int again = 0;
            label1:
            for (int i = 0; i < a.Length; i++)
            {
                float min1 = 1000, min2 = 1000, max1 = 0, max2 = 0;
                float sxy = a[i].x * a[i].x + a[i].y * a[i].y;
                for (int l = 0; l < 3; l++)
                {
                    float dxy = A[l].x * a[i].x + A[l].y * a[i].y;
                    float x = dxy / sxy * a[i].x;
                    if (x < 0)
                        x = 0 - x;
                    float y = dxy / sxy * a[i].y;
                    if (y < 0)
                        y = 0 - y;
                    x = x + y;
                    if (x > max1)
                        max1 = x;
                    if (x < min1)
                        min1 = x;
                }
                for (int l = 0; l < B.Length; l++)
                {
                    float dxy = B[l].x * a[i].x + B[l].y * a[i].y;
                    float x = dxy / sxy * a[i].x;
                    if (x < 0)
                        x = 0 - x;
                    float y = dxy / sxy * a[i].y;
                    if (y < 0)
                        y = 0 - y;
                    x = x + y;
                    if (x > max2)
                        max2 = x;
                    if (x < min2)
                        min2 = x;
                }
                if (min1 > max2 | min2 > max1)
                {
                    return false;
                }
            }
            if (again > 0)
                return true;
            a = new Vector2[B.Length];
            for (int i = 0; i < B.Length - 1; i++)
            {
                a[i].x = B[i + 1].x - B[i].x;
                a[i].y = B[i + 1].y - B[i].y;
            }
            a[a.Length - 1].x = B[0].x - B[a.Length - 1].x;
            a[a.Length - 1].y = B[0].y - B[a.Length - 1].y;
            again++;
            goto label1;
        }
        public static bool CircleToPolygon(Vector2 C, float r, Vector2[] P)
        {
            Vector2 A = new Vector2();
            Vector2 B = new Vector2();
            float z = 10, r2 = r * r, x = 0, y = 0;
            float[] d = new float[P.Length];
            int id = 0;
            for (int i = 0; i < P.Length; i++)
            {
                x = C.x - P[i].x;
                y = C.y - P[i].y;
                x = x * x + y * y;
                if (x <= r2)
                    return true;
                d[i] = x;
                if (x < z)
                {
                    z = x;
                    id = i;
                }
            }
            int p1 = id - 1;
            if (p1 < 0)
                p1 = P.Length - 1;
            float a, b, c;
            c = d[p1];
            a = d[id];
            B = P[id];
            A = P[p1];
            x = B.x - A.x;
            x *= x;
            y = B.y - A.y;
            y *= y;
            b = x + y;
            x = c - a;
            if (x < 0)
                x = -x;
            if (x <= b)
            {
                y = b + c - a;
                y = y * y / 4 / b;
                if (c - y <= r2)
                    return true;
            }
            else
            {
                p1 = id + 1;
                if (p1 == P.Length)
                    p1 = 0;
                c = d[p1];
                A = P[p1];
                x = B.x - A.x;
                x *= x;
                y = B.y - A.y;
                y *= y;
                b = x + y;
                x = c - a;
                if (x < 0)
                    x = -x;
                if (x <= b)
                {
                    y = b + c - a;
                    y = y * y / 4 / b;
                    if (c - y <= r2)
                        return true;
                }
            }
            return DotToPolygon(P, new Vector2(C.x, C.y));//circle inside polygon
        }
        public static bool CircleToLine(Vector2 C, float r, Vector2 A, Vector2 B)
        {
            float vx1 = C.x - A.x;
            float vy1 = C.y - A.y;
            float vx2 = B.x - A.x;
            float vy2 = B.y - A.y;
            float len = Mathf.Sqrt(vx2 * vx2 + vy2 * vy2);
            vx2 /= len;
            vy2 /= len;
            float u = vx1 * vx2 + vy1 * vy2;
            float x0 = 0f;
            float y0 = 0f;
            if (u <= 0)
            {
                x0 = A.x;
                y0 = A.y;
            }
            else if (u >= len)
            {
                x0 = B.x;
                y0 = B.y;
            }
            else
            {
                x0 = A.x + vx2 * u;
                y0 = A.y + vy2 * u;
            }
            return (C.x - x0) * (C.x - x0) + (C.y - y0) * (C.y - y0) <= r * r;
        }
        public static bool CircleToLineA(Vector2 C, float r, Vector2 A, Vector2 B)
        {
            r *= r;
            float x = C.x - B.x;
            x *= x;
            float y = C.y - B.y;
            y *= y;
            float a = x + y;
            if (a <= r)
                return true;
            x = A.x - C.x;
            x *= x;
            y = A.y - C.y;
            y *= y;
            float c = x + y;
            if (c <= r)
                return true;
            x = B.x - A.x;
            x *= x;
            y = B.y - A.y;
            y *= y;
            float b = x + y;
            x = c - a;
            if (x < 0)
                x = -x;
            if (x > b)
                return false;
            y = b + c - a;
            y *= y / 4 / b;
            if (c - y <= r)
                return true;
            return false;
        }
        #endregion

        /// <summary>
        /// 求三个点夹角的中间点
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <returns></returns>
        public static Vector3 AngleCenter(Vector3 A, Vector3 B, Vector3 C)
        {
            float ab = Vector3.Distance(A, B);
            float cb = Vector3.Distance(C, B);
            float r = ab / cb;
            Vector3 s = B + (C - B) * r;
            Vector3 o = A + (s - A) * 0.5f;
            return o;
        }
        /// <summary>
        /// 抛物线解析式，返回一般表达式 y=a*x*x+b*x+c的 a,b,c的值
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <returns></returns>
        //public static Vector3 Parabola(Vector2 A, Vector2 B, Vector2 C)
        //{
        //    //一般表达式 y=a*x*x+b*x+c;
        //    //带入公式
        //    //float a = 0, b = 0, c = 0;
        //    //三元表达式
        //    //B.y - A.y = a * (B.x * B.x - A.x * A.x) + b * (B.x - A.x) ;
        //    //C.y - B.y = a * (C.x * C.x - B.x * B.x) + b * (C.x - B.x) ;
        //    //(B.y - A.y) *(C.x - B.x) / (B.x - A.x) = a * (B.x * B.x - A.x * A.x) * (C.x - B.x) /( B.x - A.x)+ b * (C.x - B.x) ;
        //    //C.y - B.y - (B.y - A.y) *(C.x - B.x) / (B.x - A.x) = a * (C.x * C.x - B.x * B.x) - a * (B.x * B.x - A.x * A.x) * (C.x - B.x) /( B.x - A.x);
        //    //C.y - B.y - (B.y - A.y) *(C.x - B.x) / (B.x - A.x) = a *( (C.x * C.x - B.x * B.x) -  (B.x * B.x - A.x * A.x) * (C.x - B.x) /( B.x - A.x));
        //    //a =(C.y - B.y - (B.y - A.y) *(C.x - B.x) / (B.x - A.x) )/( (C.x * C.x - B.x * B.x) -  (B.x * B.x - A.x * A.x) * (C.x - B.x) /( B.x - A.x));
        //    float ax = A.x;
        //    float asq = ax * ax;
        //    float bx = B.x;
        //    float bsq = bx * bx;
        //    float baq = bsq - asq;//(B.x * B.x - A.x * A.x) 
        //    float bax = bx - ax;
        //    float bay = B.y - A.y;
        //    //b = (B.y - A.y - a * (B.x * B.x - A.x * A.x) ) / (B.x - A.x)
        //    //b = (bay - a * baq) / bax;
        //    //b = bay / bax - a * baq / bax;
        //    float bar = bay / bax;
        //    float baqx = baq / bax;
        //    //b = bar - a * baqx;
        //    //C.y - B.y = a * (C.x * C.x - B.x * B.x) + b * (C.x - B.x) ;
        //    float cx = C.x;
        //    float csq = cx * cx;
        //    float cbq = csq - bsq;//(C.x * C.x - B.x * B.x) 
        //    float cbx = cx - bx;
        //    float cby = C.y - B.y;
        //    //cby = a * cbq + b * cbx;
        //    //cby = a * cbq + (bar - a * baqx) * cbx;
        //    //cby = a * cbq +bar * cbx - a * baqx * cbx;
        //    //cby - bar * cbx =a * cbq - a * baqx * cbx;
        //    //cby - bar * cbx =a * (cbq - baqx * cbx);
        //    //float a = (cby - bar * cbx ) / (cbq - baqx * cbx);//精度有丢失
        //    float a = (C.y - B.y - (B.y - A.y) * (C.x - B.x) / (B.x - A.x)) / ((C.x * C.x - B.x * B.x) - (B.x * B.x - A.x * A.x) * (C.x - B.x) / (B.x - A.x));//精度好
        //    //float a = (cby - bar * cbx) / (cbq - baqx * cbx);//简化后精度有丢失
        //    if (a == 0)//直线
        //        return Vector3.zero;
        //    float b = bar - a * baqx;
        //    float c = A.y - a * asq - b * A.x;
        //    return new Vector3(a, b, c);
        //}
        /// <summary>
        /// 抛物线解析式，返回一般表达式 y=a*x*x+b*x+c的 a,b,c的值
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <returns></returns>
        public static Vector3 Parabola(Vector2 A, Vector2 B, Vector2 C)
        {
            float a = 0, b = 0, c = 0;
            float x1 = A.x, y1 = A.y, x2 = B.x, x3 = C.x, y2 = B.y, y3 = C.y;
            float m;
            m = x1 * x1 * x2 + x2 * x2 * x3 + x1 * x3 * x3 - x3 * x3 * x2 - x2 * x2 * x1 - x1 * x1 * x3;

            if ((m + 1) == 1)
            {
            }
            else
            {
                a = (y1 * x2 + y2 * x3 + y3 * x1 - y3 * x2 - y2 * x1 - y1 * x3) / m;
                b = (x1 * x1 * y2 + x2 * x2 * y3 + x3 * x3 * y1 - x3 * x3 * y2 - x2 * x2 * y1 - x1 * x1 * y3) / m;
                c = (x1 * x1 * x2 * y3 + x2 * x2 * x3 * y1 + x3 * x3 * x1 * y2 - x3 * x3 * x2 * y1 - x2 * x2 * x1 * y3 - x1 * x1 * x3 * y2) / m;
            }
            return new Vector3(a, b, c);
        }
        /// <summary>
        /// 一阶贝塞尔曲线
        /// </summary>
        /// <param name="t">比率</param>
        /// <param name="p0">起点</param>
        /// <param name="p1">中间点</param>
        /// <param name="p2">结束点</param>
        /// <returns></returns>
        public static Vector3 BezierPoint(float t, ref Vector3 p0, ref Vector3 p1, ref Vector3 p2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 p = uu * p0;
            p += 2 * u * t * p1;
            p += tt * p2;
            return p;
        }
        /// <summary>
        /// 二阶贝塞尔曲线
        /// </summary>
        /// <param name="t">比率</param>
        /// <param name="p0">起点</param>
        /// <param name="p1">中间点1</param>
        /// <param name="p2">中间点2</param>
        /// <param name="p3">结束点</param>
        /// <returns></returns>
        public static Vector3 BezierPoint(float t, ref Vector3 p0, ref Vector3 p1, ref Vector3 p2, ref Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0; //first term
            p += 3 * uu * t * p1; //second term
            p += 3 * u * tt * p2; //third term
            p += ttt * p3; //fourth term

            return p;
        }
        /// <summary>
        /// 欧拉角转四元数
        /// </summary>
        /// <param name="euler"></param>
        /// <returns></returns>
        public static Vector4 EulerToQuaternion(Vector3 euler)
        {
            float x = euler.x * 0.5f;
            float y = euler.y * 0.5f;
            float z = euler.z * 0.5f;
            float sx = Sin(x);
            float sy = Sin(y);
            float sz = Sin(z);
            float cx = Cos(x);
            float cy = Cos(y);
            float cz = Cos(z);
            Vector4 q = Vector4.zero;
            q.w = sx * sy * sz + cx * cy * cz;
            q.x = sx * cy * cz + cx * sy * sz;
            q.y = cx * sy * cz - sx * cy * sz;
            q.z = cx * cy * sz - sx * sy * cz;
            return q;
        }
        /// <summary>
        /// 四元数相乘，代码来源xenko
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Vector4 MultiplyQuaternion(Vector4 left, Vector4 right)
        {
            float lx = left.x;
            float ly = left.y;
            float lz = left.z;
            float lw = left.w;
            float rx = right.x;
            float ry = right.y;
            float rz = right.z;
            float rw = right.w;
            var result = Vector4.zero; ;
            result.x = (rx * lw + lx * rw + ry * lz) - (rz * ly);
            result.y = (ry * lw + ly * rw + rz * lx) - (rx * lz);
            result.z = (rz * lw + lz * rw + rx * ly) - (ry * lx);
            result.w = (rw * lw) - (rx * lx + ry * ly + rz * lz);
            return result;
        }
        /// <summary>
        /// 旋转顶点，代码来源xenko
        /// </summary>
        /// <param name="q"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 QuaternionMultiplyVector(Vector4 q,Vector3 v)
        {
            var t = Vector4.zero;
            t.x = v.x;
            t.y = v.y;
            t.z = v.z;
            var cc = Vector4.zero;
            cc.x = -q.x;
            cc.y = -q.y;
            cc.z = -q.z;
            cc.w = q.w;
            return MultiplyQuaternion(MultiplyQuaternion(cc, t),q);
        }
        /// <summary>
        /// 旋转网格所有顶点
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="quat"></param>
        public static void RotationVertex(Vector3[] vertex, Vector4 quat)
        {
            var cc = Vector4.zero;
            cc.x = -quat.x;
            cc.y = -quat.y;
            cc.z = -quat.z;
            cc.w = quat.w;
            for (int i=0;i<vertex.Length;i++)
                vertex[i] = MultiplyQuaternion(MultiplyQuaternion(cc,vertex[i]),quat);
        }
        /// <summary>
        /// 旋转网格所有顶点
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="angle"></param>
        public static void RotationVertex(Vector3[] vertex,Vector3 angle)
        {
            RotationVertex(vertex,EulerToQuaternion(angle));
        }
        static Vector3[] CutTriangle(Vector3[] vertex,Vector2[] uv,Vector2[] mask)
        {
            bool a = DotToPolygon(mask,vertex[0]);
            bool b = DotToPolygon(mask,vertex[1]);
            bool c = DotToPolygon(mask,vertex[2]);
            if (a & b & c)
                return vertex;
            if(!a & !b & !c)
                return null;
            for(int i=0;i<mask.Length;i++)
            {
                if(DotToPolygon(vertex,mask[i]))
                {
                    
                }
            }
            return null;
        }
        /// <summary>
        /// 计算三角形法线
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Vector3 GetTriangleNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            var ab = Vector3.zero;
            ab.x = a.x - b.x;
            ab.y = a.y - b.y;
            ab.z = a.z - b.z;
            var bc = Vector3.zero;
            bc.x = b.x - c.x;
            bc.y = b.y - c.y;
            bc.z = b.z - c.z;
            //然后计算法线，即另一个向量。求该对象的法向量（norm）。下面的代码用于计算向量ab和bc的外积：
            var nor = Vector3.zero;
            nor.x = (ab.y * bc.z) - (ab.z * bc.y);
            nor.y = -((ab.x * bc.z) - (ab.z * bc.x));
            nor.z = (ab.x * bc.y) - (ab.y * bc.x);
            return nor;
        }
        /// <summary>
        /// 解一元二次方程
        /// </summary>
        /// <param name="a">ax²</param>
        /// <param name="b">bx</param>
        /// <param name="c"></param>
        /// <returns>数组0为实数1为虚数</returns>
        public static double[] SolutionTowEquation(double a, double b, double c)
        {
            double delt = b * b - 4 * a * c;
            double[] v =new double[2];
            if (delt >= 0)
            {
                if (a > 1e-10)
                {
                    v[0] = (float)((-b + Math.Sqrt(delt)) / (2 * a));
                    v[1] = (float)((-b - Math.Sqrt(delt)) / (2 * a));

                }
                else
                {
                    v[0] = (float)((2 * c) / (-b + Math.Sqrt(delt)));
                    v[1] = (float)((2 * c) / (-b - Math.Sqrt(delt)));
                }
            }
            return v;
        }
        /// <summary>
        /// 解一元三次方式，盛金公式法
        /// </summary>
        /// <param name="_a">ax³</param>
        /// <param name="_b">bx²</param>
        /// <param name="_c">cx</param>
        /// <param name="_d"></param>
        /// <returns></returns>
        public static Complex[] ThreeEquationShengjin(double _a, double _b = 0, double _c = 0, double _d = 0)
        {
            Shengjin _Shengjin = new Shengjin(_a, _b, _c, _d);
            return _Shengjin.calc();
        }
        class Shengjin
        {
            double a;
            double b;
            double c;
            double d;

            public Shengjin(double _a, double _b = 0, double _c = 0, double _d = 0)
            {
                Debug.Assert(_a != 0, "三次系数，不能为0");
                a = _a;
                b = _b;
                c = _c;
                d = _d;
            }

            public Complex[] calc()
            {
                Complex[] x = new Complex[3];

                double A = b * b - 3 * a * c;
                double B = b * c - 9 * a * d;
                double C = c * c - 3 * b * d;

                double sj = B * B - 4 * A * C;

                if (A == 0 && B == 0)
                {
                    //盛金公式1
                    x[0] = x[1] = x[2] = new Complex(-b / (3 * a));
                }
                else if (sj > 0)
                {
                    //盛金公式2
                    double Y1 = A * b + 1.5 * a * (-B + Math.Pow(sj, 0.5));
                    double Y2 = A * b + 1.5 * a * (-B - Math.Pow(sj, 0.5));
                    //Y1立方根+Y2立方根
                    double t1 = Y1 > 0 ? Math.Pow(Y1, 1.0 / 3) : -Math.Pow(-Y1, 1.0 / 3);
                    double t2 = Y2 > 0 ? Math.Pow(Y2, 1.0 / 3) : -Math.Pow(-Y2, 1.0 / 3);
                    double Y12 = t1 + t2;
                    //Y1立方根-Y2立方根
                    double _Y12 = t1 - t2;

                    x[0] = new Complex((-b - Y12) / (3 * a));
                    x[1] = new Complex((-b + 0.5 * Y12) / (3 * a), Math.Pow(3, 0.5) * _Y12 / (6 * a));
                    x[2] = new Complex((-b + 0.5 * Y12) / (3 * a), -Math.Pow(3, 0.5) * _Y12 / (6 * a));
                }
                else if (sj == 0)
                {
                    //盛金公式3
                    double K = B / A;
                    x[0] = new Complex(-b / K);
                    x[1] = x[2] = new Complex(-K / 2);
                }
                else
                {
                    //盛金公式4,sj<0
                    double T = (2 * A * b - 3 * a * B) / (2 * Math.Pow(A, 1.5));
                    double o = Math.Acos(T);
                    x[0] = new Complex((-b - 2 * Math.Pow(A, 0.5) * Math.Cos(o / 3.0)) / (3 * a));
                    x[1] = new Complex((-b + Math.Pow(A, 0.5) * (Math.Cos(o / 3.0) + Math.Sqrt(3) * Math.Sin(o / 3.0))) / (3 * a));
                    x[2] = new Complex((-b + Math.Pow(A, 0.5) * (Math.Cos(o / 3.0) - Math.Sqrt(3) * Math.Sin(o / 3.0))) / (3 * a));
                }

                return x;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="r">AttenuationRate</param>
        /// <param name="v">Velocity</param>
        /// <param name="t">time</param>
        /// <returns>最大速率行驶到当前时间产生的距离</returns>
        public static double PowDistance(double r, double v, int t)
        {
            //S= [a^(n+1) -a]/（a-1)
            return v * (Math.Pow(r, t + 1) - r) / (r - 1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="r">AttenuationRate</param>
        /// <param name="d">Distance</param>
        /// <returns>行驶到指定位置所需的最大速率</returns>
        public static double DistanceToVelocity(double r, double d)
        {
            return d * (r - 1) / (Math.Pow(r, 1000000) - r);
        }
        public static void Cross(ref Vector3 left, ref Vector3 right, ref Vector3 result)
        {
            result.x = (left.y * right.z) - (left.z * right.y);
            result.y = (left.z * right.x) - (left.x * right.z);
            result.z = (left.x * right.y) - (left.y * right.x);
        }
        [ThreadStatic]
        static Vector3 E1;
        [ThreadStatic]
        static Vector3 E2;
        [ThreadStatic]
        static Vector3 P;
        [ThreadStatic]
        static Vector3 T;
        [ThreadStatic]
        static Vector3 Q;
        public static bool IntersectTriangle(ref Vector3 orig, ref Vector3 dir,
     ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, ref Vector3 result)
        {
            // E1
            E1.x = v1.x - v0.x;
            E1.y = v1.y - v0.y;
            E1.z = v1.z - v0.z;
            //E1 = v1 - v0;
            // E2
            E2.x = v2.x - v0.x;
            E2.y = v2.y - v0.y;
            E2.z = v2.z - v0.z;
            //E2 = v2 - v0;
            // P
            Cross(ref dir, ref E2, ref P);
            // determinant
            float det = Vector3.Dot(E1, P);
            // keep det > 0, modify T accordingly
            if (det > 0)
            {
                T = orig - v0;
            }
            else
            {
                T = v0 - orig;
                det = -det;
            }
            // If determinant is near zero, ray lies in plane of triangle
            if (det < 0.0001f)
                return false;
            // Calculate u and make sure u <= 1
            float u = Vector3.Dot(T, P);
            if (u < 0.0f || u > det)
                return false;
            // Q
            Cross(ref T, ref E1, ref Q);
            // Calculate v and make sure u + v <= 1
            float v = Vector3.Dot(dir, Q);
            if (v < 0.0f || u + v > det)
                return false;
            // Calculate t, scale parameters, ray intersects triangle
            float t = Vector3.Dot(E2, Q);
            float fInvDet = 1.0f / det;
            result.x = t * fInvDet;
            result.y = u * fInvDet;
            result.z = v * fInvDet;
            return true;
        }

        /// <summary>
        /// 两个向量的投影
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="onNormal"></param>
        /// <returns></returns>
        public static Vector3 Project(Vector3 vector, Vector3 onNormal)
        {
            float num = Vector3.Dot(onNormal, onNormal);
            if (num < 1.17549435E-38f)
            {
                return Vector3.zero;
            }
            return onNormal * Vector3.Dot(vector, onNormal) / num;
        }
        /// <summary>
        /// 两个向量的投影
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="onNormal"></param>
        /// <returns></returns>
        public static Vector2 Project(Vector2 vector, Vector2 onNormal)
        {
            float num = Vector2.Dot(onNormal, onNormal);
            if (num < 1.17549435E-38f)
            {
                return Vector2.zero;
            }
            return onNormal * Vector2.Dot(vector, onNormal) / num;
        }
    }
    /// <summary>
    /// 表示一个复数
    /// </summary>
    public class Complex
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Complex() : this(0, 0) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="real">实部</param>
        public Complex(double real) : this(real, 0) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="real">实部</param>
        /// <param name="image">虚部</param>
        public Complex(double real, double image)
        {
            this.real = real;
            this.image = image;
        }

        private double real;

        /// <summary>
        /// 实部
        /// </summary>
        public double Real
        {
            get { return real; }
            set { real = value; }
        }

        private double image;

        /// <summary>
        /// 虚部
        /// </summary>
        public double Image
        {
            get { return image; }
            set { image = value; }
        }

        /// <summary>
        /// 复数的加法运算
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static Complex operator +(Complex c1, Complex c2)
        {
            return new Complex(c1.real + c2.real, c1.image + c2.image);
        }

        /// <summary>
        /// 复数的减法运算
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static Complex operator -(Complex c1, Complex c2)
        {
            return new Complex(c1.real - c2.real, c1.image - c2.image);
        }

        /// <summary>
        /// 复数的乘法运算
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static Complex operator *(Complex c1, Complex c2)
        {
            return new Complex(c1.real * c2.real - c1.image * c2.image, c1.image * c2.real + c1.real * c2.image);
        }

        /// <summary>
        /// 复数的求模运算
        /// </summary>
        /// <returns></returns>
        public double ToModul()
        {
            return Math.Sqrt(real * real + image * image);
        }

        public override string ToString()
        {
            if (Real == 0 && Image == 0)
            {
                return string.Format("{0}", 0);
            }

            if (Real == 0 && (Image != 1 && Image != -1))
            {
                return string.Format("{0} i", Image);
            }

            if (Image == 0)
            {
                return string.Format("{0}", Real);
            }

            if (Image == 1)
            {
                return string.Format("i");
            }

            if (Image == -1)
            {
                return string.Format("- i");
            }

            if (Image < 0)
            {
                return string.Format("{0} - {1} i", Real, -Image);
            }
            return string.Format("{0} + {1} i", Real, Image);
        }
    }
}