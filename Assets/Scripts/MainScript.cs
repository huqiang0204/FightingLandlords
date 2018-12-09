using UnityEngine;
using huqiang.UIModel;
using huqiang;
using System;
using System.Collections;
using System.Collections.Generic;
using huqiang.Data;

public class MainScript : MonoBehaviour {
    public static string NewVersion="1.0.11";
    public static string CurVersion = "1.0.11";
    public static bool NeedUpdate()
    {
        //if (NewVersion != CurVersion)
        //    return true;
        //else return false;
        var nv = NewVersion.Split('.');
        var cur = CurVersion.Split('.');
        for (int i = 0; i < nv.Length; i++)
        {
            int a = 0;
            int.TryParse(nv[i], out a);
            int b = 0;
            int.TryParse(cur[i], out b);
            if (a > b)
                return true;
        }
        return false;
    }
    public static bool IsPortrait()
    {
        if (Screen.height > Screen.width)
            return true;
        else return false;
    }
    public TextAsset baseUI;
    //public MovieTexture cg;
    public static MainScript Instance;
    public float AllTime=0;
    public RectTransform uiRoot;
    public RectTransform buffRoot;
	// Use this for initialization
	void Awake () {
        App.InitialBase();
        App.InitialScene(uiRoot,buffRoot);
        ModelManager.LoadModels(baseUI.bytes,"baseUI");
        Page.LoadPage<LoadingPage>();
        TcpDataControll.Instance.Connection("192.168.31.34",6666);
    }
    // Update is called once per frame
    void Update () {
        //UdpDataControll.Instance.DispatchMessage();
        TcpDataControll.Instance.DispatchMessage();
        App.Update();
    }
    /// <summary>
    /// 执行协程任务
    /// </summary>
    /// <param name="action"></param>
    public void AddCoProcess(Func<object, IEnumerator> action,object obj=null)
    {
        StartCoroutine(action(obj));
    }
    public void StopCoProcess(Func<object, IEnumerator> action, object obj = null)
    {
        StopCoroutine(action(obj));
    }
    /// <summary>
    /// 播放音频
    /// </summary>
    /// <param name="clip"></param>
    public void PlaySound(AudioClip clip)
    {
        mainAudio.clip = clip;
        mainAudio.Play();
    }
    public void EndSound()
    {
        mainAudio.Stop();
        mainAudio.clip = null;
    }
    private void OnApplicationQuit()
    {
        //UdpDataControll.Instance.Close();
        TcpDataControll.Instance.Close();
        App.Dispose();
    }
    bool rtc;
    public bool RealTimeChat { get { return rtc; }
        set { rtc = value;
            if (!rtc)
                for (int i = 0; i < maxNum; i++)
                {
                    ad[i].uid = 0;
                    ad[i].data.Clear();
                }
        } }
    AudioSource mainAudio;
    AudioSource[] audios;
    struct Sound
    {
        public int uid;
        public List<float[]> data;
    }
    Sound[] ad;
    int maxNum = 6;
    void InitialAudios()
    {
        mainAudio = gameObject.AddComponent<AudioSource>();
        ad = new Sound[maxNum];
        audios = new AudioSource[maxNum];
        for (int i = 0; i < maxNum; i++)
        {
            audios[i] = gameObject.AddComponent<AudioSource>();
            ad[i].data = new List<float[]>();
        } 
    }
    int GetIndex(int uid)
    {
        for(int i=0;i<maxNum;i++)
        {
            if (ad[i].uid == uid)
                return i;
        }
        for (int i = 0; i < maxNum; i++)
        {
            if (ad[i].uid == 0)
            {
                ad[i].uid = uid;
                return i;
            }
        }
        return -1;
    }
    public void RemovePlayer(int uid)
    {
        for(int i=0;i<maxNum;i++)
        {
            if (ad[i].uid == uid)
            {
                ad[i].uid = 0;
                return;
            }
        }
    }
}
