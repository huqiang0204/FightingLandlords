using huqiang.Data;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace huqiang.UI
{
    public class PrefabAsset
    {
        public string name;
        public UIElement models;
    }
    public class ModelManagerUI
    {
        struct TypeContext
        {
            public Type type;
            public Func<Component, bool> Compare;
            public Func<DataConversion> CreateConversion;
            public Func<Component, DataBuffer, FakeStruct> LoadFromObject;
        }
        static int point;
        /// <summary>
        /// 0-62,第63为负数位
        /// </summary>
        static TypeContext[] types = new TypeContext[63];
        /// <summary>
        /// 注册一个组件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="Compare">不可空</param>
        /// <param name="create">可空</param>
        /// <param name="load">可空</param>
        public static void RegComponent(Type type, Func<Component, bool> Compare, Func<DataConversion> create, Func<Component, DataBuffer, FakeStruct> load)
        {
            if (point >= 63)
                return;
            for (int i = 0; i < point; i++)
                if (types[i].type == type)
                {
                    types[i].Compare = Compare;
                    types[i].CreateConversion = create;
                    types[i].LoadFromObject = load;
                    return;
                }
            types[point].type = type;
            types[point].Compare = Compare;
            types[point].CreateConversion = create;
            types[point].LoadFromObject = load;
            point++;
        }
        public static Int64 GetTypeIndex(Component com)
        {
            for (int i = 0; i < point; i++)
            {
                if (types[i].Compare(com))
                {
                    Int64 a = 0 << i;
                    return a;
                }
            }
            return -1;
        }
        public static Int64 GetTypeIndex(Component[] com)
        {
            if (com == null)
                return 0;
            Int64 a = 0;
            for (int i = 0; i < com.Length; i++)
            {
                var c = com[i];
                if (c != null)
                    a |= GetTypeIndex(c);
            }
            return a;
        }
        static int GetTypeIndex(Type type)
        {
            for (int i = 1; i < point; i++)
            {
                if (type == types[i].type)
                {
                    int a = 1 << i;
                    return a;
                }
            }
            return -1;
        }
        /// <summary>
        /// 根据所有类型生成一个id
        /// </summary>
        /// <param name="typ"></param>
        /// <returns></returns>
        public static int GetTypeIndex(Type[] typ)
        {
            if (typ == null)
                return -1;
            int a = 0;
            for (int i = 0; i < typ.Length; i++)
                a |= GetTypeIndex(typ[i]);
            return a;
        }

        public void Initial()
        {
        }
        static List<ModelBuffer> models = new List<ModelBuffer>();
        /// <summary>
        /// 注册一种模型的管理池
        /// </summary>
        /// <param name="reset">模型被重复利用时,进行重置,为空则不重置</param>
        /// <param name="buffsize">池子大小,建议32</param>
        /// <param name="types">所有的Component组件</param>
        public static void RegModel(Action<GameObject> reset, int buffsize, params Type[] types)
        {
            int typ = GetTypeIndex(types);
            for (int i = 0; i < models.Count; i++)
            {
                if (typ == models[i].type)
                    return;
            }
            ModelBuffer model = new ModelBuffer(typ, buffsize, reset, types);
            models.Add(model);
        }
        public static DataConversion Load(int type)
        {
            if (type < 0 | type >= point)
                return null;
            if (types[type].CreateConversion != null)
                return types[type].CreateConversion();
            return null;
        }
        public static FakeStruct LoadFromObject(Component com, DataBuffer buffer, ref Int16 type)
        {
            for (int i = 0; i < point; i++)
                if (types[i].Compare(com))
                {
                    type = (Int16)i;
                    if (types[i].LoadFromObject != null)
                        return types[i].LoadFromObject(com, buffer);
                    return null;
                }
            return null;
        }
        /// <summary>
        /// 将场景内的对象保存到文件
        /// </summary>
        /// <param name="uiRoot"></param>
        /// <param name="path"></param>
        public static void SavePrefab(GameObject uiRoot, string path)
        {
            DataBuffer db = new DataBuffer(1024);
            db.fakeStruct = UIElement.LoadFromObject(uiRoot.transform, db);
            File.WriteAllBytes(path, db.ToBytes());
        }
        static List<PrefabAsset> prefabAssets = new List<PrefabAsset>();
        public unsafe static PrefabAsset LoadModels(byte[] buff, string name)
        {
            DataBuffer db = new DataBuffer(buff);
            UIElement model = new UIElement();
            model.Load(db.fakeStruct);
            var asset = new PrefabAsset();
            asset.models = model;
            asset.name = name;
            for (int i = 0; i < prefabAssets.Count; i++)
                if (prefabAssets[i].name == name)
                { prefabAssets.RemoveAt(i); break; }
            prefabAssets.Add(asset);
            return asset;
        }
        /// <summary>
        /// 查询一个模型数据,并实例化对象
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="mod"></param>
        /// <param name="o"></param>
        /// <param name="parent"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static UIElement LoadToGame(string asset, string mod, object o, Transform parent, string filter = "mod")
        {
            for (int i = 0; i < prefabAssets.Count; i++)
            {
                if (asset == prefabAssets[i].name)
                {
                    var ms = prefabAssets[i].models.child;
                    for (int j = 0; j < ms.Count; j++)
                    {
                        if (ms[j].name == mod)
                        {
                            LoadToGame(ms[j], o, parent, filter);
                            return ms[j];
                        }
                    }
                    return null;
                }
            }
            return null;
        }
        /// <summary>
        /// 使用模型数据,实例化对象
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="o"></param>
        /// <param name="parent"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static GameObject LoadToGame(UIElement mod, object o, Transform parent, string filter = "mod")
        {
            if (mod == null)
            {
#if DEBUG
                Debug.Log("Mod is null");
#endif
                return null;
            }
            if (mod.tag == filter)
            {
                return null;
            }
            var g = CreateNew(mod.data.type);
            if (g == null)
            {
#if DEBUG
                Debug.Log("Name:" + mod.name + " is null");
#endif
                return null;
            }
            var t = g.transform;
            if (parent != null)
                t.SetParent(parent);
            mod.LoadToObject(g.transform);
            mod.Main = g;
            var c = mod.child;
            for (int i = 0; i < c.Count; i++)
                LoadToGame(c[i], o, t, filter);
            if (o != null)
                ReflectionObject(t, o, mod);
            return g;
        }
        static void ReflectionObject(Transform t, object o, UIElement mod)
        {
            var m = o.GetType().GetField(t.name);
            if (m != null)
            {
                if (typeof(Component).IsAssignableFrom(m.FieldType))
                    m.SetValue(o, t.GetComponent(m.FieldType));
            }
        }

        #region 创建和回收
        public static GameObject CreateNew(params Type[] types)
        {
            return CreateNew(GetTypeIndex(types));
        }
        public static GameObject CreateNew(Int64 type)
        {
            if (type == 0)
                return null;
            for (int i = 0; i < models.Count; i++)
                if (type == models[i].type)
                    return models[i].CreateObject();
            return null;
        }
        /// <summary>
        /// 挂载被回收得对象
        /// </summary>
        public static Transform CycleBuffer;
        /// <summary>
        /// 回收一个对象，包括子对象
        /// </summary>
        /// <param name="game"></param>
        public static void RecycleGameObject(GameObject game)
        {
            if (game == null)
                return;
            var rect = game.GetComponent<RectTransform>();
            if (rect != null)
                EventCallBack.ReleaseEvent(rect);
            var ts = game.GetComponents<Component>();
            Int64 type = GetTypeIndex(ts);
            if (type > 0)
            {
                for (int i = 0; i < models.Count; i++)
                {
                    if (models[i].type == type)
                    {
                        models[i].ReCycle(game);
                        break;
                    }
                }
            }
            var p = game.transform;
            for (int i = p.childCount - 1; i >= 0; i--)
                RecycleGameObject(p.GetChild(i).gameObject);
            if (type > 0)
                p.SetParent(CycleBuffer);
            else GameObject.Destroy(game);

        }
        public static void RecycleSonObject(GameObject game)
        {
            if (game == null)
                return;
            var p = game.transform;
            for (int i = p.childCount - 1; i >= 0; i--)
                RecycleGameObject(p.GetChild(i).gameObject);
        }
        #endregion
    }
}
