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
        mission.Name = "versions.txt";
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
        mission.Completed = (o) => {ossMissions.Remove(o as OssMission); };
        mission.Run();
        mission = new OssMission();
        ossMissions.Add(mission);
        mission.dir = Application.persistentDataPath + "/";
        mission.Name = "baseUI.bytes";
        mission.Completed = (o) => { ossMissions.Remove(o as OssMission);};
        mission.Run();
    }
    void VersionCheck()
    {
        string dic = Application.persistentDataPath + "/";
        string tempath = dic + "versions.txt";
        string[] temversions = File.ReadAllLines(tempath);
        for (int i = 0; i < temversions.Length; i++)
        {
            string[] ss = temversions[i].Split('-');
            if (ss.Length > 1)
            {
                string value = PlayerPrefs.GetString(ss[0]);
                string path = dic + ss[0];
                if (value != ss[1] | !File.Exists(path))
                {
                    OssMission mission = new OssMission();
                    mission.dir = Application.persistentDataPath + "/";
                    mission.Name = ss[0];
                    mission.Version = ss[1];
                    ossMissions.Add(mission);
                    mission.Tip = (o) => {
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
                else if (ss[0].Contains(".lzma"))
                {
                    string key = ss[0];
                    key = key.Replace(".lzma", "");
                    value = PlayerPrefs.GetString(key);
                    if (value != ss[1])
                    {
                        DecompressLZMA(ss[0], ss[1]);
                    }
                }
            }
        }
    }
    float fill;
    bool loadingScene;
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
            if (request == null)
            {
                AsyncLoading();
            }
        }
    }
    Action<AsyncOperation> Complete;
    AssetBundleCreateRequest request;
    void AsyncLoading()
    {
        view.Tip.Text = "加载资源 :";
        string scePath = Application.persistentDataPath + "/base.unity3d";
        request = AssetBundle.LoadFromFileAsync(scePath);
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
