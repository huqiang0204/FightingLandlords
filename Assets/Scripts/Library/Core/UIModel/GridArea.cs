using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace huqiang.UIModel
{
    /// <summary>
    /// 4X2 竖排4横排2
    /// </summary>
    public class GridArea
    {
        /// <summary>
        /// 检测鼠标是否在横线里面
        /// </summary>
        /// <param name="sx">线开始x坐标</param>
        /// <param name="ex">线结束x坐标</param>
        /// <param name="sy">线坐标y</param>
        /// <param name="px">鼠标x</param>
        /// <param name="py">鼠标y</param>
        /// <returns></returns>
        static bool PointInsideLine(float sx, float ex, float sy, float px, float py)
        {
            if(px>sx)
            {
                if (px < ex)
                    goto label;
            }else if(px>ex)
            {
                if (px < sx)
                    goto label;
            }
            return false;
            label:;
            float ay = sy - 0.5f * LineWidth;
            float by = sy + 0.5f * LineWidth;
            if (ay < py & by > py)
                return true;
            return false;
        }
        /// <summary>
        /// 检测鼠标是否在竖线里面
        /// </summary>
        /// <param name="sx">线坐标x</param>
        /// <param name="sy">线开始y坐标</param>
        /// <param name="ey">线结束y坐标</param>
        /// <param name="px">鼠标x</param>
        /// <param name="py">鼠标y</param>
        /// <returns></returns>
        static bool PointInsideVertical(float sx,float sy,float ey,float px,float py)
        {
            if (py < ey)
            {
                if(py>sy)
                    goto label;
            }
            else if(py<sy)
            {
                if(py>ey)
                    goto label;
            }
            return false;
            label:;
            float ax = sx - 0.5f * LineWidth;
            float bx = sx + 0.5f * LineWidth;
            if (ax < px & bx > px)
                return true;
            return false;
        }
        public static float LineWidth=10;
        List<BigArea> areas;//所有区域
        List<BigArea> OptAreas;//当前正在操作的区域
        public GridArea()
        {
            areas = new List<BigArea>();
            OptAreas = new List<BigArea>();
        }
        public void AddItem(Area area)
        {

        }
  
        public bool DragLine(Vector2 pos)
        {
            OptAreas.Clear();
            for(int i=0;i<areas.Count;i++)
            {
                var a = areas[i];
                float w = a.w * 0.5f;
                float h = a.h * 0.5f;
                float x0 = a.x - w;
                float x1 = a.x + w;
                float y0 = a.y - h;
                float y1 = a.y + h;
                if(PointInsideVertical(x0,y0,y1,pos.x,pos.y))//左边
                {

                }
                else if (PointInsideVertical(x1, y0, y1, pos.x, pos.y))//右边
                {

                }
                else if(PointInsideLine(x0,x1,y0,pos.x,pos.y))//底部线
                {

                }else if(PointInsideLine(x0, x1, y1, pos.x, pos.y))//顶部线
                {

                }
            }
            if (OptAreas.Count > 0)
                return true;
            return false;
        }
    }
    public class Line
    {
        public RectTransform rect;
        public EventCallBack callBack;
        public Line()
        {
            areas = new List<Area>();
        }
        public List<Area> areas;//所有关联的区域
    }
    public class BigArea
    {
        public float x;
        public float y;
        public float w;
        public float h;
        List<Area> OptAreas;//当前正在操作的区域
        List<Area> areas;//所有区域
        public BigArea()
        {
            areas = new List<Area>();
        }
        public void DragLine()
        {

        }
    }
    public class Area
    {
        public float x;
        public float y;
        public float w;
        public float h;
        public Action<Area> AreaChanged;
    }
}
