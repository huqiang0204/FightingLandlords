using huqiang.Core.HGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Language
{
    class Lan
    {
        public string key;
        public string value;
    }
    static Dictionary<string, List<Lan>> dic;
    static string Current="cn";
    public static void LoadLanguage(string lan)
    {
        if (Current == lan)
            return;
        var txt = Resources.Load<TextAsset>("/lan/"+lan);
        if (txt != null)
            Analytical(txt.text);
        if (UIPage.CurrentPage != null)
            UIPage.CurrentPage.ChangeLanguage();
    }
    static void Analytical(string text)
    {
        if (dic == null)
            dic = new Dictionary<string, List<Lan>>();
        else dic.Clear();
        var ls= text.Split('\r','\n');
        List<Lan> vs = null;
        for (int i = 0; i < ls.Length; i++)
        {
            if (ls[i] != null)
            {
                if (ls[i].Length > 0)
                {
                    if (ls[i][0] == '[')
                    {
                        vs = new List<Lan>();
                        dic.Add(ls[i].Substring(1, ls[i].Length - 2), vs);
                    }
                    else
                    {
                        if (vs != null)
                        {
                            var ss= ls[i].Split('=');
                            if(ss.Length>1)
                            {
                                Lan l= new Lan();
                                l.key = ss[0];
                                l.value = ss[1];
                                vs.Add(l);
                            }
                        }
                    }
                }
            }
        }
    }
    public bool IsChanged(string lan)
    {
        if (lan != Current)
            return true;
        return false;
    }
    static List<Lan> CurrentPage;
    public static void LoadLanguage(Transform transform,string page)
    {
        if(dic.ContainsKey(page))
        {
            CurrentPage = dic[page];
            LoadLanguage(transform);
        }
    }
    public static string GetContent(string str,string page)
    {
        if (dic.ContainsKey(page))
        {
            var t = dic[page];
            for (int i = 0; i < t.Count; i++)
                if (t[i].key == str)
                    return t[i].value;
        }
        return "";
    }
    static string GetContent(string str)
    {
        if(CurrentPage==null)
        return "";
        for (int i = 0; i < CurrentPage.Count; i++)
            if (CurrentPage[i].key == str)
                return CurrentPage[i].value;
        return "";
    }
    static void LoadLanguage(Transform transform)
    {
        var txt = transform.GetComponent<HText>();
        if (txt != null)
        {
            txt.Text = GetContent(txt.name);
        }
        var c = transform.childCount;
        for(int i = 0; i < c; i++)
        {
            LoadLanguage(transform.GetChild(i));
        }
    }
}