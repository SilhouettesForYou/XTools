namespace Serializer
{
    public class UIntParser : Parser<uint>
    {
        public override int Compare(uint value0, uint value1)
        {
            return value0.CompareTo(value1);
        }

        public override bool Parse(string str, out uint value)
        {
            str = str?.Trim();
            if (string.IsNullOrWhiteSpace(str))
            {
                value = (uint)DefaultValue;
                return true;
            }
            return uint.TryParse(str, out value);
        }

        public override string SerializeExcel(object obj)
        {
            return obj.ToString();
        }
    }
}