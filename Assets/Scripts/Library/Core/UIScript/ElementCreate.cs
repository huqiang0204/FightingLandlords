using huqiang.UIModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ElementCreate : MonoBehaviour {
    public TextAsset bytesUI;
    public void ClearAllAssetBundle()
    {
        AssetBundle.UnloadAllAssetBundles(true);
        ElementAsset.bundles.Clear();
    }
    void LoadBundle()
    {
        if (ElementAsset.bundles.Count == 0)
        {
            var dic = Application.dataPath + "/StreamingAssets";
            if (Directory.Exists(dic))
            {
                var bs = Directory.GetFiles(dic, "*.unity3d");
                for (int i = 0; i < bs.Length; i++)
                {
                    ElementAsset.bundles.Add(AssetBundle.LoadFromFile(bs[i]));
                }
            }
        }

    }
    public string dicpath;
    public string Assetname = "prefabs";
    public void Create()
    {
        if (Assetname == null)
            return;
        if (Assetname == "")
            return;
        LoadBundle();
        Assetname = Assetname.Replace(" ", "");
        ModelManager.Initial();
        var dc = dicpath;
        if (dc == null | dc == "")
        {
            dc = Application.dataPath + "/AssetsBundle/";
        }
        ModelManager.SavePrefab(gameObject, dc + Assetname);
        Debug.Log("create done");
    }
    public string CloneName;
    public void Clone()
    {
        if (bytesUI != null)
        {
            if (CloneName != null)
                if (CloneName != "")
                {
                    LoadBundle();
                    ModelManager.Initial();
                    ModelManager.LoadModels(bytesUI.bytes, "assTest");
                    ModelManager.LoadToGame(CloneName, null, transform, "");
                }
        }
    }
    public void CloneAll()
    {
        if (bytesUI != null)
        {
            if (CloneName != null)
                if (CloneName != "")
                {
                    LoadBundle();
                    ModelManager.Initial();
                    var all = ModelManager.LoadModels(bytesUI.bytes, "assTest");
                    var models = all.models;
                    for (int i = 0; i < models.Length; i++)
                        ModelManager.LoadToGame(models[i], null, transform, "");
                }
        }
    }
}
