namespace Serializer
{
    public class VectorParser<TV> : Parser<TV[]>
    {
        public override object DefaultValue => new TV[0];

        private readonly Parser<TV> _parser;

        public VectorParser() : this(ParserUtil.GetParser<TV>() as Parser<TV>)
        {

        }

        public VectorParser(Parser<TV> parser)
        {
            _parser = parser;
            _parser?.SetParent(this);
        }
        public override int Compare(TV[] value0, TV[] value1)
        {
            if (value0.Length != value1.Length)
                return value0.Length.CompareTo(value1.Length);

            for (int i = 0; i < value0.Length; i++)
            {
                var result = _parser.Compare(value0[i], value1);
                if (result != 0)
                    return result;
            }
            return 0;
        }

        public override bool Parse(string str, out TV[] value)
        {
            str = str?.Trim();
            if (string.IsNullOrWhiteSpace(str))
            {
                value = new TV[0];
                return true;
            }
            var values = str.Split(ParserUtil.ListSeparator[ChildLevel]);
            value = new TV[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                TV res;
                if (!_parser.Parse(values[i], out res))
                {
                    res = (TV)_parser.DefaultValue;
                    return false;
                }
                value[i] = res;
            }
            return true;
        }

        public override string SerializeExcel(object obj)
        {
            TV[] value = (TV[])obj;
            string res = "";
            var splistStr = ParserUtil.ListSeparator[ChildLevel];
            for (int i = 0; i < value.Length; i++)
            {
                res += _parser.SerializeExcel(value[i]);
                if (i != value.Length - 1)
                    res += splistStr;
            }
            return res;
        }
    }

}