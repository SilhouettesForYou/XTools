using System.Collections.Generic;
using System.IO;

namespace XTools
{
    public class Writer
    {
        protected string name = "";
        protected string postfix = "";
        protected string path = "";
        protected CSVUtils.WorkSheetInfo workSheetInfo;

        public Writer(string name)
        {
            this.name = name;
            workSheetInfo = CSVUtils.GetCSVSheetInfo(name);
        }

        public virtual void Write(object obj)
        {
        }
    }
}
