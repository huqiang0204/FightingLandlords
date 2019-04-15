﻿using huqiang.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace huqiang.UIEvent
{
    public class BaseEvent
    {
        static List<BaseEvent> events=new List<BaseEvent>();
        static List<UIElement> Roots;
        public static void ClearEvent()
        {
            events.Clear();
        }
        public static Vector2 MinBox = new Vector2(80, 80);
        public static bool PauseEvent;
        internal static void DispatchEvent(UserInput action)
        {
            if (PauseEvent)
                return;
            if (events.Count == 0)
                return;
            if (Roots != null)
                for (int j = 0; j < Roots.Count; j++)
                {
                    var t = Roots[j];
                    if (t != null)
                        for (int i = t.child.Count- 1; i >= 0; i--)
                        {
                            var r = t.child[i];
                            if (DispatchEvent(r , Vector3.zero, Vector3.one, Quaternion.identity, action))
                                goto label;
                        }
                }
            label:;
        }
        public static bool DispatchEvent(UIElement ui, Vector3 pos, Vector3 scale, Quaternion quate, UserInput action)
        {
            if (ui == null)
            {
                return false;
            }
            if (!ui.activeSelf)
                return false;
            Vector3 p = ui.data.localPosition;
            Vector3 o = Vector3.zero;
            o.x = p.x * scale.x;
            o.y = p.y * scale.y;
            o.z = p.z * scale.z;
            o += pos;
            Vector3 s = ui.data.localScale;
            Quaternion q = ui.data.localRotation * quate;
            s.x *= scale.x;
            s.y *= scale.y;
            var callBack = ui.baseEvent;
            if (callBack == null)
            {
                var child = ui.child;
                for (int i = child.Count - 1; i >= 0; i--)
                {
                    if (DispatchEvent(child[i], o, s, q, action))
                    {
                        return true;
                    }
                }
            }
            else if (callBack.Forbid)
            {
                var child = ui.child;
                for (int i = child.Count - 1; i >= 0; i--)
                {
                    if (DispatchEvent(child[i], o, s, q, action))
                        return true;
                }
            }
            else
            {
                callBack.pgs = scale;
                callBack.GlobalScale = s;
                callBack.GlobalPosition = o;
                callBack.GlobalRotation = q;
                bool inside = false;
                float w = ui.data.sizeDelta.x * s.x;
                float h = ui.data.sizeDelta.y * s.y;
                if (!callBack.UseActualSize)
                {
                    if (w < MinBox.x)
                        w = MinBox.x;
                    if (h < MinBox.y)
                        h = MinBox.y;
                }
                if (callBack.IsCircular)
                {
                    float x = action.CanPosition.x - o.x;
                    float y = action.CanPosition.y - o.y;
                    w *= 0.5f;
                    if (x * x + y * y < w * w)
                        inside = true;
                }
                else
                {
                    float x1 = 0.5f * w;
                    float x0 = -x1;
                    float y1 = 0.5f * h;
                    float y0 = -y1;

                    var v = action.CanPosition;
                    var Rectangular = callBack.Rectangular;
                    Rectangular[0] = q * new Vector3(x0, y0) + o;
                    Rectangular[1] = q * new Vector3(x0, y1) + o;
                    Rectangular[2] = q * new Vector3(x1, y1) + o;
                    Rectangular[3] = q * new Vector3(x1, y0) + o;
                    inside = Physics2D.DotToPolygon(Rectangular, v);
                }
                if (inside)
                {
                    var child = ui.child;
                    //action.CurrentEntry.Add(callBack);
                    for (int i = child.Count- 1; i >= 0; i--)
                    {
                        if (DispatchEvent(child[i], o, s, q, action))
                        {
                            if (callBack.ForceEvent)
                            {
                                if (!callBack.Forbid)
                                    break;
                            }
                            return true;
                        }
                    }
                    if (action.IsLeftButtonDown | action.IsRightButtonPressed | action.IsMiddleButtonPressed)
                    {
                        callBack.OnMouseDown(action);
                    }
                    else if (action.IsLeftButtonUp | action.IsRightButtonUp | action.IsMiddleButtonUp)
                    {
                        if (callBack.Pressed)
                            callBack.OnMouseUp(action);
                    }
                    else
                    {
                        callBack.OnMouseMove(action);
                    }
                    if (callBack.Penetrate)
                        return false;
                    return true;
                }
                else if (!callBack.CutRect)
                {
                    var child = ui.child;
                    for (int i = child.Count- 1; i >= 0; i--)
                    {
                        if (DispatchEvent(child[i], o, s, q, action))
                            return true;
                    }
                }
            }
            return false;
        }
        internal static void Rolling()
        {
            for (int i = 0; i < events.Count; i++)
                if (events[i] != null)
                    if (!events[i].Forbid)
                        if (!events[i].Pressed)
                            DuringSlide(events[i]);
        }
        static void DuringSlide(BaseEvent back)
        {
            if (back.mVelocity.x == 0 & back.mVelocity.y == 0)
                return;
            back.xTime += UserInput.TimeSlice;
            back.yTime += UserInput.TimeSlice;
            float x = 0, y = 0;
            bool endx = false, endy = false;
            if (back.mVelocity.x != 0)
            {
                float t = (float)MathH.PowDistance(back.DecayRateX, back.maxVelocity.x, back.xTime);
                x = t - back.lastX;
                back.lastX = t;
                float vx = Mathf.Pow(back.DecayRateX, back.xTime) * back.maxVelocity.x;
                if (vx < 0.001f & vx > -0.001f)
                {
                    back.mVelocity.x = 0;
                    endx = true;
                }
                else back.mVelocity.x = vx;
            }
            if (back.mVelocity.y != 0)
            {
                float t = (float)MathH.PowDistance(back.DecayRateY, back.maxVelocity.y, back.yTime);
                y = t - back.lastY;
                back.lastY = t;
                float vy = Mathf.Pow(back.DecayRateY, back.yTime) * back.maxVelocity.y;
                if (vy < 0.001f & vy > -0.001f)
                {
                    back.mVelocity.y = 0;
                    endy = true;
                }
                else back.mVelocity.y = vy;
            }
            if (back.Scrolling != null)
                back.Scrolling(back, new Vector2(x, y));
            if (endx)
                if (back.ScrollEndX != null)
                    back.ScrollEndX(back);
            if (endy)
                if (back.ScrollEndY != null)
                    back.ScrollEndY(back);
        }
        public UIElement Context;
        Vector2 mVelocity;
        public float VelocityX { get { return mVelocity.x; } set { maxVelocity.x = mVelocity.x = value; RefreshRateX(); } }
        public float VelocityY { get { return mVelocity.y; } set { maxVelocity.y = mVelocity.y = value; RefreshRateY(); } }
        public void StopScroll()
        {
            mVelocity.x = 0;
            mVelocity.y = 0;
        }
        int xTime;
        int yTime;
        float lastX;
        float lastY;
        Vector2 maxVelocity;
        Vector2 sDistance;
        public float ScrollDistanceX
        {
            get { return sDistance.x; }
            set
            {
                if (value == 0)
                    maxVelocity.x = 0;
                else
                    maxVelocity.x = (float)MathH.DistanceToVelocity(DecayRateX, value);
                mVelocity.x = maxVelocity.x;
                sDistance.x = value;
                xTime = 0;
                lastX = 0;
            }
        }
        public float ScrollDistanceY
        {
            get { return sDistance.y; }
            set
            {
                if (value == 0)
                    maxVelocity.y = 0;
                else
                    maxVelocity.y = (float)MathH.DistanceToVelocity(DecayRateY, value);
                mVelocity.y = maxVelocity.y;
                sDistance.y = value;
                yTime = 0;
                lastY = 0;
            }
        }
        public float DecayRateX = 0.998f;
        public float DecayRateY = 0.998f;
        public float speed = 1f;
        public static long ClickTime = 1800000;
        public static float ClickArea = 400;
        public Vector2 RawPosition { get; protected set; }
        Vector2 LastPosition;
        public int HoverTime { get; protected set; }
        public long pressTime { get; internal set; }
        public long entryTime { get; protected set; }
        public long stayTime { get; protected set; }
        public bool Pressed { get; internal set; }
        public bool Forbid;
        public bool CutRect = false;
        /// <summary>
        /// 强制事件不被子组件拦截
        /// </summary>
        public bool ForceEvent = false;
        /// <summary>
        /// 允许事件穿透
        /// </summary>
        public bool Penetrate = false;
        /// <summary>
        /// 当此项开启时忽略最小尺寸校正
        /// </summary>
        public bool UseActualSize = false;
        public bool IsCircular = false;
        public bool entry { get; protected set; }
        private int index;
        public bool AutoColor = true;
        Color g_color;
        public object DataContext;
        Vector3 pgs = Vector3.one;
        public Vector3 GlobalScale = Vector3.one;
        public Vector3 GlobalPosition;
        public Quaternion GlobalRotation;
        public Vector3[] Rectangular { get; private set; }
        public BaseEvent(UIElement element)
        {
            Context = element;
            Rectangular = new Vector3[4];
            events.Add(this);
        }
        void RefreshRateX()
        {
            xTime = 0;
            lastX = 0;
            if (maxVelocity.x == 0)
                sDistance.x = 0;
            else
                sDistance.x = (float)MathH.PowDistance(DecayRateX, maxVelocity.x, 1000000);
        }
        void RefreshRateY()
        {
            yTime = 0;
            lastY = 0;
            if (maxVelocity.y == 0)
                sDistance.y = 0;
            else
                sDistance.y = (float)MathH.PowDistance(DecayRateY, maxVelocity.y, 1000000);
        }
        #region event
        public Action<BaseEvent, UserInput> PointerDown;
        public Action<BaseEvent, UserInput> PointerUp;
        public Action<BaseEvent, UserInput> Click;
        public Action<BaseEvent, UserInput> PointerEntry;
        public Action<BaseEvent, UserInput> PointerMove;
        public Action<BaseEvent, UserInput> PointerLeave;
        public Action<BaseEvent, UserInput> PointerHover;
        public Action<BaseEvent, UserInput> MouseWheel;
        public Action<BaseEvent, UserInput, Vector2> Drag;
        public Action<BaseEvent, UserInput, Vector2> DragEnd;
        public Action<BaseEvent, Vector2> Scrolling;
        public Action<BaseEvent> ScrollEndX;
        public Action<BaseEvent> ScrollEndY;
        public Action<BaseEvent, UserInput> LostFocus;

        UserInput FocusAction;
        public virtual void OnMouseDown(UserInput action)
        {
            //if (!action.MultiFocus.Contains(this))
            //    action.MultiFocus.Add(this);
            Pressed = true;
            pressTime = action.EventTicks;
            RawPosition = action.CanPosition;
            if (PointerDown != null)
                PointerDown(this, action);
            entry = true;
            FocusAction = action;
            mVelocity = Vector2.zero;
        }
        protected virtual void OnMouseUp(UserInput action)
        {
            Pressed = false;
            entry = false;
            if (PointerUp != null)
                PointerUp(this, action);
            long r = DateTime.Now.Ticks - pressTime;
            if (r <= ClickTime)
            {
                float x = RawPosition.x - action.CanPosition.x;
                float y = RawPosition.y - action.CanPosition.y;
                x *= x;
                y *= y;
                x += y;
                if (x < ClickArea)
                    if (Click != null)
                        Click(this, action);
            }
        }
        protected virtual void OnMouseMove(UserInput action)
        {
            if (!entry)
            {
                entry = true;
                entryTime = DateTime.Now.Ticks;
                if (PointerEntry != null)
                    PointerEntry(this, action);
                LastPosition = action.CanPosition;
            }
            else
            {
                stayTime = action.EventTicks - entryTime;
                if (action.CanPosition == LastPosition)
                {
                    HoverTime += UserInput.TimeSlice * 2000;
                    if (HoverTime > ClickTime)
                        if (PointerHover != null)
                            PointerHover(this, action);
                }
                else
                {
                    HoverTime = 0;
                    LastPosition = action.CanPosition;
                    if (PointerMove != null)
                        PointerMove(this, action);
                }
            }
        }
        internal virtual void OnMouseLeave(UserInput action)
        {
            entry = false;
            if (PointerLeave != null)
                PointerLeave(this, action);
        }
        internal virtual void OnFocusMove(UserInput action)
        {
            if (Pressed)
                OnDrag(action);
        }
        protected virtual void OnDrag(UserInput action)
        {
            if (Drag != null)
            {
                var v = action.Motion;
                v.x /= pgs.x;
                v.y /= pgs.y;
                Drag(this, action, v);
            }
        }
        internal virtual void OnDragEnd(UserInput action)
        {
            if (Scrolling != null)
            {
                var v = action.Velocities;
                v.x /= GlobalScale.x;
                v.y /= GlobalScale.y;
                maxVelocity = mVelocity = v;
                RefreshRateX();
                RefreshRateY();
            }
            if (DragEnd != null)
            {
                var v = action.Motion;
                v.x /= pgs.x;
                v.y /= pgs.y;
                DragEnd(this, action, v);
            }
        }
        internal virtual void OnLostFocus(UserInput action)
        {
            FocusAction = null;
            if (LostFocus != null)
                LostFocus(this, action);
        }
        public virtual void Reset()
        {
            Reset(this);
        }
        static void Reset(BaseEvent eventCall)
        {
            eventCall.PointerDown = null;
            eventCall.PointerUp = null;
            eventCall.Click = null;
            eventCall.PointerEntry = null;
            eventCall.PointerMove = null;
            eventCall.PointerLeave = null;
            eventCall.Drag = null;
            eventCall.DragEnd = null;
            eventCall.Scrolling = null;
            eventCall.AutoColor = true;
            eventCall.Forbid = false;
            eventCall.mVelocity = Vector2.zero;
            eventCall.maxVelocity = Vector2.zero;
            eventCall.CutRect = false;
            eventCall.ForceEvent = false;
            eventCall.Penetrate = false;
        }
        #endregion
    }
}