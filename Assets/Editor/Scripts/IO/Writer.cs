using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XTools
{
    public abstract class Writer
    {
        public Writer(string name) { }

        public abstract void Write();
    }
}
