using Assets.Scripts.PageControll;
using huqiang.Data;
using huqiang.UIModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPage:Page
{
    class LoadingView
    {
        public Text tips;
    }
    LoadingView view;
    public override void Initial(Transform parent, object dat = null)
    {
        base.Initial(parent, dat);
        view = new LoadingView();
        model = ModelManager.LoadToGame(StringAssets.LoadingPage, view, parent);
        main = model.Main;

        InitialUI();
    }
    public override void Update(float time)
    {
        base.Update(time);
    }

    public override void Cmd(DataBuffer dat)
    {
        var fake = dat.fakeStruct;
        var cmd = fake[Req.Cmd] ;
        switch (cmd)
        {
            case 0:
                CheckVersions(fake[Req.Args]);
                break;
            case 1:
                Debug.Log(fake.GetData(Req.Args) as string);
                DownLoad(fake.GetData(Req.Args) as string, fake.GetData(Req.Length) as string);
                break;
            default:
                break;
        }
    }

    public void InitialUI()
    {
        view.tips.text = "正在连接服务器。。。。。。";
    }

    int Versions;

    public void CheckVersions(int vers)
    {
        Versions = vers;
        int v = LocalFileManager.GetBundleVersion("HotfixDll");
        if (v < vers)
        {
            DataBuffer db = new DataBuffer();
            var fake = new FakeStruct(db, Req.Length);
            fake[Req.Cmd] = 1;
            fake[Req.Type] = MessageType.Def;
            db.fakeStruct = fake;
            TcpDataControll.Instance.SendAesStream(db);
        }
        else
        {
#if UNITY_EDITOR
            var fs = File.Open("", FileMode.Open);
            byte[] buf = new byte[fs.Length];
            fs.Read(buf, 0, buf.Length);
            fs.Dispose();
            ModelManager.LoadModels(buf, "hotui");
            LoadPage<HotFixPageEntry>();
#else
            dll = LocalFileManager.LoadFile("HotfixDll");
            ui = LocalFileManager.LoadFile("HotfixUI");
            dll = AES.Decrypt(dll, "C31838BAFD614E77BF61A8DB37E1244E", "21F4FCB1B5EA43D7");
            ModelManager.LoadModels(ui, "hotui");
            LoadPage<HotFixPageEntry>(dll);
#endif
        }    
    }

    public void DownLoad(string url, string uiurl)
    {
        DownloadManager.DownloadAsset("dll", "HotfixDll", url, null, DownLoadCallBack, Versions);
        DownloadManager.DownloadAsset("ui", "HotfixUI", uiurl, null, DownLoadCallBack, Versions);
    }

    byte[] dll, ui;
    public void DownLoadCallBack(DownLoadMission mission)
    {
        if (mission.cmd == "dll")
        {
            dll = mission.result;
            LocalFileManager.SaveAssetBundle("HotfixDll", Versions, mission.result);
            if (ui != null)
            {
                dll= AES.Decrypt(dll, "C31838BAFD614E77BF61A8DB37E1244E", "21F4FCB1B5EA43D7");
                //ui = AES.Decrypt(ui, "C31838BAFD614E77BF61A8DB37E1244E", "21F4FCB1B5EA43D7");
                ModelManager.LoadModels(ui,"hotui");
                LoadPage<HotFixPageEntry>(dll);
            }
        }
        else
        {
            ui = mission.result;
            LocalFileManager.SaveAssetBundle("HotfixUI", Versions, mission.result);
            if (dll != null)
            {
                dll = AES.Decrypt(dll, "C31838BAFD614E77BF61A8DB37E1244E", "21F4FCB1B5EA43D7");
                //ui = AES.Decrypt(ui, "C31838BAFD614E77BF61A8DB37E1244E", "21F4FCB1B5EA43D7");
                ModelManager.LoadModels(ui, "hotui");
                LoadPage<HotFixPageEntry>(dll);
            }
        }
    }
}
