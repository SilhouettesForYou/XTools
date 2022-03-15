using UnityEngine;
using UnityEditor;


namespace XTools
{
    public class CustomEditorStyle
    {
        public static GUIStyle toolbarButton;
        public static GUIStyle popup;
        public static GUIStyle dropdownButtonNormal;
        public static GUIStyle dropdownButtonSelected;

        static CustomEditorStyle()
        {
            toolbarButton = new GUIStyle(EditorStyles.toolbarButton);
            toolbarButton.fontSize = 12;
            toolbarButton.fixedHeight = 18;
            toolbarButton.font = GlobalConfig.FONT;

            popup = new GUIStyle(EditorStyles.popup);
            popup.fontSize = 12;
            popup.fixedHeight = 18;
            popup.alignment = TextAnchor.MiddleCenter;

            dropdownButtonNormal = new GUIStyle();
            dropdownButtonNormal.fontSize = 15;
            dropdownButtonNormal.fontStyle = FontStyle.Normal;
            dropdownButtonNormal.font = GlobalConfig.FONT;
            dropdownButtonNormal.alignment = TextAnchor.MiddleCenter;
            dropdownButtonNormal.hover.background = (Texture2D)GlobalConfig.MENU_HOVER_TEX;
            dropdownButtonNormal.hover.textColor = new Color(0, 0, 0);
            dropdownButtonNormal.normal.background = null;
            dropdownButtonNormal.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

            dropdownButtonSelected = new GUIStyle();
            dropdownButtonSelected.fontSize = 15;
            dropdownButtonSelected.fontStyle = FontStyle.Normal;
            dropdownButtonSelected.font = GlobalConfig.FONT;
            dropdownButtonSelected.alignment = TextAnchor.MiddleCenter;
            dropdownButtonSelected.hover.background = (Texture2D)GlobalConfig.MENU_HOVER_TEX;
            dropdownButtonSelected.hover.textColor = new Color(0, 0, 0);
            dropdownButtonSelected.normal.background = (Texture2D)GlobalConfig.MENU_HOVER_TEX;
            dropdownButtonSelected.normal.textColor = new Color(0, 0, 0);

        }
    }
}
