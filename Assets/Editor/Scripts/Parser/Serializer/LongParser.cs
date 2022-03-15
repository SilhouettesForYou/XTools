namespace Serializer
{
    public class LongParser : Parser<long>
    {
        public override int Compare(long value0, long value1)
        {
            return value0.CompareTo(value1);
        }

        public override bool Parse(string str, out long value)
        {
            str = str?.Trim();
            if (string.IsNullOrWhiteSpace(str))
            {
                value = (long)DefaultValue;
                return true;
            }
            return long.TryParse(str, out value);
        }

        public override string SerializeExcel(object obj)
        {
            return obj.ToString();
        }

        public override string SerializeLua(object obj)
        {
            return SerializeExcel(obj);
        }
    }
}