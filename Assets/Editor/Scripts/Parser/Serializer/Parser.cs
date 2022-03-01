namespace Serializer
{
    public abstract class Parser
    {
        public int ChildLevel { get; private set; }
        public Parser ParentParser { get; private set; }
        public void SetParent(Parser parser)
        {
            ParentParser = parser;
            ChildLevel = parser.ChildLevel + 1;
        }

        public abstract object DefaultValue { get; }

        public abstract int Compare(object value0, object value1);

        public int CompareFromString(string str1, string str2)
        {
            if (!Parse(str1, out object obj1) || !Parse(str2, out object obj2))
            {
                return 0;
            }
            return Compare(obj1, obj2);
        }

        public abstract bool Parse(string str, out object obj);

        public abstract string SerializeExcel(object obj);

    }

    public abstract class Parser<T> : Parser
    {
        public override object DefaultValue => default(T);

        public abstract int Compare(T value0, T value1);

        public abstract bool Parse(string str, out T value);


        public override int Compare(object value0, object value1)
        {
            return Compare((T)value0, (T)value1);
        }

        public override bool Parse(string str, out object value)
        {
            if (Parse(str, out T result))
            {
                value = result;
                return true;
            }
            value = result;
            return false;
        }
    }
}