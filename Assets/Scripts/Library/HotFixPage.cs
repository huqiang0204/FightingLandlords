﻿using Assets.Game.HotFix;
using huqiang.Core.HGUI;
using huqiang.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class HotFixPage:UIPage
{
    public override void Initial(Transform parent, object dat = null)
    {
        base.Initial(parent, dat);
        string dic = Application.persistentDataPath + "/";
        string path = dic + "baseUI.bytes";
        if(File.Exists(path))
        {
            byte[] ui = File.ReadAllBytes(dic + "baseUI.bytes");
            HGUIManager.LoadModels(ui, "baseUI");
        }
        string dll = dic + "HotFix.dll";
        string pdb = dic + "HotFix.dll";
        HotFix.Instance.Load(File.ReadAllBytes(dll));
        HotFix.Instance.Start(Parent, "Start", dat);
    }
    public override void Show(object dat = null)
    {
        HotFix.Instance.Start(Parent, "Start", dat);
    }
    public override void Update(float time)
    {
        HotFix.Instance.RuntimeUpdate(time);
    }
    public override void Cmd(string cmd, object dat)
    {
        HotFix.Instance.RuntimeCmd(cmd, dat as string);
    }
    public override void Cmd(DataBuffer dat)
    {
        HotFix.Instance.RuntimeFullCmd(dat);
    }
    public override void ReSize()
    {
        HotFix.Instance.RuntimeReSize();
    }
    public override void Dispose()
    {
        HotFix.Instance.RuntimeDispose();
    }
}

