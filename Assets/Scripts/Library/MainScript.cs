using huqiang;
using huqiang.Data;
using huqiang.Core.HGUI;
using System;
using System.IO;
using UnityEngine;

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
        ElementAsset.LoadAssetsAsync("base.unity3d").PlayOver = (o, e) =>
        {
            UIPage.LoadPage<LoadingPage>("checkOss");
        };
    }
    public bool Pause;
    private void OnApplicationQuit()
    {
        base.OnDestroy();
        App.Dispose();
    }
}
