using Assets.Scripts.PageControll;
using huqiang;
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
        public EventCallBack bg;
        public Text tips;
        public TreeView tree;
    }
    LoadingView view;
    public override void Initial(Transform parent, object dat = null)
    {
        base.Initial(parent, dat);
        view = new LoadingView();
        model = ModelManager.LoadToGame(StringAssets.LoadingPage, view, parent);
        main = model.Main;

        InitialUI();
        TreeViewNode node = new TreeViewNode();
        node.content = "test";
        for(int i=0;i<6;i++)
        {
            var t = new TreeViewNode();
            t.content = "yyy"+i;
            node.child.Add(t);
            for(int j=0;j<7;j++)
            {
                var u = new TreeViewNode();
                u.content = "xxx" + j;
                t.child.Add(u);
                for (int d= 0; d < 7; d++)
                {
                    var p= new TreeViewNode();
                    p.content = "oooo" + d;
                    u.child.Add(p);
                    for (int a = 0; a < 7; a++)
                    {
                        var l = new TreeViewNode();
                        l.content = "eeee" + a;
                        p.child.Add(l);
                    }
                }
            }
        }
        view.tree.nodes = node;
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
        view.bg.Click = (o, e) => {
            //view.tips.color = Color.white;
            //view.tips.ColorTo(Color.red,3000);
            //view.tips.transform.localPosition = new Vector3(200,300,0);
            //view.tips.transform.MoveTo(new Vector3(-200,-200,0),1000,800);
            //view.tips.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
            //view.tips.transform.ScaleTo(new Vector3(2,2,2),1000,800);
            //view.tips.transform.RotateTo(new Vector3(0,360,0),800);
        };
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
            KcpDataControll.Instance.SendAesStream(db);
        }
        else
        {
#if UNITY_EDITOR
            string path=  Application.dataPath+ "/AssetsBundle/HotUI.bytes";
            var fs = File.Open(path, FileMode.Open);
            byte[] buf = new byte[fs.Length];
            fs.Read(buf, 0, buf.Length);
            fs.Dispose();
            ModelManager.LoadModels(buf, "hotui");

            fs = File.Open(@"F:\SelfWork\HotFixGame\HotFixGame\bin\Debug\HotFixGame.dll", FileMode.Open);
            byte[] dll = new byte[fs.Length];
            fs.Read(dll, 0, dll.Length);
            fs.Dispose();

            LoadPage<HotFixPageEntry>(dll);
#else
            dll = LocalFileManager.LoadAssetBundle("HotfixDll");
            ui = LocalFileManager.LoadAssetBundle("HotfixUI");
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
