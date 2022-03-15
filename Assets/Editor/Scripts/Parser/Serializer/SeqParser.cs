using System;
using System.Text;

namespace Serializer
{
    public class Seq<T>
    {
        private T[] _values = null;
        private byte _capacity = 0;

        public Seq(byte capacity)
        {
            _capacity = capacity;
            _values = new T[_capacity];
        }

        public int Capacity { get { return _capacity; } }

        public T this[int key]
        {
            get
            {
                if (key >= _capacity)
                {
                    return default(T);
                }
                return _values[key];
            }
            set
            {
                if (key >= _capacity)
                {
                    return;
                }
                _values[key] = value;
            }
        }

        public override string ToString()
        {
            StringBuilder builfer = new StringBuilder();
            for (int i = 0; i < _values.Length; i++)
            {
                if (i != 0)
                {
                    builfer.Append(",");
                }
                builfer.Append(_values[i].ToString());
            }
            return builfer.ToString();
        }
    }
    public class SeqParser<TV> : Parser<Seq<TV>>
    {
        private readonly byte _length;
        private readonly Parser<TV> _parser;

        public override object DefaultValue => new Seq<TV>(_length);

        public SeqParser(byte length) : this(length, ParserUtil.GetParser<TV>() as Parser<TV>)
        {

        }

        public SeqParser(byte length, Parser<TV> parser)
        {
            if (length > ParserUtil.SEQ_MAX_LENGTH)
                throw new Exception($"sequence数量不能超过{ParserUtil.SEQ_MAX_LENGTH}个");
            _length = length;
            _parser = parser;
            _parser?.SetParent(this);
        }
        public override int Compare(Seq<TV> value0, Seq<TV> value1)
        {
            if (value0.Capacity != value1.Capacity)
                return value0.Capacity.CompareTo(value1.Capacity);

            for (int i = 0; i < value0.Capacity; i++)
            {
                var result = _parser.Compare(value0[i], value1[i]);
                if (result != 0)
                    return result;
            }
            return 0;
        }

        public override bool Parse(string str, out Seq<TV> value)
        {
            str = str?.Trim();
            var values = str.Split(ParserUtil.SeqSeparator[ChildLevel]);
            if (values.Length > ParserUtil.SEQ_MAX_LENGTH)
            {
                throw new Exception($"sequence数量不能超过{ParserUtil.SEQ_MAX_LENGTH}个");
            }
            
            value = new Seq<TV>(_length);
            for (int i = 0; i < _length; i++)
            {
                if (values.Length <= i)
                {
                    value[i] = (TV)_parser.DefaultValue;
                }
                else
                {
                    TV res;
                    if (!_parser.Parse(values[i], out res))
                    {
                        res = (TV)_parser.DefaultValue;
                    }
                    value[i] = res;
                }
            }
            return true;
        }

        public override string SerializeExcel(object obj)
        {
            string res = "";
            Seq<TV> value = (Seq<TV>)obj;
            char splitStr = ParserUtil.SeqSeparator[ChildLevel];
            int capacity = Math.Min(value.Capacity, _length);
            for (int i = 0; i < capacity; i++)
            {
                res += _parser.SerializeExcel(value[i]);
                if (i != capacity - 1)
                    res += splitStr;
            }
            return res;
        }

        public override string SerializeLua(object obj)
        {
            string res = "{ ";
            Seq<TV> value = (Seq<TV>)obj;
            var splitStr = ", ";
            int capacity = Math.Min(value.Capacity, _length);
            for (int i = 0; i < capacity; i++)
            {
                res += _parser.SerializeLua(value[i]);
                if (i != capacity - 1)
                    res += splitStr;
            }
            return res + " }";
        }
    }
}