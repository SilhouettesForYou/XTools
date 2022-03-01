using System;

namespace Serializer
{
    public class EnumParser<TE> : Parser<TE> where TE : struct
    {
        public override int Compare(TE value0, TE value1)
        {
            return ((int)(object)value0).CompareTo((int)(object)value1);
        }

        public override bool Parse(string str, out TE value)
        {
            str = str?.Trim();
            if (string.IsNullOrWhiteSpace(str))
            {
                value = (TE)DefaultValue;
                return true;
            }
            return Enum.TryParse(str, false, out value);
        }

        public override string SerializeExcel(object obj)
        {
            return obj.ToString();
        }
    }
}