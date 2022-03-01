namespace Serializer
{
    public class CharParser : Parser<char>
    {
        public override int Compare(char value0, char value1)
        {
            return value0.CompareTo(value1);
        }

        public override bool Parse(string str, out char value)
        {
            str = str?.Trim();
            if (string.IsNullOrWhiteSpace(str))
            {
                value = (char)DefaultValue;
                return true;
            }
            return char.TryParse(str, out value);
        }

        public override string SerializeExcel(object obj)
        {
            return obj.ToString();
        }
    }
}