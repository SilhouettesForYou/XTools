namespace Serializer
{
    public class BoolParser : Parser<bool>
    {
        public override int Compare(bool value0, bool value1)
        {
            return 0;
        }

        public override bool Parse(string str, out bool value)
        {
            str = str?.Trim();
            if (string.IsNullOrWhiteSpace(str))
            {
                value = (bool)DefaultValue;
                return true;
            }
            value = false;
            if (string.IsNullOrEmpty(str))
            {
                value = false;
                return true;
            }
            if (str.ToLower() == "true" || str == "1")
            {
                value = true;
                return true;
            }
            if (str.ToLower() == "false" || str == "0")
            {
                value = false;
                return true;
            }
            return false;
        }

        public override string SerializeExcel(object obj)
        {
            var value = obj ?? false;
            return (bool)value ? "1" : "0";
        }

        public override string SerializeLua(object obj)
        {
            var value = obj ?? false;
            return (bool)value ? "true" : "false";
        }
    }
}