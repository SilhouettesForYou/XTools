namespace Serializer
{
    public class FloatParser : Parser<float>
    {
        public override int Compare(float value0, float value1)
        {
            return value0.CompareTo(value1);
        }

        public override bool Parse(string str, out float value)
        {
            str = str?.Trim();
            if (string.IsNullOrWhiteSpace(str))
            {
                value = (float)DefaultValue;
                return true;
            }
            return float.TryParse(str, out value);
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