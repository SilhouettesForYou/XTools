using System;
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
        private static Dictionary<string, Type> _basicTypeMap;
        private static List<string> _basicTypeStr;

        public static Dictionary<string, string> GetTypeStrMap()
        {
            if (_typeStrMap == null)
            {
                _typeStrMap = new Dictionary<string, string>();
                _typeStrMap.Add("Char", "Char");
                _typeStrMap.Add("Boolean", "Bool");
                _typeStrMap.Add("Single", "Float");
                _typeStrMap.Add("Int32", "Int");
                _typeStrMap.Add("Int64", "Long");
                _typeStrMap.Add("String", "String");
                _typeStrMap.Add("UInt32", "UInt");
            }
            return _typeStrMap;
        }

        public static Dictionary<string, string> GetBasicTypeStrMap()
        {
            if (_basicTypeStrMap == null)
            {
                _basicTypeStrMap = new Dictionary<string, string>();
                _basicTypeStrMap.Add("char", "Char");
                _basicTypeStrMap.Add("bool", "Bool");
                _basicTypeStrMap.Add("double", "Double");
                _basicTypeStrMap.Add("float", "Float");
                _basicTypeStrMap.Add("int", "Int");
                _basicTypeStrMap.Add("long long", "Long");
                _basicTypeStrMap.Add("string", "String");
                _basicTypeStrMap.Add("uint", "UInt");
            }
            return _basicTypeStrMap;
        }

        public static List<string> GetBasicTypeStr()
        {
            if (_basicTypeStr == null)
            {
                _basicTypeStr = new List<string>();
                _basicTypeStr.Add("char");
                _basicTypeStr.Add("bool");
                _basicTypeStr.Add("int");
                _basicTypeStr.Add("uint");
                _basicTypeStr.Add("float");
                _basicTypeStr.Add("double");
                _basicTypeStr.Add("long long");
                _basicTypeStr.Add("string");
            }
            return _basicTypeStr;
        }

        public static Dictionary<string, Type> GetBasicTypeMap()
        {
            if (_basicTypeMap == null)
            {
                _basicTypeMap = new Dictionary<string, Type>();
                _basicTypeMap.Add("char", typeof(char));
                _basicTypeMap.Add("bool", typeof(bool));
                _basicTypeMap.Add("double", typeof(double));
                _basicTypeMap.Add("float", typeof(float));
                _basicTypeMap.Add("int", typeof(int));
                _basicTypeMap.Add("long long", typeof(long));
                _basicTypeMap.Add("string", typeof(string));
                _basicTypeMap.Add("uint", typeof(uint));
            }
            return _basicTypeMap;
        }
    }
}
