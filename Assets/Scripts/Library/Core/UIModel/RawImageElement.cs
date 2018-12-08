using System;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang.UIModel
{
    public unsafe struct RawImageAttribute
    {
        public Rect uvRect;
        public Color color;
        public Int32 material;
        public Int32 shader;
        public Int32 assetName;
        public Int32 textureName;
        public static int Size = 48;////sizeof(RawImageAttribute);
        public static void LoadFromBuff(ref RawImageAttribute raw, void* p)
        {
            fixed (Rect* trans = &raw.uvRect)
            {
                Int32* a =(Int32*) trans;
                Int32* b = (Int32*)p;
                for (int i = 0; i < 12; i++)
                {
                    *a = *b;
                    a++;
                    b++;
                }
            }
        }
    }
    public class RawImageElement:ModelElement
    {
        RawImageAttribute raw;
        public Texture texture;
        public Material material;
        string assetName;
        string textureName;
        string shader;
        string smat;
        public unsafe override byte* LoadFromBuff(byte* point)
        {
            //transAttribute = *(ElementAttribute*)point;
            ElementAttribute.LoadFromBuff(ref transAttribute,point);
            name = StringAssets[transAttribute.name];
            tag = StringAssets[transAttribute.tag];
            point += ElementAttribute.Size;
            //raw = *(RawImageAttribute*)point;
            RawImageAttribute.LoadFromBuff(ref raw, point);
            if (raw.material > -1)
            {
                smat = StringAssets[raw.material];
                shader = StringAssets[raw.shader];
            }
            if (raw.textureName > -1)
            {
                if (raw.assetName > -1)
                    assetName = StringAssets[raw.assetName];
                textureName = StringAssets[raw.textureName];
            }
            return point + RawImageAttribute.Size;
        }
        public unsafe override byte[] ToBytes()
        {
            int size = ElementAttribute.Size;
            int tsize = RawImageAttribute.Size;
            byte[] buff = new byte[size + tsize];
            fixed (byte* bp = &buff[0])
            {
                *(ElementAttribute*)bp = transAttribute;
                byte* a = bp + size;
                *(RawImageAttribute*)a = raw;
            }
            return buff;
        }
        static void Load(GameObject tar, ref RawImageAttribute att)
        {
            var a = tar.GetComponent<RawImage>();
            a.uvRect = att.uvRect;
            a.color = att.color;
            a.raycastTarget = false;
        }
        static void Save(GameObject tar, ref RawImageAttribute att)
        {
            var b = tar.GetComponent<RawImage>();
            if (b != null)
            {
                if (b.texture != null)
                {
                    string tn = b.texture.name;
                    att.textureName = SaveString(tn);
                    var an = ElementAsset.TxtureFormAsset(tn);
                    if (an != null)
                        att.assetName = SaveString(an);
                }
                else att.textureName = -1;
                var mat = b.material;
                att.material = SaveString(mat.name);
                att.shader = SaveString(mat.shader.name);
                att.color = b.color;
                att.uvRect = b.uvRect;
            }
        }
        public override void Load(GameObject tar)
        {
            base.Load(tar);
            Load(tar, ref this.raw);
            var img = tar.GetComponent<RawImage>();
            if (smat != null)
                if (smat != "Default UI Material")
                    img.material = new Material(Shader.Find(shader));
            if (textureName != null)
                img.texture = ElementAsset.FindTexture(assetName, textureName);
        }
        public override void Save(GameObject tar)
        {
            base.Save(tar);
            Save(tar, ref this.raw);
        }
    }
}
