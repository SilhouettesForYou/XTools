using System;
using System.Collections.Generic;
using System.Reflection;
using XTools;

namespace Serializer
{
    public class ParserUtil : SingleBase<ParserUtil>
    {
        public static readonly char[] ListSeparator = { '|', '='};
        public static readonly char[] SeqSeparator = { '=', '=' };
        public static readonly byte SEQ_MAX_LENGTH = 9;
        public static readonly char PLACE_HOLDER = '#';

        private static Dictionary<string, Parser> _cacheParser = new Dictionary<string, Parser>();

        private static Type GetAssemblyType(string typeName)
        {
            var classes = Assembly.Load("Serializer").GetTypes();
            foreach (var item in classes)
            {
                if (typeName == item.Name)
                {
                    return item;
                }
            }
            return null;
        }

        private static string GetTypeFullName(string typeName)
        {
            return null;
        }

        public static Parser GetParser<T>(bool userCache = false)
        {
            var name = typeof(T).Name;
            if (userCache && _cacheParser.TryGetValue(name, out var ret))
            {
                return ret;
            }

            if (TypeDefine.GetTypeStrMap().TryGetValue(name, out string value))
            {
                var type = GetAssemblyType($"{value}Parser");
                var argTypes = new Type[0];
                var args = new object[0];
                ret = type.GetConstructor(argTypes)?.Invoke(args) as Parser<T>;
                if (userCache)
                {
                    _cacheParser.Add(name, ret);
                }
                return ret;
            }
            return null;
        }

        public static Parser GetParser(string typeName, bool userCache = false)
        {
            if (userCache && _cacheParser.TryGetValue(typeName, out var ret))
            {
                return ret;
            }

            if (TypeDefine.GetBasicTypeStrMap().ContainsKey(typeName))
            {
                var type = GetAssemblyType($"{TypeDefine.GetBasicTypeStrMap()[typeName]}Parser");
                var argTypes = new Type[0];
                var args = new object[0];
                ret = type.GetConstructor(argTypes)?.Invoke(args) as Parser;
                if (userCache)
                {
                    _cacheParser.Add(typeName, ret);
                }
                return ret;
            }
            else if (typeName.StartsWith("Sequence"))
            {
                typeName = typeName.Trim();
                var subType = typeName.Substring("Sequence<".Length, typeName.Length - "Sequence<, #>".Length);
                var subTypeFullName = GetTypeFullName(subType);
                var num = byte.Parse(typeName.Substring(typeName.Length - 2, 1));
                var parserTypeName = "";
                var type = Type.GetType(parserTypeName);
                var subParser = GetParser(subType, false);
                var argTypes = new Type[2] { typeof(byte), subParser.GetType() };
                var args = new object[2] { num, subParser };
                ret = type.GetConstructor(argTypes)?.Invoke(args) as Parser;
                if (userCache)
                {
                    _cacheParser.Add(typeName, ret);
                }
                return ret;
            }
            else if (typeName.StartsWith("vector"))
            {
                typeName = typeName.Trim();
                var subType = typeName.Substring("vector<".Length, typeName.Length - "vector<>".Length);
                var subTypeFullName = GetTypeFullName(subType);
                var num = byte.Parse(typeName.Substring(typeName.Length - 2, 1));
                var parserTypeName = "";
                var type = Type.GetType(parserTypeName);
                var subParser = GetParser(subType, false);
                var argTypes = new Type[1] { subParser.GetType() };
                var args = new object[1] { subParser };
                ret = type.GetConstructor(argTypes)?.Invoke(args) as Parser;
                if (userCache)
                {
                    _cacheParser.Add(typeName, ret);
                }
                return ret;
            }

            return null;
        }

        public static Parser GetParser(FieldTypes typeEnum, sbyte seqLength)
        {
            int type = (int)typeEnum;
            if (type < 0 || type % 100 >= TypeDefine.GetBasicTypeStr().Count)
            {
                return null;
            }

            string baseType = TypeDefine.GetBasicTypeStr()[type % 100];
            if (type / 100 == 0)
            {
                return GetParser(baseType);
            }
            else if (type / 100 == 1)
            {
                return GetParser($"vector<{baseType}>");
            }
            else if (type / 100 == 2)
            {
                return GetParser($"Sequence<{baseType}, {seqLength}>");
            }
            else if (type / 100 == 3)
            {
                return GetParser($"vector<Sequence<{baseType}, {seqLength}>>");
            }
            else if (type / 100 == 4)
            {
                return GetParser($"vector<vector<{baseType}>>");
            }
            return null;
        }
    }
}