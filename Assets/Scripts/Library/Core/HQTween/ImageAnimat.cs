using System;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang
{
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
    public class ColorAnimat : AnimatBase, AnimatInterface
    {
        public Graphic Target { get; private set; }
        public ColorAnimat(Graphic img)
        {
            Target = img;
            AnimationManage.Manage.AddAnimat(this);
        }
        public void Play()
        {
            lifetime = 0;
            Playing = true;
        }
        public Action<ColorAnimat> PlayOver;
        bool Playing;
        float lifetime = 0;
        int index = 0;
        public float Interval = 100;
        public bool autoHide;
        public Color StartColor;
        public Color EndColor;
        public void Update(float time)
        {
            if (playing)
            {
                if (Delay > 0)
                {
                    Delay -= time;
                    if (Delay <= 0)
                    {
                        c_time = -Delay;
                    }
                }
                else
                {
                    c_time += time;
                    if (!Loop & c_time >= m_time)
                    {
                        playing = false;
                        Target.color = EndColor;
                        if (PlayOver != null)
                            PlayOver(this);
                    }
                    else
                    {
                        if (c_time >= m_time)
                            c_time -= m_time;
                        float r = c_time / m_time;
                        if (Linear != null)
                            r = Linear(this, r);
                        Color v = EndColor- StartColor;
                        Target.color = StartColor + v * r;
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
