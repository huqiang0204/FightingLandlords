﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang
{
    public static class AnimationExtand
    {
        public static void MoveTo(this Transform trans, Vector3 pos, float time, float delay = 0, Action<MoveAnimat> over = null, bool cover = true)
        {
            if (trans == null)
                return;
            trans.gameObject.SetActive(true);
            var ani = AnimationManage.Manage.FindAni<MoveAnimat>((o) => { return o.Target == trans ? true : false; });
            if (ani == null)
                ani = new MoveAnimat(trans);
            else if (!cover)
                return;
            ani.StartPosition = trans.localPosition;
            ani.EndPosition = pos;
            ani.Time = time;
            ani.Delay = time;
            if (over == null)
                ani.PlayOver = (o) => { o.Dispose(); };
            else ani.PlayOver = over;
            ani.Play();
        }
        public static void RotateTo(this Transform trans, Vector3 angle, float time, float delay = 0, Action<RotateAnimat> over = null, bool cover = true)
        {
            if (trans == null)
                return;
            trans.gameObject.SetActive(true);
            var ani = AnimationManage.Manage.FindAni<RotateAnimat>((o) => { return o.Target == trans ? true : false; });
            if (ani == null)
                ani = new RotateAnimat(trans);
            else if (!cover)
                return;
            ani.StartAngle = trans.localEulerAngles;
            ani.EndAngle = angle;
            ani.Time = time;
            ani.Delay = delay;
            if (over == null)
                ani.PlayOver = (o) => { o.Dispose(); };
            else ani.PlayOver = over;
            ani.Play();
        }
        public static void ScaleTo(this Transform trans, Vector3 scale, float time, float delay, Action<ScaleAnimat> over = null, bool cover = true)
        {
            if (trans == null)
                return;
            trans.gameObject.SetActive(true);
            var ani = AnimationManage.Manage.FindAni<ScaleAnimat>((o) => { return o.Target == trans ? true : false; });
            if (ani == null)
                ani = new ScaleAnimat(trans);
            else if (!cover)
                return;
            ani.StartScale = trans.localScale;
            ani.EndScale = scale;
            ani.Time = time;
            ani.Delay = delay;
            if (over == null)
                ani.PlayOver = (o) => { o.Dispose(); };
            else ani.PlayOver = over;
            ani.Play();
        }
        public static void Play(this Image img, Sprite[] sprites, float inter = 16, Action<ImageAnimat> over = null, bool hide = true, bool cover = true)
        {
            if (img == null)
                return;
            img.gameObject.SetActive(true);
            var ani = AnimationManage.Manage.FindAni<ImageAnimat>((o) => { return o.image == img ? true : false; });
            if (ani == null)
                ani = new ImageAnimat(img);
            else if (!cover)
                return;
            ani.autoHide = hide;
            if (over == null)
                ani.PlayOver = (o) => { o.Dispose(); };
            else ani.PlayOver = over;
            ani.Interval = inter;
            ani.Play(sprites);
        }
        public static void Play(this Material mat,string name, float sv,float ev,float time,float delay=0, Action<ShaderAnimat> over = null, bool cover =true)
        {
            if (mat == null)
                return;
            var ani = AnimationManage.Manage.FindAni<ShaderAnimat>((o) => { return o.Target == mat ? true : false; });
            if (ani == null)
            {
                ani = new ShaderAnimat(mat);
                ShaderFloat sf = new ShaderFloat();
                sf.ParameterName = name;
                sf.StartValue = sv;
                sf.EndValue = ev;
                sf.Time = time;
                sf.Delay = delay;
                ani.Time = time;
                if (over == null)
                    ani.PlayOver = (o) => { o.Dispose(); };
                else ani.PlayOver = over;
            }
            else
            {
                var sf = ani.FindFloatShader(name);
                if (sf != null)
                {
                    if (!cover)
                        return;
                }
                else sf = new ShaderFloat();
                sf.StartValue = sv;
                sf.EndValue = ev;
                sf.Time = time;
                sf.Delay = delay;
                ani.Time = time;
                if (over == null)
                    ani.PlayOver = (o) => { o.Dispose(); };
                else ani.PlayOver = over;
            }
            ani.Play();
        }
        public static void Play(this Material mat, string name, Vector4 sv, Vector4 ev, float time, float delay = 0, Action<ShaderAnimat> over = null, bool cover = true)
        {
            if (mat == null)
                return;
            var ani = AnimationManage.Manage.FindAni<ShaderAnimat>((o) => { return o.Target == mat ? true : false; });
            if (ani == null)
            {
                ani = new ShaderAnimat(mat);
                ShaderVector4 sf = new ShaderVector4();
                sf.ParameterName = name;
                sf.StartValue = sv;
                sf.EndValue = ev;
                sf.Time = time;
                sf.Delay = delay;
                ani.Time = time;
                if (over == null)
                    ani.PlayOver = (o) => { o.Dispose(); };
                else ani.PlayOver = over;
            }
            else
            {
                var sf = ani.FindVectorShader(name);
                if (sf != null)
                {
                    if (!cover)
                        return;
                }
                else sf = new ShaderVector4();
                sf.StartValue = sv;
                sf.EndValue = ev;
                sf.Time = time;
                sf.Delay = delay;
                ani.Time = time;
                if (over == null)
                    ani.PlayOver = (o) => { o.Dispose(); };
                else ani.PlayOver = over;
            }
            ani.Play();
        }
        public static void Play(this RawImage raw, List<Texture2D> t2ds,float inter, Action<GifAnimat> over = null,bool hide=true,bool cover=true)
        {
            if (raw == null)
                return;
            raw.gameObject.SetActive(true);
            var ani = AnimationManage.Manage.FindAni<GifAnimat>((o) => { return o.image == raw ? true : false; });
            if (ani == null)
            {
                ani = new GifAnimat(raw);
                if (over == null)
                    ani.PlayOver = (o) => { o.Dispose(); };
                else ani.PlayOver = over;
            }
            else if (!cover)
                return;
            ani.Play(t2ds);
        }
    }
}
