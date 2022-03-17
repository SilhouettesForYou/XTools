using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XTools
{
    public class MenuFunctions
    {
        public static void ExportLuaAllForServerWithIndex()
        {
            JsonDataProcesser.Instance().ExportToLua(ExportLuaType.SERVER_WITH_INDEX);
        }

        public static void ExportLuaAllForClientWithIndex()
        {
            JsonDataProcesser.Instance().ExportToLua(ExportLuaType.CLIENT_WITH_INDEX);
        }

        public static void ExportLuaAllForServerWithKey()
        {
            JsonDataProcesser.Instance().ExportToLua(ExportLuaType.SERVER_WITH_KEY);
        }

        public static void ExportLuaAllForClientWithKey()
        {
            JsonDataProcesser.Instance().ExportToLua(ExportLuaType.CLIENT_WITH_KEY);
        }
    }
}
