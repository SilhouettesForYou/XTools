namespace Serializer
{
    public class StringParser : Parser<string>
    {
        public override object DefaultValue => string.Empty;

        public override int Compare(string value0, string value1)
        {
            return string.Compare(value0, value1, System.StringComparison.Ordinal);
        }

        public override bool Parse(string str, out string value)
        {
            if (str == null)
            {
                value = (string)DefaultValue;
                return true;
            }
            value = str;
            return true;
        }

        public override string SerializeExcel(object obj)
        {
            return obj.ToString();
        }

    }
}