using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XTools
{
    public enum FieldTypes
    {
        NONE = -1,
        CHAR = 0,
        VECTOR_CHAR = 100,
        SEQUENCE_CHAR = 200,
        VECTOR_SEQUENCE_CHAR = 300,
        VECTOR_VECTOR_CHAR = 400,
        BOOL = 1,
        VECTOR_BOOL = 101,
        SEQUENCE_BOOL = 201,
        VECTOR_SEQUENCE_BOOL = 301,
        VECTOR_VECTOR_BOOL = 401,
        INT = 2,
        VECTOR_INT = 102,
        SEQUENCE_INT = 202,
        VECTOR_SEQUENCE_INT = 302,
        VECTOR_VECTOR_INT = 402,
        UINT = 3,
        VECTOR_UINT = 103,
        SEQUENCE_UINT = 203,
        VECTOR_SEQUENCE_UINT = 303,
        VECTOR_VECTOR_UINT = 403,
        FLOAT = 4,
        VECTOR_FLOAT = 104,
        SEQUENCE_FLOAT = 204,
        VECTOR_SEQUENCE_FLOAT = 304,
        VECTOR_VECTOR_FLOAT = 404,
        DOUBLE = 5,
        VECTOR_DOUBLE = 105,
        SEQUENCE_DOUBLE = 205,
        VECTOR_SEQUENCE_DOUBLE = 305,
        VECTOR_VECTOR_DOUBLE = 405,
        LONGLONG = 6,
        VECTOR_LONGLONG = 106,
        SEQUENCE_LONGLONG = 206,
        VECTOR_SEQUENCE_LONGLONG = 306,
        VECTOR_VECTOR_LONGLONG = 406,
        STRING = 7,
        VECTOR_STRING = 107,
        SEQUENCE_STRING = 207,
        VECTOR_SEQUENCE_STRING = 307,
        VECTOR_VECTOR_STRING = 407
    }

    public class TypeDefine
    {
        private static Dictionary<string, string> _typeStrMap;
        private static Dictionary<string, string> _basicTypeStrMap;
        private static List<string> _basicTypeStr;

        public static Dictionary<string, string> GetTypeStrMap()
        {
            if (_typeStrMap == null)
            {
                _typeStrMap = new Dictionary<string, string>();
                _typeStrMap.Add("Char", "Char");
            }
            return _typeStrMap;
        }

        public static Dictionary<string, string> GetBasicTypeStrMap()
        {
            if (_basicTypeStrMap == null)
            {
                _basicTypeStrMap.Add("char", "Char");
            }
            return _basicTypeStrMap;
        }

        public static List<string> GetBasicTypeStr()
        {
            if (_basicTypeStr == null)
            {
                _basicTypeStr.Add("char");
            }
            return _basicTypeStr;
        }
    }
}
