using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang.UIModel
{
    public unsafe struct ImageAttribute
    {
        public float alphaHit;
        public float fillAmount;
        public bool fillCenter;
        public bool fillClockwise;
        public Image.FillMethod fillMethod;
        public Int32 fillOrigin;
        public bool preserveAspect;
        public Image.Type type;
        public Int32 material;
        public Int32 shader;
        public Color color;
        public Int32 assetName;
        public Int32 textureName;
        public Int32 spriteName;
        public static int Size = 64;//sizeof(ImageAttribute);
        public static void LoadFromBuff(ref ImageAttribute img, void* p)
        {
            fixed (float* trans = &img.alphaHit)
            {
                Int32* a =(Int32*) trans;
                Int32* b = (Int32*)p;
                for (int i = 0; i < 16; i++)
                {
                    *a = *b;
                    a++;
                    b++;
                }
            }
        }
    }
    public class ImageElement : ModelElement
    {
        public ImageAttribute imageAttribute;

        public Sprite sprite;
        public Material material;
        string assetName;
        string textureName;
        string spriteName;
        string shader;
        string smat;
        public unsafe override byte* LoadFromBuff(byte* point)
        {
            //transAttribute = *(ElementAttribute*)point;
            ElementAttribute.LoadFromBuff(ref transAttribute,point);
            name = StringAssets[transAttribute.name];
            tag = StringAssets[transAttribute.tag];
            point += ElementAttribute.Size;
            //imageAttribute = *(ImageAttribute*)point;
            ImageAttribute.LoadFromBuff(ref imageAttribute,point);
            if (imageAttribute.material > -1)
            {
                smat = StringAssets[imageAttribute.material];
                shader = StringAssets[imageAttribute.shader];
            }
            if (imageAttribute.spriteName > -1)
            {
                if (imageAttribute.assetName > -1)
                   assetName = StringAssets[imageAttribute.assetName];
                textureName = StringAssets[imageAttribute.textureName];
                spriteName = StringAssets[imageAttribute.spriteName];
            }
            return point + ImageAttribute.Size;
        }
        public unsafe override byte[] ToBytes()
        {
            int size = ElementAttribute.Size;
            int tsize = ImageAttribute.Size;
            byte[] buff = new byte[size + tsize];
            fixed (byte* bp = &buff[0])
            {
                *(ElementAttribute*)bp = transAttribute;
                byte* a = bp + size;
                *(ImageAttribute*)a = imageAttribute;
            }
            return buff;
        }
        static void Load(GameObject tar, ref ImageAttribute att)
        {
            var a = tar.GetComponent<Image>();
            a.alphaHitTestMinimumThreshold = att.alphaHit;
            a.fillAmount = att.fillAmount;
            a.fillCenter = att.fillCenter;
            a.fillClockwise = att.fillClockwise;
            a.fillMethod = att.fillMethod;
            a.fillOrigin = att.fillOrigin;
            a.preserveAspect = att.preserveAspect;
            a.type = att.type;
            a.raycastTarget = false;
            a.color = att.color;
        }
        static void Save(GameObject tar, ref ImageAttribute att)
        {
            var b = tar.GetComponent<Image>();
            if (b != null)
            {
                att.alphaHit = b.alphaHitTestMinimumThreshold;
                att.fillAmount = b.fillAmount;
                att.fillCenter = b.fillCenter;
                att.fillClockwise = b.fillClockwise;
                att.fillMethod = b.fillMethod;
                att.fillOrigin = b.fillOrigin;
                att.preserveAspect = b.preserveAspect;
                if (b.sprite != null)
                {
                    att.spriteName = SaveString(b.sprite.name);
                    string tn = b.sprite.texture.name;
                    att.textureName = SaveString(tn);
                    var an = ElementAsset.TxtureFormAsset(tn);
                    if (an != null)
                        att.assetName = SaveString(an);
                    else att.assetName = -1;
                }
                else att.spriteName = -1;
                att.type = b.type;
                var mat = b.material;
                att.material = SaveString(mat.name);
                att.shader = SaveString(mat.shader.name);
                att.color = b.color;
            }
        }
        public override void Load(GameObject tar)
        {
            base.Load(tar);
            Load(tar, ref this.imageAttribute);
            var img = tar.GetComponent<Image>();
            if (smat != null)
                if (smat != "Default UI Material")
                    img.material = new Material(Shader.Find(shader));
            if(spriteName!=null)
                img.sprite = ElementAsset.FindSprite(assetName, textureName, spriteName);
        }
        public override void Save(GameObject tar)
        {
            base.Save(tar);
            Save(tar, ref this.imageAttribute);
        }
    }
    public class ViewportElement : ImageElement
    {
        public override void Load(GameObject tar)
        {
            base.Load(tar);
            var mask = tar.GetComponent<Mask>();
            if(mask!=null)
            mask.showMaskGraphic = false;
        }
    }
}
