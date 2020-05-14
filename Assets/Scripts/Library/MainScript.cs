using huqiang;
using huqiang.Data;
using huqiang.Core.HGUI;
using System;
using System.IO;
using UnityEngine;
using DataControll;

public class MainScript : HCanvas
{
    public static MainScript Instance;
    public TextAsset baseUI;
    protected override void Start()
    {
        base.Start();
        Instance = this;
        Scale.DpiScale = true;
        App.Initial(transform);
        DontDestroyOnLoad(gameObject);
        HGUIManager.LoadModels(baseUI.bytes, "baseUI");
#if UNITY_EDITOR
        AssetBundle.UnloadAllAssetBundles(true);
#endif
        //ElementAsset.LoadAssetsAsync("base.unity3d").PlayOver = (o, e) =>
        //{
            UIPage.LoadPage<LoadingPage>("checkOss");
        // };
        KcpDataControll.Instance.Connection("193.112.70.170",8899);
    }
    public bool Pause;
    protected override void Update()
    {
        base.Update();
        KcpDataControll.Instance.DispatchMessage();
    }
    private void OnApplicationQuit()
    {
        base.OnDestroy();
        App.Dispose();
        KcpDataControll.Instance.Close();
    }
}
