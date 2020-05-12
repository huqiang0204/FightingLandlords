using huqiang.Core.HGUI;
using System;
using UnityEngine;
using huqiang.UIComposite;
using huqiang.UIEvent;
using huqiang;
using huqiang.Data;
using System.IO;
using System.Collections.Generic;

public class LoadingPage : UIPage
{

    //反射UI界面上的物体
    class View
    {
        public HImage FillImage;
        public HImage Nob;
        public HText Tip;
    }
    View view;
    AsyncOperation operation;

    public override void Initial(Transform parent, object dat = null)
    {
        base.Initial(parent, dat);
        view = LoadUI<View>("baseUI", "Loading");//"baseUI"创建的bytes文件名,"page"为创建的页面名
        view.FillImage.SizeChanged = (o) => { SetProgress(fill); };
        DownloadVersion();
    }
    class Version
    {
        public string name;
        public string num;
    }
    void DownloadVersion()
    {
        view.Tip.Text = "检测服务器版本";
        OssMission mission = new OssMission();
        ossMissions.Add(mission);
        mission.dir = Application.persistentDataPath + "/";
        mission.Name = "versions.ini";
        mission.Tip = (o) => {
            view.Tip.Text = "检测资源版本";
            SetProgress(o.Progress);
        };
        mission.Completed = (o) => {
            OssMission oss = o as OssMission;
            if (o.Done)
            {
                VersionCheck();
            }
            else
            {
                view.Tip.Text = "未能找到服务器";
            }
            ossMissions.Remove(oss);
        };
        mission.Run();
        mission = new OssMission();
        ossMissions.Add(mission);
        mission.dir = Application.persistentDataPath + "/";
        mission.Name = "HotFix.dll";
        mission.Completed = (o) => { ossMissions.Remove(o as OssMission); };
        mission.Run();
        mission = new OssMission();
        ossMissions.Add(mission);
        mission.dir = Application.persistentDataPath + "/";
        mission.Name = "baseUI.bytes";
        mission.Completed = (o) => { ossMissions.Remove(o as OssMission); };
        mission.Run();
    }
    void VersionCheck()
    {
        string dic = Application.persistentDataPath + "/";
        string tempath = dic + "versions.ini";
        INIReader ini = new INIReader();
        ini.LoadFromFile(tempath);
        string key = "win";
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            key = "ios";
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            key = "and";
        }
        var sec = ini.FindSection(key);
        var values = sec.values;
        for (int i = 0; i < values.Count; i++)
        {
            var kv = values[i];
            string value = PlayerPrefs.GetString(kv.key);
            string path = dic + kv.key;
            if (value != kv.value | !File.Exists(path))
            {
                OssMission mission = new OssMission();
                mission.dir = Application.persistentDataPath + "/";
                mission.Name = kv.key;
                mission.Version = kv.value;
                ossMissions.Add(mission);
                mission.Tip = (o) =>
                {
                    view.Tip.Text = "正在下载资源:" + o.Name;
                    SetProgress(o.Progress);
                };
                mission.Completed = (o) =>
                {
                    OssMission oss = o as OssMission;
                    ossMissions.Remove(oss);
                    PlayerPrefs.SetString(oss.Name, oss.Version);
                    if (oss.Name.Contains(".lzma"))
                    {
                        if (oss.Done)
                        {
                            DecompressLZMA(oss.Name, oss.Version);
                        }
                    }
                };
            }
            else if (kv.key.Contains(".lzma"))
            {
                string str = kv.key;
                str = str.Replace(".lzma", "");
                value = PlayerPrefs.GetString(str);
                if (value != kv.value)
                {
                    DecompressLZMA(kv.key, kv.value);
                }
            }
        }
    }
    float fill;
    void SetProgress(float r)
    {
        fill = r;
        view.FillImage.FillAmount = r;
        float a = view.FillImage.SizeDelta.x;
        view.Nob.transform.localPosition = new Vector3((r - 0.5f) * a, 0, 0);
        float t = 75 + (140 - 75) * (1 - r);
        var col = Color.HSVToRGB(t / 255, 1, 1);
        view.Nob.MainColor = col;
    }
    public override void Update(float time)
    {
        if (ossMissions.Count > 0)
        {
            if (ossMissions[0].Tip != null)
                ossMissions[0].Tip(ossMissions[0]);
            if (!ossMissions[0].Running)
                ossMissions[0].Run();
        }
        else if (LMissions.Count > 0)
        {
            if (LMissions[0].Tip != null)
                LMissions[0].Tip(LMissions[0]);
        }
        int max = 3;
        if (LMissions.Count < max)
            max = LMissions.Count;
        for (int i = 0; i < max; i++)
        {
            if (!LMissions[i].Running)
            {
                LMissions[i].Run();
            }
        }
        if (ossMissions.Count == 0 && LMissions.Count == 0)
        {
            LoadPage<HotFixPage>();
        }
    }
    void DecompressLZMA(string name, string version)
    {
        LZMAMission lz = new LZMAMission();
        string save = name.Replace(".lzma", "");
        string dic = Application.persistentDataPath + "/";
        lz.Name = save;
        lz.Version = version;
        lz.filePath = dic + name;
        lz.savePath = dic + save;
        lz.Tip = (o) => {
            view.Tip.Text = "解压缩资源:" + o.Name;
            SetProgress(o.Progress);
        };
        lz.Completed = (o) => {
            LMissions.Remove(o as LZMAMission);
            PlayerPrefs.SetString(o.Name, o.Version);
        };
        LMissions.Add(lz);
    }
    List<OssMission> ossMissions = new List<OssMission>();
    List<LZMAMission> LMissions = new List<LZMAMission>();
}
