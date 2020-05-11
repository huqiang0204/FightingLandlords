using Aliyun.OSS;
using Aliyun.OSS.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Config
{
    public const string AccessKeyId = "LTAIps8wkImlvvAo";
    public const string AccessKeySecret = "SMMoNOlZ16k943tbFHhKmQLm0X6Upx";
    public const string EndPoint = "oss-cn-shenzhen.aliyuncs.com";
    public const string Bucket = "hotchess";

}
public class OssMission : IMission
{
    public override bool Done => done;
    bool done;
    public override float Progress
    {
        get
        {
            if (TotalBytes == 0)
                return 0;
            return (float)TransferredBytes / (float)TotalBytes;
        }
    }

    public override bool Running => running;
    bool running;

    long TotalBytes;
    long TransferredBytes;
    public string dir;
    public override async void Run()
    {
        if (running)
            return;
        running = true;
        var dat = await OssDownload();
        if (dat != null)
            done = true;
        if (Completed != null)
            Completed(this);
    }
    async Task<ObjectMetadata> OssDownload()
    {
        string path = dir + Name;
        if (File.Exists(path))
            File.Delete(path);
        var ossClient = new OssClient(Config.EndPoint, Config.AccessKeyId, Config.AccessKeySecret);
        try
        {
            DownloadObjectRequest request = new DownloadObjectRequest(Config.Bucket, Name, path)
            {
                PartSize = 8 * 1024 * 1024,
                ParallelThreadCount = 3,
                CheckpointDir = dir
            };
            request.StreamTransferProgress += OssDown;
            ObjectMetadata dat = await Task.Run(
                () => {
                    try
                    {
                        return ossClient.ResumableDownloadObject(request);
                    }
                    catch
                    {
                        return null;
                    }
                }
                );
            return dat;
        }
        catch (OssException ex)
        {
            Debug.Log(ex.StackTrace);
            return null;
        }
    }
    void OssDown(object sender, StreamTransferProgressArgs args)
    {
        TotalBytes = args.TotalBytes;
        TransferredBytes = args.TransferredBytes;
    }
}
