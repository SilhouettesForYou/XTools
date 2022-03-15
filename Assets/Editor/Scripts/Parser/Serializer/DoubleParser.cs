namespace Serializer
{
    public class DoubleParser : Parser<double>
    {
        public override int Compare(double value0, double value1)
        {
            return value0.CompareTo(value1);
        }

        public override bool Parse(string str, out double value)
        {
            str = str?.Trim();
            if (string.IsNullOrWhiteSpace(str))
            {
                value = (double)DefaultValue;
                return true;
            }
            return double.TryParse(str, out value);
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