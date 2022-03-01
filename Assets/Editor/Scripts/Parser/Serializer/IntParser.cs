namespace Serializer
{
    public class IntParser : Parser<int>
    {
        public override int Compare(int value0, int value1)
        {
            return value0.CompareTo(value1);
        }

        public override bool Parse(string str, out int value)
        {
            str = str?.Trim();
            if (string.IsNullOrWhiteSpace(str))
            {
                value = (int)DefaultValue;
                return true;
            }
            return int.TryParse(str, out value);
        }

        public override string SerializeExcel(object obj)
        {
            return obj.ToString();
        }
    }
}