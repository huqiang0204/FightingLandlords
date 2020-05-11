using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class LZMAMission : IMission
{
    public Decoder decoder;
    public override bool Done => decoder.Progress == 1;
    public override float Progress
    {
        get
        {
            if (decoder == null)
                return 0;
            return decoder.Progress;
        }
    }
    public override bool Running => decoder != null;

    public string filePath;
    public string savePath;
    public override async void Run()
    {
        if (decoder != null)
            return;
        decoder = new Decoder();
        await Task.Run(() => { decoder.DecompressFile(filePath, savePath); });
        if (Completed != null)
            Completed(this);
    }
}
