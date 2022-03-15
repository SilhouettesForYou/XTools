using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XTools
{
    public class GlobalConfig
    {
        public static string DATA_DIR = "Assets/Editor/Data/";
        public static string JSON_DIR = DATA_DIR + "Configs/";

        public static string CSV_PATH = DATA_DIR + "CSV/";
        public static string OUTPUT_LUA_PATH = DATA_DIR + "Output/Lua/";

        public static string RES_DIR = "Assets/Arts/";
        public static string FONT_PATH = RES_DIR + "Fonts/FZLTHJW.TTF";
        public static Font _font;
        public static Font FONT
        {
            get
            {
                if (_font == null)
                {
                    _font = AssetDatabase.LoadAssetAtPath<Font>(FONT_PATH);
                }
                return _font;
            }
        }

        public static string MENU_NORMAL_PATH = RES_DIR + "Icons/normalwhite.png";
        public static Texture _menuNormalTex;
        public static Texture MENU_NORMAL_TEX
        {
            get
            {
                if (_menuNormalTex == null)
                {
                    _menuNormalTex = AssetDatabase.LoadAssetAtPath<Texture>(MENU_NORMAL_PATH);
                }
                return _menuNormalTex;
            }
        }

        public static string MENU_HOVER_BLUE_PATH = RES_DIR + "Icons/hoverblue.png";
        public static Texture _menuHoverBlueTex;
        public static Texture MENU_HOVER_BLUE_TEX
        {
            get
            {
                if (_menuHoverBlueTex == null)
                {
                    _menuHoverBlueTex = AssetDatabase.LoadAssetAtPath<Texture>(MENU_HOVER_BLUE_PATH);
                }
                return _menuHoverBlueTex;
            }
        }

        public static string MENU_ARROW_PATH = RES_DIR + "Icons/arrow.png";
        public static Texture _menuArrowTex;
        public static Texture MENU_ARROW_TEX
        {
            get
            {
                if (_menuArrowTex == null)
                {
                    _menuArrowTex = AssetDatabase.LoadAssetAtPath<Texture>(MENU_ARROW_PATH);
                }
                return _menuArrowTex;
            }
        }

        public static string MENU_HOVER_PATH = RES_DIR + "Icons/menuhover.png";
        public static Texture _menuHoverTex;
        public static Texture MENU_HOVER_TEX
        {
            get
            {
                if (_menuHoverTex == null)
                {
                    _menuHoverTex = AssetDatabase.LoadAssetAtPath<Texture>(MENU_HOVER_PATH);
                }
                return _menuHoverTex;
            }
        }

        public static string MENU_TOOLS_NAME = "Tools";
        public static string MENU_TOOLS_EXPORT_LUA_ALL = "Export Lua (All)";
    }
}

