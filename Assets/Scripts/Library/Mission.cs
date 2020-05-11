using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class IMission
{
    public string Name;
    public string Version;
    public Action<IMission> Tip;
    public virtual bool Running { get; }
    public virtual bool Done { get; }
    public virtual float Progress { get; }
    public Action<IMission> Completed;
    public virtual void Run()
    {
    }
}