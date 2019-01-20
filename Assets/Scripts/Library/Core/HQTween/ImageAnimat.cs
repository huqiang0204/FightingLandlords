using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
}
