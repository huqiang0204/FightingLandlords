using huqiang.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace huqiang
{
    public class DataConversion
    {
        public GameObject Main;
        public virtual void Load(FakeStruct fake) { }
        public virtual void LoadToObject(Component game) { }
    }
}
