using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang.UIModel
{
    public unsafe struct TextAttribute
    {
        public bool alignByGeometry;
        public TextAnchor alignment;
        public Int32 fontSize;
        public FontStyle fontStyle;
        public HorizontalWrapMode horizontalOverflow;
        public float lineSpacing;
        public bool resizeTextForBestFit;
        public Int32 resizeTextMaxSize;
        public Int32 resizeTextMinSize;
        public bool supportRichText;
        public Color color;
        public VerticalWrapMode verticalOverflow;
        public Int32 font;
        public Int32 text;
        public Int32 material;
        public Int32 shader;
        public static int Size = 76;//sizeof(TextAttribute);
        public static void LoadFromBuff(ref TextAttribute txt, void* p)
        {
            fixed (Boolean* trans = &txt.alignByGeometry)
            {
                Int32* a =(Int32*) trans;
                Int32* b = (Int32*)p;
                for (int i = 0; i < 19; i++)
                {
                    *a = *b;
                    a++;
                    b++;
                }
            }
        }
    }
    public class TextElement : ModelElement
    {
        public static List<Font> fonts=new List<Font>();
        public static Font FindFont(string str)
        {
            if (fonts == null)
                return null;
            for(int i=0;i<fonts.Count;i++)
            {
                if (str == fonts[i].name)
                    return fonts[i];
            }
            if (fonts.Count > 0)
                return fonts[0];
            return null;
        }
        public TextAttribute textAttribute;
        public Font font;
        public Material material;
        string text;
        string fontName;
        string smat;
        string shader;
        public unsafe override byte* LoadFromBuff(byte* point)
        {
            //transAttribute = *(ElementAttribute*)point;
            ElementAttribute.LoadFromBuff(ref transAttribute,point);
            name = StringAssets[transAttribute.name];
            tag = StringAssets[transAttribute.tag];
            point += ElementAttribute.Size;
            //textAttribute = *(TextAttribute*)point;
            TextAttribute.LoadFromBuff(ref textAttribute,point);
            if (textAttribute.material > -1)
            {
                smat = StringAssets[textAttribute.material];
                shader = StringAssets[textAttribute.shader];
            }
            if (textAttribute.font > -1)
                fontName = StringAssets[textAttribute.font];
            if (textAttribute.text > -1)
                text = StringAssets[textAttribute.text];
            return point + TextAttribute.Size;
        }
        public unsafe override byte[] ToBytes()
        {
            int size = ElementAttribute.Size;
            int tsize = TextAttribute.Size;
            byte[] buff = new byte[size + tsize];
            fixed (byte* bp = &buff[0])
            {
                *(ElementAttribute*)bp = transAttribute;
                byte* a = bp+ size;
                *(TextAttribute*)a = textAttribute;
            }
            return buff;
        }
        static void Load(GameObject tar, ref TextAttribute att)
        {
            var a = tar.GetComponent<Text>();
            a.alignByGeometry = att.alignByGeometry;
            a.alignment = att.alignment;
            a.fontSize = att.fontSize;
            a.fontStyle = att.fontStyle;
            a.horizontalOverflow = att.horizontalOverflow;
            a.lineSpacing = att.lineSpacing;
            a.resizeTextForBestFit = att.resizeTextForBestFit;
            a.resizeTextMaxSize = att.resizeTextMaxSize;
            a.resizeTextMinSize = att.resizeTextMinSize;
            a.supportRichText = att.supportRichText;
            a.verticalOverflow = att.verticalOverflow;
            a.color = att.color;
            a.raycastTarget = false;
            a.enabled = true;
        }
        static void Save(GameObject tar, ref TextAttribute att)
        {
            var txt = tar.GetComponent<Text>();
            if (txt != null)
            {
                att.alignByGeometry = txt.alignByGeometry;
                att.alignment = txt.alignment;
                att.fontSize = txt.fontSize;
                att.fontStyle = txt.fontStyle;
                att.horizontalOverflow = txt.horizontalOverflow;
                att.lineSpacing = txt.lineSpacing;
                att.resizeTextForBestFit = txt.resizeTextForBestFit;
                att.resizeTextMaxSize = txt.resizeTextMaxSize;
                att.resizeTextMinSize = txt.resizeTextMinSize;
                att.supportRichText = txt.supportRichText;
                att.verticalOverflow = txt.verticalOverflow;
                att.color = txt.color;
                att.text = SaveString(txt.text);
                var mat = txt.material;
                att.material = SaveString(mat.name);
                att.shader = SaveString(mat.shader.name);
                if (txt.font != null)
                    att.font = SaveString(txt.font.name);
                else att.font = -1;
            }
        }
        public override void Load(GameObject tar)
        {
            base.Load(tar);
            Load(tar, ref this.textAttribute);
            var txt = tar.GetComponent<Text>();
            if (smat != null)
                if (smat != "Default UI Material")
                    txt.material = new Material(Shader.Find(shader));
            txt.font = FindFont(fontName);
            txt.text = text;
        }
        public override void Save(GameObject tar)
        {
            base.Save(tar);
            Save(tar, ref this.textAttribute);
        }
    }
}
