﻿using huqiang.Core.HGUI;
using huqiang.Data;
using huqiang.UIEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace huqiang.UIComposite
{
    public class ScrollY : ScrollContent
    {
        public static void CenterScroll(ScrollY scroll)
        {
            var eve = scroll.eventCall;
            var tar = scroll.eventCall.ScrollDistanceY;
            float ty = scroll.Size.y * 0.5f;
            float v = scroll.Point + tar+ty;
            float sy = scroll.ctSize.y;
            float oy = v % sy;
            tar -= oy;
            if (oy > sy * 0.5f)
                tar += sy;
            tar += sy * 0.5f;
            scroll.eventCall.ScrollDistanceY = tar;
        }
        public UserEvent eventCall;//scrollY自己的按钮
        protected float height;
        int Column = 1;
        float m_point;
        /// <summary>
        /// 滚动的当前位置，从0开始
        /// </summary>
        public float Point { get { return m_point; } set { Refresh(0,value - m_point); } }
        /// <summary>
        /// 0-1之间
        /// </summary>
        public float Pos
        {
            get {
                if (ActualSize.y <= Size.y)
                    return 0;
                var p = m_point / (ActualSize.y - Size.y);
                if (p < 0)
                    p = 0;
                else if (p > 1)
                    p = 1;
                return p; 
            }
            set
            {
                if (value < 0 | value > 1)
                    return;
                m_point = value * (ActualSize.y - Size.y);
                Order();
            }
        }
        public bool ItemDockCenter;
        public Vector2 ContentSize { get; private set; }
        public bool DynamicSize = true;
        Vector2 ctSize;
        float ctScale;
        public override UISlider Slider { 
            get => m_slider; 
            set {
                if (m_slider != null)
                    m_slider.OnValueChanged = null;
                m_slider = value;
                if (m_slider != null)
                    m_slider.OnValueChanged = (o) => { Pos = 1 - o.Percentage; };
            } }
        public override void Initial(FakeStruct mod, UIElement script)
        {
            base.Initial(mod,script);
            eventCall = Enity.RegEvent<UserEvent>();
            eventCall.Drag = Draging;
            eventCall.DragEnd = (o, e, s) =>
            {
                Scrolling(o, s);
                if (ItemDockCenter)
                    CenterScroll(this);
                if (ScrollStart != null)
                    ScrollStart(this);
                if (eventCall.VelocityY == 0)
                    OnScrollEnd(o);
            };
            eventCall.Scrolling = Scrolling;
            eventCall.ScrollEndY = OnScrollEnd;
            eventCall.ForceEvent = true;
            eventCall.AutoColor = false;
            eventCall.CutRect = true;
            Size = Enity.SizeDelta;
            Enity.SizeChanged = (o) => {
                Refresh(0,m_point);
            };
        }
        public Action<ScrollY, Vector2> Scroll;
        public Action<ScrollY> ScrollStart;
        public Action<ScrollY> ScrollEnd;
        public float DecayRate = 0.998f;
        void Draging(UserEvent back, UserAction action, Vector2 v)
        {
            back.DecayRateY = DecayRate;
            Scrolling(back, v);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="back"></param>
        /// <param name="v">移动的实际像素位移</param>
        void Scrolling(UserEvent back, Vector2 v)
        {
            if (Main == null)
                return;
            Vector2 u = v;
            v.y /= Enity.transform.localScale.y;
            back.VelocityX = 0;
            v.x = 0;
            float x = 0;
            float y = 0;
            switch (scrollType)
            {
                case ScrollType.None:
                    y = ScrollNone(back, ref v, ref x, ref m_point).y;
                    break;
                case ScrollType.Loop:
                    y = ScrollLoop(back, ref v, ref x, ref m_point).y;
                    break;
                case ScrollType.BounceBack:
                    y = BounceBack(back, ref v, ref x, ref m_point).y;
                    break;
            }
            Order();
            if (y != 0)
            {
                if (Scroll != null)
                    Scroll(this, u);
            }
            else
            {
                if (ScrollEnd != null)
                    ScrollEnd(this);
            }
            if (m_slider != null)
            {
                m_slider.Percentage = 1 - Pos;
            }
        }
        void OnScrollEnd(UserEvent back)
        {
            if (scrollType == ScrollType.BounceBack)
            {
                if (m_point < -Tolerance)
                {
                    back.DecayRateY = 0.988f;
                    float d = - m_point;
                    back.ScrollDistanceY = d * eventCall.Context.transform.localScale.y;
                }
                else
                {
                    float max = ActualSize.y + Tolerance;
                    if (max < Size.y)
                        max = Size.y + Tolerance;
                    if (m_point + Size.y > max)
                    {
                        back.DecayRateY = 0.988f;
                        float d = ActualSize.y - m_point - Size.y;
                        back.ScrollDistanceY = d * eventCall.Context.transform.localScale.y;
                    }
                    else
                    {
                        if (ScrollEnd != null)
                            ScrollEnd(this);
                    }
                }
            }
            else if (ScrollEnd != null)
                ScrollEnd(this);
            if (m_slider != null)
                m_slider.Percentage = 1- Pos;
        }
        public void Calcul()
        {
            float w = Size.x - ItemOffset.x;
            float dw = w / ItemSize.x;
            Column = (int)dw;
            if (Column < 1)
                Column = 1;
            if (DynamicSize)
            {
                float dx = w / Column;
                ctScale =dx / ItemSize.x;
                ctSize.x = dx;
                ctSize.y = ItemSize.y * ctScale;
            }
            else
            {
                ctSize = ItemSize;
                ctScale = 1;
            }
            int c = DataLength;
            int a = c % Column;
            c /= Column;
            if (a > 0)
                c++;
            height = c * ctSize.y;
            if (height < Size.y)
                height = Size.y;
            ActualSize = new Vector2(Size.x, height);
        }
        public override void Refresh(float x = 0, float y = 0)
        {
            m_point = y;
            Size = Enity.SizeDelta;
            ActualSize = Vector2.zero;
            if (DataLength == 0)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    Items[i].target.gameObject.SetActive(false);
                }
                return;
            }
            if (ItemMod == null)
            {
                return;
            }
            Calcul();
            Order(true);
            if (m_slider != null)
            {
                m_slider.Percentage = 1 - Pos;
                if (ActualSize.y <= Enity.SizeDelta.y)
                    m_slider.Enity.gameObject.SetActive(false);
                else m_slider.Enity.gameObject.SetActive(true);
            }
        }
        /// <summary>
        /// 指定下标处的位置重排
        /// </summary>
        /// <param name="_index"></param>
        public void ShowByIndex(int _index)
        {
            ActualSize = Vector2.zero;
            if (DataLength == 0)
            {
                for (int i = 0; i < Items.Count; i++)
                    Items[i].target.gameObject.SetActive(false);
                return;
            }
            if (ItemMod == null)
            {
                return;
            }
            Calcul();
            float y = _index * ctSize.y;
            m_point = y;
            Order(true);
        }
        void Order(bool force=false)
        {
            int len = DataLength;
            if (len <= 0)
                return;
            float ly = ctSize.y;
            int sr = (int)(m_point /ly);//起始索引
            int er = (int)((m_point + Size.y) / ly)+1;
            sr *= Column;
            er *= Column;//结束索引
            int e = er - sr;//总计显示数据
            if (e > len)
                e = len;
            if(scrollType==ScrollType.Loop)
            {
                if (er >= len)
                {
                    er -= len;
                    RecycleInside(er, sr);
                }
                else
                {
                    RecycleOutside(sr, er);
                }
            }
            else
            {
                if (sr < 0)
                    sr = 0;
                if (er >= len)
                    er = len;
                e = er - sr;
                RecycleOutside(sr, er);
            }
            PushItems();//将未被回收的数据压入缓冲区
            int index = sr;
            float oy = 0;
            for (int i=0;i<e;i++)
            {
                UpdateItem(index,oy,force);
                index++;
                if (index >= len)
                {
                    index = 0;
                    oy = ActualSize.y;
                }
            }
            RecycleRemain();
        }
        void UpdateItem(int index,float oy,bool force)
        {
            float ly = ctSize.y;
            int row = index / Column;
            float dy = ly * row + oy;
            dy -= m_point;
            float ss = (1 - Enity.Pivot.y) * Size.y - 0.5f * ly;//0.5f * Size.y 
            dy = ss - dy;
            float ox = (index%Column) * ctSize.x + ctSize.x * 0.5f + ItemOffset.x - Size.x * 0.5f;
            var a = PopItem(index);
            a.target.localPosition = new Vector3(ox, dy, 0);
            a.target.localScale = new Vector3(ctScale,ctScale,ctScale);
            Items.Add(a);
            if (a.index < 0 | force)
            {
                var dat = GetData(index);
                a.datacontext = dat;
                a.index = index;
                ItemUpdate(a.obj, dat, index);
            }
        }
        public static ScrollItem GetCenterItem(List<ScrollItem> items)
        {
            if (items.Count < 1)
                return null;
            float min = 100;
            ScrollItem item = items[0];
            for (int i = 1; i < items.Count; i++)
            {
                float y = items[i].target.localPosition.y;
                if (y < 0)
                    y = -y;
                if (y < min)
                {
                    min = y;
                    item = items[i];
                }
            }
            return item;
        }
    }
}
