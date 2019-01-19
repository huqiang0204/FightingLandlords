using huqiang.Data;
using huqiang.UIModel;
using UnityEngine;

namespace huqiang
{
    public class App
    {
        public static void InitialBase()
        {
#if UNITY_EDITOR||UNITY_STANDALONE_WIN
            IME.Initial();
#endif
            MathH.Inital();
            ThreadPool.Initial();
            TextElement.fonts.Add(Font.CreateDynamicFontFromOSFont("Arial", 16));
            ModelManager.Initial();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            UserAction.inputType = UserAction.InputType.Blend;
#elif UNITY_IPHONE || UNITY_ANDROID
            UserAction.inputType = UserAction.InputType.OnlyTouch;
#endif
        }
        static RectTransform UIRoot;
        public static void InitialScene(RectTransform uiRoot)
        {
            UIRoot = uiRoot;
            Page.Root = uiRoot;
            var buff= new GameObject("buffer",typeof(Canvas));
            buff.SetActive(false);
            ModelManager.CycleBuffer = buff.transform;
            EventCallBack.MinBox = new Vector2(80, 80);
            EventCallBack.InsertRoot(uiRoot.root as RectTransform);
        }
        public static float AllTime;
        public static void Update()
        {
            AnimationManage.Manage.Update();
            UserAction.DispatchEvent();
            ThreadPool.ExtcuteMain();
            Resize();
            Page.Refresh(UserAction.TimeSlice);
            AllTime += Time.deltaTime;
            DownloadManager.UpdateMission();
            Resources.UnloadUnusedAssets();
        }
        static void Resize()
        {
            float w = Screen.width;
            float h = Screen.height;
            float s = Scale.ScreenScale;
            UIRoot.localScale = new Vector3(s, s, s);
            w /= s;
            h /= s;
            if (Scale.ScreenWidth != w | Scale.ScreenHeight != h)
            {
                Scale.ScreenWidth = w;
                Scale.ScreenHeight = h;
                UIRoot.sizeDelta = new Vector2(w, h);
                if (Page.CurrentPage != null)
                    Page.CurrentPage.ReSize();
            }
        }
        public static void Dispose()
        {
            EventCallBack.ClearEvent();
            ThreadPool.Dispose();
            RecordManager.ReleaseAll();
        }
    }
}
