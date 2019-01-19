using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang
{
    /// <summary>
    /// 线性变化值，参数范围为0-1
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="ratio"></param>
    /// <returns></returns>
    public delegate float LinearTransformation(AnimatBase sender, float ratio);
    /// <summary>
    /// 动画管理类，将所有动画添加至此类，进行统一更新
    /// </summary>
    public class AnimationManage
    {
        static AnimationManage am;
        static int Frames =0;
        /// <summary>
        /// 返回此类的唯一实例
        /// </summary>
        public static AnimationManage Manage { get { if (am == null) am = new AnimationManage(); return am; } }
        //public void Add
        List<AnimatInterface> Actions;
        /// <summary>
        /// 获取当前对象池中对象的数量
        /// </summary>
        public int Count { get { return Actions.Count; } }
        AnimationManage()
        {
            Actions = new List<AnimatInterface>();
            droc = new List<AnimatInterface>();
            key_events = new List<ToDoEvent>();
            time_events = new List<ToDoEvent>();
            frame_events = new List<ToDoEvent>();
        }
        /// <summary>
        /// 主更新函数，更新所有动画
        /// </summary>
        public void Update()
        {
            Frames++;
            float timeslice = Time.deltaTime * 1000;
            for (int i = 0; i < Actions.Count; i++)
            {
                if (Actions[i] != null)
                    Actions[i].Update(timeslice);
            }
            DoEvent(timeslice);
            DoFrameEvent();
            LeanTween.update();
        }
        /// <summary>
        /// 创建一个位置和旋转的基础动画
        /// </summary>
        /// <param name="target">Transform</param>
        /// <returns></returns>
        public Animat CreateAnimat(Transform target)
        {
            var ani = new Animat(target);
            Actions.Add(ani);
            return ani;
        }
        /// <summary>
        /// 添加一个新动画，重复添加会造成多倍运行
        /// </summary>
        /// <param name="ani">动画接口</param>
        public void AddAnimat(AnimatInterface ani)
        {
            if (Actions.Contains(ani))
                return;
            Actions.Add(ani);
        }
        /// <summary>
        /// 删除动画
        /// </summary>
        /// <param name="ani">动画接口</param>
        public void ReleaseAnimat(AnimatInterface ani)
        {
            Actions.Remove(ani);
            droc.Remove(ani);
        }
        /// <summary>
        /// 释放所有动画
        /// </summary>
        public void ReleaseAll()
        {
            Actions.Clear();
            Actions.AddRange(droc);
        }
        List<ToDoEvent> key_events;
        List<ToDoEvent> time_events;
        List<ToDoEvent> frame_events;
        void DoEvent(float time)
        {
            if (time_events == null)
                return;
            int i = time_events.Count - 1;
            for (; i >= 0; i--)
            {
                var t = time_events[i];
                if (t != null)
                {
                    t.time -= time;
                    if (t.time <= 0)
                    {
                        if (t.DoEvent != null)
                            t.DoEvent(t.parameter);
                        time_events.RemoveAt(i);
                    }
                }
                else time_events.RemoveAt(i);
            }
        }
        void DoFrameEvent()
        {
            if (frame_events == null)
                return;
            int i = frame_events.Count - 1;
            for (; i >= 0; i--)
            {
                var t = frame_events[i];
                if (t != null)
                {
                    t.level --;
                    if (t.level <= 0)
                    {
                        if (t.DoEvent != null)
                            t.DoEvent(t.parameter);
                        frame_events.RemoveAt(i);
                    }
                }
                else frame_events.RemoveAt(i);
            }
        }
        public void ToDo(string key, Action<object> action, object parameter, int level = 0)
        {
            ToDoEvent to = new ToDoEvent();
            to.key = key;
            to.DoEvent = action;
            to.parameter = parameter;
            to.level = level;
            for (int i = 0; i < key_events.Count; i++)
                if (key_events[i].key == key)
                {
                    if (to.level > key_events[i].level)
                        key_events[i] = to;
                    return;
                }
            key_events.Add(to);
        }
        /// <summary>
        /// 委托某个事件在多少毫秒后执行
        /// </summary>
        /// <param name="time"></param>
        /// <param name="action"></param>
        /// <param name="parameter"></param>
        public void ToDo(float time, Action<object> action, object parameter)
        {
            for (int i = 0; i < time_events.Count; i++)
            {
                if (action == time_events[i].DoEvent)
                {
                    time_events[i].time = time;
                    return;
                }
            }
            ToDoEvent to = new ToDoEvent();
            to.time = time;
            to.DoEvent = action;
            to.parameter = parameter;
            time_events.Add(to);
        }
        public void FrameToDo(int frames, Action<object> action, object parameter)
        {
            ToDoEvent to = new ToDoEvent();
            to.level = frames;
            to.DoEvent = action;
            to.parameter = parameter;
            frame_events.Add(to);
        }
        /// <summary>
        /// 执行某个事件
        /// </summary>
        /// <param name="key"></param>
        public bool DoEvent(string key)
        {
            if (key_events == null)
                return false;
            for (int i = 0; i < key_events.Count; i++)
                if (key == key_events[i].key)
                {
                    if (key_events[i].DoEvent != null)
                        key_events[i].DoEvent(key_events[i].parameter);
                    key_events.RemoveAt(i);
                    return true;
                }
            return false;
        }
        /// <summary>
        /// 清除所有事件
        /// </summary>
        public void ClearEvent()
        {
            key_events.Clear();
            time_events.Clear();
            frame_events.Clear();
        }
        List<AnimatInterface> droc;
        public void DontReleaseOnClear(AnimatInterface animat)
        {
            droc.Add(animat);
        }
        public T FindAni<T>(Func<T, bool> equl) where T : class, AnimatInterface
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                var ani = Actions[i];
                if (ani is T)
                {
                    if (equl != null)
                    {
                        if (equl(ani as T))
                            return ani as T;
                    }
                    else return ani as T;
                }
            }
            return null;
        }
    }
    /// <summary>
    /// 动画类型，用于基本动画
    /// </summary>
    public enum PlayStyle
    {
        None = 0, Position = 1, Angle = 2
    }
    /// <summary>
    /// 动画接口
    /// </summary>
    public interface AnimatInterface
    {
        void Update(float time);
    }
    /// <summary>
    /// 基本动画
    /// </summary>
    public class AnimatBase
    {
        public object DataContext { get; set; }
        protected float m_time;
        public float Delay { get; set; }
        public float Time { get { return m_time; } set {  m_time = value; } }
        public bool Loop;
        protected float c_time;
        protected bool playing = false;
        public virtual void Play()
        {
            playing = true;
            c_time = 0;
        }
        public virtual void Pause()
        {
            playing = false;
        }
        /// <summary>
        /// 动画运动线
        /// </summary>
        public LinearTransformation Linear;
        /// <summary>
        /// 用于缓存数据
        /// </summary>
        public float[] DataCache;
        public Vector3[] points;

    }
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
        public static Animat MoveTo(Transform trans, Vector3 start, Vector3 pos, float time)
        {
            var ani = new Animat(trans);
            AnimationManage.Manage.AddAnimat(ani);
            ani.StartPosition = start;
            ani.EndPosition = pos;
            ani.playstyle = PlayStyle.Position;
            ani.m_time = time;
            ani.playing = true;
            ani.c_time = 0;
            ani.ReleaseOnOver = true;
            return ani;
        }
        public static Animat MoveTo(Transform trans, Vector3 pos,float time)
        {
            return MoveTo(trans,trans.localPosition,pos,time);
        }
        public Animat(Transform t)
        {
            Target = t;
        }
        public Transform Target;
        public PlayStyle playstyle;
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public Vector3 StartAngle;
        public Vector3 EndAngle;
        public Action<Animat> OnPlayStart;
        public Action<Animat> OnPlayOver;
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
                        if (OnPlayStart != null)
                            OnPlayStart(this);
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
                        if (OnPlayOver != null)
                        {
                            OnPlayOver(this);
                        }
                        if (ReleaseOnOver)
                            Dispose();
                    }
                    else
                    {
                        if (c_time >= m_time)
                            c_time -= m_time;
                        float r =  c_time / m_time;
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
    /// <summary>
    /// 用于控制着色器浮点数的动画
    /// </summary>
    public class ShaderFloat
    {
        public float StartValue;
        public float EndValue;
        float delay;
        public float Delay;
        public float Time;
        public string ParameterName;
        float SurplusTime;
        internal Material Target;
        internal void Reset()
        {
            SurplusTime = Time;
            delay = Delay;
        }
        internal static void Update(ShaderFloat t, float timeslice)
        {
            if (t.delay > 0)
            {
                t.delay -= timeslice;
                if (t.delay <= 0)
                {
                    t.SurplusTime += t.delay;
                    t.Target.SetFloat(t.ParameterName, t.StartValue);
                }
            }
            else
            {
                t.SurplusTime -= timeslice;
                if (t.SurplusTime <= 0)
                {
                    t.Target.SetFloat(t.ParameterName, t.EndValue);
                    return;
                }
                float r = 1 - t.SurplusTime / t.Time;
                float d = t.EndValue - t.StartValue;
                d *= r;
                d += t.StartValue;
                t.Target.SetFloat(t.ParameterName, d);
            }
        }
    }
    /// <summary>
    /// 用于控制着色器四维向量的动画
    /// </summary>
    public class ShaderVector4
    {
        public Vector4 StartValue;
        public Vector4 EndValue;
        float delay;
        public float Delay;
        public float Time;
        float SurplusTime;
        public string ParameterName;
        internal Material Target;
        internal void Reset()
        {
            SurplusTime = Time;
            delay = Delay;
        }
        internal static void Update(ShaderVector4 t, float timeslice)
        {
            if (t.delay > 0)
            {
                t.delay -= timeslice;
                if (t.delay <= 0)
                {
                    t.SurplusTime += t.delay;
                    t.Target.SetVector(t.ParameterName, t.StartValue);
                }
            }
            else
            {
                t.SurplusTime -= timeslice;
                if (t.SurplusTime <= 0)
                {
                    t.Target.SetVector(t.ParameterName, t.EndValue);
                    return;
                }
                float r = 1 - t.SurplusTime / t.Time;
                Vector4 d = t.EndValue - t.StartValue;
                d *= r;
                d += t.StartValue;
                t.Target.SetVector(t.ParameterName, d);
            }
        }
    }
    /// <summary>
    /// 着色器动画基本类
    /// </summary>
    public class ShaderAnimat : AnimatBase, AnimatInterface
    {
        List<ShaderFloat> lsf;
        List<ShaderVector4> lsv;
        public Action<ShaderAnimat> OnPlayStart;
        public Action<ShaderAnimat> OnPlayOver;
        public Material Target { get; private set; }
        public ShaderAnimat(Material m)
        {
            Target = m;
        }
        public void AddDelegate(ShaderFloat sf)
        {
            if (lsf == null)
                lsf = new List<ShaderFloat>();
            lsf.Add(sf);
            sf.Target = Target;
        }
        public void AddDelegate(ShaderVector4 sv)
        {
            if (lsv == null)
                lsv = new List<ShaderVector4>();
            lsv.Add(sv);
            sv.Target = Target;
        }
        public void RemoveDelegate(ShaderFloat sf)
        {
            if (lsf != null)
                lsf.Remove(sf);
        }
        public void RemoveDelegate(ShaderVector4 sv)
        {
            if (lsv != null)
                lsv.Remove(sv);
        }
        public void Update(float timeslice)
        {
            if (playing)
            {
                if (Delay > 0)
                {
                    Delay -= timeslice;
                    if (Delay <= 0)
                    {
                        if (OnPlayStart != null)
                            OnPlayStart(this);
                        c_time += Delay;
                        timeslice += Delay;
                        if (lsf != null)
                            for (int i = 0; i < lsf.Count; i++)
                                ShaderFloat.Update(lsf[i], timeslice);
                        if (lsv != null)
                            for (int i = 0; i < lsv.Count; i++)
                                ShaderVector4.Update(lsv[i], timeslice);
                    }
                }
                else
                {
                    c_time -= timeslice;
                    if (lsf != null)
                        for (int i = 0; i < lsf.Count; i++)
                            ShaderFloat.Update(lsf[i], timeslice);
                    if (lsv != null)
                        for (int i = 0; i < lsv.Count; i++)
                            ShaderVector4.Update(lsv[i], timeslice);
                    if (!Loop & c_time <= 0)
                    {
                        playing = false;
                        if (OnPlayOver != null)
                        {
                            OnPlayOver(this);
                        }
                    }
                    else
                    {
                        if (c_time <= 0)
                            Play();
                    }
                }
            }
        }
        public override void Play()
        {
            c_time = m_time;
            if (lsf != null)
                for (int i = 0; i < lsf.Count; i++)
                    lsf[i].Reset();
            if (lsv != null)
                for (int i = 0; i < lsv.Count; i++)
                    lsv[i].Reset();
            playing = true;
        }
    }
    /// <summary>
    /// 属性动画，用于更新某个类的某个属性的动画
    /// </summary>
    public class PropertyFloat
    {
        public PropertyFloat(object cla, string PropertyName)
        {
            Target = cla;
            info = cla.GetType().GetProperty(PropertyName);
        }
        public float StartValue;
        public float EndValue;
        float delay;
        public float Delay;
        public float Time;
        float SurplusTime;
        internal object Target;
        PropertyInfo info;
        internal void Reset()
        {
            SurplusTime = Time;
            delay = Delay;
            info.SetValue(Target, 0, null);
        }
        internal static void Update(PropertyFloat t, float timeslice)
        {
            if (t.delay > 0)
            {
                t.delay -= timeslice;
                if (t.delay <= 0)
                {
                    t.SurplusTime += t.delay;
                    t.info.SetValue(t.Target, t.StartValue, null);
                }
            }
            else
            {
                t.SurplusTime -= timeslice;
                if (t.SurplusTime <= 0)
                {
                    t.info.SetValue(t.Target, t.EndValue, null);
                    return;
                }
                float r = 1 - t.SurplusTime / t.Time;
                float d = t.EndValue - t.StartValue;
                d *= r;
                d += t.StartValue;
                t.info.SetValue(t.Target, d, null);
            }
        }
    }
    /// <summary>
    /// 属性动画基本类
    /// </summary>
    public class PropertyAnimat : AnimatBase, AnimatInterface
    {
        List<PropertyFloat> lpf;
        public Action<PropertyAnimat> OnPlayStart;
        public Action<PropertyAnimat> OnPlayOver;
        public void AddDelegate(PropertyFloat pf)
        {
            if (lpf == null)
                lpf = new List<PropertyFloat>();
            lpf.Add(pf);
        }
        public void RemoveDelegate(PropertyFloat pf)
        {
            if (lpf != null)
                lpf.Remove(pf);
        }
        public void Update(float timeslice)
        {
            if (playing)
            {
                if (Delay > 0)
                {
                    Delay -= timeslice;
                    if (Delay <= 0)
                    {
                        if (OnPlayStart != null)
                            OnPlayStart(this);
                        c_time += Delay;
                        timeslice += Delay;
                        if (lpf != null)
                            for (int i = 0; i < lpf.Count; i++)
                                PropertyFloat.Update(lpf[i], timeslice);
                    }
                }
                else
                {
                    c_time -= timeslice;
                    if (lpf != null)
                        for (int i = 0; i < lpf.Count; i++)
                            PropertyFloat.Update(lpf[i], timeslice);
                    if (!Loop & c_time <= 0)
                    {
                        playing = false;
                        if (OnPlayOver != null)
                        {
                            OnPlayOver(this);
                        }
                    }
                    else
                    {
                        if (c_time <= 0)
                            Play();
                    }
                }
            }
        }
        public override void Play()
        {
            c_time = m_time;
            if (lpf != null)
                for (int i = 0; i < lpf.Count; i++)
                    lpf[i].Reset();
            playing = true;
        }
    }
    /// <summary>
    /// 定时器
    /// </summary>
    public class Timer : AnimatBase, AnimatInterface
    {
        public Action<Timer> OnPlayStart;
        public Action<Timer> OnPlayOver;
        public void Update(float timeslice)
        {
            if (playing)
            {
                if (Delay > 0)
                {
                    Delay -= timeslice;
                    if (Delay <= 0)
                    {
                        if (OnPlayStart != null)
                            OnPlayStart(this);
                        c_time += Delay;
                    }
                }
                else
                {
                    c_time -= timeslice;
                    if (c_time <= 0)
                    {
                        if (!Loop)
                            playing = false;
                        else c_time += m_time;
                        if (OnPlayOver != null)
                        {
                            OnPlayOver(this);
                        }
                    }
                }
            }
        }
        public Timer()
        {
            AnimationManage.Manage.AddAnimat(this);
        }
        public void Dispose()
        {
            AnimationManage.Manage.ReleaseAnimat(this);
        }
    }
    class ToDoEvent
    {
        public float time;
        public string key;
        public Action<object> DoEvent;
        public object parameter;
        public int level;
    }
    public class DoWaitQueue : AnimatInterface
    {
        void AnimatInterface.Update(float time)
        {
            if (time_events == null)
                return;
            if (time_events.Count > 0)
            {
                var t = time_events[0];
                t.time -= time;
                label:;
                if (t.time <= 0)
                {
                    time_events.RemoveAt(0);
                    if (time_events.Count > 0)
                    {
                        var a = t.time;
                        t = time_events[0];
                        t.time += a;
                        if (t.DoEvent != null)
                            t.DoEvent(t.parameter);
                        goto label;
                    }
                }
            }
        }
        List<ToDoEvent> time_events;
        public DoWaitQueue()
        {
            time_events = new List<ToDoEvent>();
            AnimationManage.Manage.AddAnimat(this);
        }
        public void DoWait(float time, Action<object> action, object parameter)
        {
            if (time_events.Count == 0)
                if (action != null)
                    action(parameter);
            ToDoEvent to = new ToDoEvent();
            to.time = time;
            to.DoEvent = action;
            to.parameter = parameter;
            time_events.Add(to);
        }
        public void Dispose()
        {
            AnimationManage.Manage.ReleaseAnimat(this);
        }
    }
    public class GifAnimat : AnimatInterface
    {
        RawImage imge;
        public GifAnimat(RawImage img)
        {
            imge = img;
            AnimationManage.Manage.AddAnimat(this);
        }
        List<Texture2D> texture2s;
        public void Play(List<Texture2D> gif)
        {
            lifetime = 0;
            if(gif !=null)
            {
                texture2s = gif;
                imge.texture = texture2s[0];
                Playing = true;
            }
        }
        public void Pause()
        {
            Playing = false;
        }
        public Action<GifAnimat> PlayOver;
        public bool Loop;
        bool Playing;
        float lifetime = 0;
        int index = 0;
        public void Update(float time)
        {
            if(Playing)
            {
                lifetime += time;
                if(texture2s!=null)
                {
                    int c =(int) lifetime / 66;
                    if (c >= texture2s.Count)
                    {
                        if (Loop)
                        {
                            lifetime = 0;
                            imge.texture = texture2s[0];
                        }
                        else
                        {
                            Playing = false;
                            if (PlayOver != null)
                                PlayOver(this);
                        }
                    }
                    else
                    {
                        imge.texture = texture2s[c];
                    }
                }
            }
        }
        public void Dispose()
        {
            AnimationManage.Manage.ReleaseAnimat(this);
        }
    }
    public class ImageAnimat : AnimatInterface
    {
        public Image image { get; private set; }
        public ImageAnimat(Image img)
        {
            image = img;
            AnimationManage.Manage.AddAnimat(this);
        }
        Sprite[] sprites;
        public void Play(Sprite[] gif)
        {
            lifetime = 0;
            if (gif != null)
            {
                sprites = gif;
                image.sprite = sprites[0];
                image.SetNativeSize();
                Playing = true;
            }
        }
        public void Pause()
        {
            Playing = false;
        }
        public void Stop()
        {
            Playing = false;
            if (image != null)
            {
                if (sprites != null)
                {
                    image.sprite = sprites[0];
                    image.SetNativeSize();
                }
            }
        }
        public Action<ImageAnimat> PlayOver;
        public bool Loop;
        bool Playing;
        float lifetime = 0;
        int index = 0;
        public float Interval = 100;
        public bool autoHide;
        public void Update(float time)
        {
            if (Playing)
            {
                lifetime += time;
                if (sprites != null)
                {
                    int c = (int)(lifetime / Interval);
                    if (c >= sprites.Length)
                    {
                        if (Loop)
                        {
                            lifetime = 0;
                            image.sprite = sprites[0];
                            image.SetNativeSize();
                        }
                        else
                        {
                            Playing = false;
                            if (PlayOver != null)
                                PlayOver(this);
                        }
                    }
                    else
                    {
                        image.sprite = sprites[c];
                        image.SetNativeSize();
                    }
                }
            }
        }
        public void Dispose()
        {
            if (autoHide)
                image.gameObject.SetActive(false);
            AnimationManage.Manage.ReleaseAnimat(this);
        }
    }
}