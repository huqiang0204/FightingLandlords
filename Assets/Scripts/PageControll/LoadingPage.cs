using Assets.Scripts.PageControll;
using huqiang.Data;
using huqiang.UIModel;
using System;
using System.Collections.Generic;
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
                DownLoad(fake.GetData(Req.Args) as string);
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
        int v = LocalFileManager.GetBundleVersion("ModuleHotdixDev");
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
            LoadPage<HotFixPageEntry>(LocalFileManager.LoadFile("ModuleHotdixDev"));
        }  
    }

    public void DownLoad(string url)
    {
        DownloadManager.DownloadAsset("dll", "ModuleHotdixDev", url, null, DownLoadCallBack, Versions);
    }

    public void DownLoadCallBack(DownLoadMission mission)
    {
        LocalFileManager.SaveAssetBundle("ModuleHotdixDev", Versions, mission.result);
        LoadPage<HotFixPageEntry>(mission.result);
    }
}
