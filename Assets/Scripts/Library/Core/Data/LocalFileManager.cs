using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace huqiang.Data
{
    public class LocalFileManager
    {
        static string persistentDataPath = Application.persistentDataPath;
        static FileStream CreateFile(string type, string name)
        {
            string path = persistentDataPath + "/data";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path += "/" + type;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path += "/" + name;
            if (File.Exists(path))
                File.Delete(path);
            return File.Create(path);
        }
        static FileStream OpenFile(string type, string name)
        {
            string path = persistentDataPath + "/data";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path += "/" + type;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path += "/" + name;
            var file = File.Open(path, FileMode.OpenOrCreate);
            file.Seek(file.Length, SeekOrigin.Begin);
            return file;
        }
        public static void WriteData(string type,string name,byte[] data)
        {
            if (type == null | type == "" | name == null | name == "" | data == null)
                return;
            var fs= CreateFile(type,name);
            fs.Write(data,0,data.Length);
            fs.Dispose();
        }
        public static byte[] ReadData(string type,string name)
        {
            if (type == null | type == "" | name == null | name == "")
                return null;
            var fs = OpenFile(type,name);
            byte[] buf = null;
            if(fs.Length>0)
            {
                buf = new byte[fs.Length];
                fs.Read(buf,0,buf.Length);
            }
            fs.Dispose();
            return buf;
        }
        public static string GetAssetBundlePath(string name)
        {
            string path = persistentDataPath + "\\bundle";
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                if (files != null)
                    for (int i = 0; i < files.Length; i++)
                    {
                        var ss = files[i].Split('\\');
                        var str = ss[ss.Length - 1];
                        if (str.Split('_')[0] == name)
                            return path;
                    }
            }
            return null;
        }
        public static string FindAssetBundle(string name)
        {
            string path = persistentDataPath + "\\bundle";
            if(Directory.Exists(path))
            {
                var files= Directory.GetFiles(path);
                if (files != null)
                    for (int i = 0; i < files.Length; i++)
                    {
                        var ss= files[i].Split('\\');
                        var str = ss[ss.Length - 1];
                        if (str.Split('_')[0] == name)
                            return str;
                    }
            }
            return null;
        }

        public static byte[] LoadFile(string name)
        {
            string fullname= FindAssetBundle(name);
            if(fullname!=null)
            {
               string path =  persistentDataPath + "\\bundle\\" + fullname;
                var fs = File.Open(path,FileMode.Open);
                byte[] buf = new byte[fs.Length];
                fs.Read(buf,0,buf.Length);
                fs.Dispose();
                return buf;
            }
            return null;
        }

        static void DeleteAssetBundle(string name)
        {
            string path = persistentDataPath + "\\bundle";
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                if (files != null)
                    for (int i = 0; i < files.Length; i++)
                    {
                        var ss = files[i].Split('\\');
                        var str = ss[ss.Length - 1];
                        if (str.Split('_')[0] == name)
                        {
                            File.Delete(files[i]);
                        }
                    }
            }
        }
        public static int GetBundleVersion(string name)
        {
            if (name == null | name == "")
                return 0;
            var str= FindAssetBundle(name);
            if (str == null)
                return 0;
            int v = 0;
            var ss = str.Split('_');
            if (ss.Length > 1)
                int.TryParse(ss[1],out v);
            return v;
        }
        public static void SaveAssetBundle(string name, int version, byte[] data)
        {
            string path = persistentDataPath + "\\bundle";
            if (Directory.Exists(path))
                DeleteAssetBundle(name);
            else Directory.CreateDirectory(path);
            path += "\\" + name + "_" + version.ToString();
            var fs = File.Create(path);
            fs.Write(data, 0, data.Length);
            fs.Dispose();
        }
        public static void ClearAssetBundle()
        {
            string path = persistentDataPath + "\\bundle";
            if (Directory.Exists(path))
                Directory.Delete(path);
        }
    }
}
