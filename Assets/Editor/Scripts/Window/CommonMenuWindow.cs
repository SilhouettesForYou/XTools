using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XTools
{
    public class CommonMenuWindow : OdinEditorWindow
    {
        /// <summary>
        /// 实现下拉ui
        /// </summary>
        #region
        //点击item回调
        public delegate void MenuFunction();
        //点击item回调可调用参数
        public delegate void MenuFunction2(object userData);
        //item激活的外部判断
        public delegate bool MenuEnableFunction();
        //子面板
        public CommonMenuWindow mychildwindow;
        //选择面板的item索引
        public int selectindex = -1;
        //主面板
        public CommonMenuWindow MainWindow = null;
        //显示用到的GUIStyle
        private GUIStyle seperator;
        //下拉item数据
        private List<MenuItem> menuItems = new List<MenuItem>();
        //下拉item的rece
        private List<Rect> rectlist = new List<Rect>();
        //主面板宽
        public float width;
        //主面板高
        public float height;
        //固定的item高
        private const float itemheight = 26f;
        private const float separator = 2;
        private const int separatorheight = 5;
        private Color enblecolor;
        private Color noenblecolor;
        //鼠标点击事件
        public delegate void MouseEvent();
        public MouseEvent _mousedowneven;
        public MouseEvent _mousemoveeven;
        public MouseEvent _destroyeven;
        /// <summary>
        /// 加入元素
        /// </summary>
        /// <param name="path"></param>
        /// <param name="icon"></param>
        /// <param name="func"></param>
        /// <param name="_enable">是否可以点击的回调可以不传默认为空，跳过该判断</param>
        public void AddItem(string path, int icon, MenuFunction func, MenuEnableFunction _enable)
        {
            string[] patharray = path.Split('/');
            List<string> pathlist = new List<string>(patharray);
            InspectAdd(menuItems, pathlist, icon, func, _enable,"");
        }
        public void AddItem(string path, int icon, MenuFunction func)
        {
            string[] patharray = path.Split('/');
            List<string> pathlist = new List<string>(patharray);
            InspectAdd(menuItems, pathlist, icon, func, null, "");
        }
        public void AddItem(string path, int icon, MenuFunction func, string rightlabel)
        {
            string[] patharray = path.Split('/');
            List<string> pathlist = new List<string>(patharray);
            InspectAdd(menuItems, pathlist, icon, func, null, rightlabel);
        }
        public void AddItem(string path, int icon, MenuFunction func, MenuEnableFunction _enable,string rightlabel)
        {
            string[] patharray = path.Split('/');
            List<string> pathlist = new List<string>(patharray);
            InspectAdd(menuItems, pathlist, icon, func, null,rightlabel);
        }
        public void AddItem(string path, int icon, MenuFunction2 func, object userData, MenuEnableFunction _enable)
        {
            string[] patharray = path.Split('/');
            List<string> pathlist = new List<string>(patharray);
            InspectAdd(menuItems, pathlist, icon, func, userData, _enable, "");
        }

        public void AddItem(string path, int icon, MenuFunction2 func, object userData)
        {
            string[] patharray = path.Split('/');
            List<string> pathlist = new List<string>(patharray);
            InspectAdd(menuItems, pathlist, icon, func, userData, null, "");
        }
        public void AddItem(string path, int icon, MenuFunction2 func, object userData, string rightlabel)
        {
            string[] patharray = path.Split('/');
            List<string> pathlist = new List<string>(patharray);
            InspectAdd(menuItems, pathlist, icon, func, userData, null, rightlabel);
        }
        public void AddItem(string path, int icon, MenuFunction2 func, object userData, MenuEnableFunction _enable, string rightlabel)
        {
            string[] patharray = path.Split('/');
            List<string> pathlist = new List<string>(patharray);
            InspectAdd(menuItems, pathlist, icon, func, userData, _enable, rightlabel);
        }
        public void AddSeparator()
        {
            menuItems.Add(new MenuItem(new GUIContent("_______________________________________________________________________________________________________"), 0, null, delegate ()
            {
                return false;
            },"", separatorheight));
        }
        public int GetItemCount()
        {
            return menuItems.Count;
        }

        /// <summary>
        /// 点击面板的回调，如果foucs在下级点击外部由下而上的回调关闭面板，
        /// </summary>
        public void MouseDownCallBack() {

                CloseAndChild();
                if ( _mousedowneven != null) {
                    _mousedowneven();
                }
        }

        /// <summary>
        /// 悬浮的回调，如果foucs在下级当移动鼠标的时候会调用，
        /// </summary>
        public void MouseHoverCallBack()
        {
            if (_mousemoveeven != null)
            {
                _mousemoveeven();
            }
        }
        /// <summary>
        /// 递归加入元素，菜单是树形结构便于写逐级菜单
        /// </summary>
        /// <param name="list">子节点的枝干</param>
        /// <param name="content">传入的字符串用来新增叶子节点</param>
        /// <returns></returns>
        public void InspectAdd(List<MenuItem> list, List<string> content, int icon, MenuFunction func,MenuEnableFunction  _enable,string  rightlabel)
        {
            List<string> tempcontent = content;
            for (int idx = 0; idx < list.Count; idx++) {
                MenuItem menu = list[idx];
                if (menu.content.text == tempcontent[0])
                {
                    tempcontent.RemoveAt(0);
                    InspectAdd((list[idx]).menuItems, tempcontent, icon, func, _enable, rightlabel);
                    return;
                }
            }
            
            if (tempcontent.Count > 1)
            {
                int count = list.Count();
                list.Add(new MenuItem(new GUIContent(tempcontent[0]), icon, delegate ()
                {
                    DropDownChild(new Rect(width, (count - 2) * itemheight, 0, 0), position, menuItems[count].menuItems);

                    Debug.Log("有子节点所以此处无事件");
                },null, ""));
                tempcontent.RemoveAt(0);
                InspectAdd(((MenuItem)list[list.Count - 1]).menuItems, tempcontent, icon, func, _enable, rightlabel);
            }
            else
            { 
                list.Add(new MenuItem(new GUIContent(tempcontent[0]), icon, func, _enable, rightlabel));
            }

        }
        public void InspectAdd(List<MenuItem> list, List<string> content, int icon, MenuFunction2 func, object userData, MenuEnableFunction _enable, string rightlabel)
        {
            List<string> tempcontent = content;
            for (int idx = 0; idx < list.Count; idx++)
            {
                MenuItem menu = list[idx];
                if (menu.content.text == tempcontent[0])
                {
                    tempcontent.RemoveAt(0);
                    InspectAdd((list[idx]).menuItems, tempcontent, icon, func, userData, _enable, rightlabel);
                    return;
                }
            }

            if (tempcontent.Count > 1)
            {
                list.Add(new MenuItem(new GUIContent(tempcontent[0]), icon, delegate ()
                {
                    Debug.Log(new GUIContent(tempcontent[0] + "有子节点所以此处无事件"));
                }, null, ""));
                tempcontent.RemoveAt(0);
                InspectAdd(((MenuItem)list[list.Count - 1]).menuItems, tempcontent, icon, func, userData, _enable, rightlabel);
            }
            else
            {
                list.Add(new MenuItem(new GUIContent(tempcontent[0]), icon, func, userData, _enable, rightlabel));
            }
        }


        public class MenuItem
        {
            public GUIContent content;
            public GUIContent showcontent;
            public int on;
            public MenuFunction func;
            public MenuFunction2 func2;
            public MenuEnableFunction funcenable;
            public string rightlabel;
            public int fontSize = 12;

            public object userData;
            public List<MenuItem> menuItems = new List<MenuItem>();

            public MenuItem(GUIContent _content, int _on, MenuFunction _func, MenuEnableFunction _enable ,string _rightlabel,int _fontSize = 12)
            {
                content = _content;
                on = _on;
                func = _func;
                funcenable = _enable;
                showcontent = new GUIContent("        " + content.text);
                rightlabel = _rightlabel;
                fontSize = _fontSize;
            }

            public MenuItem(GUIContent _content, int _on, MenuFunction2 _func, object _userData, MenuEnableFunction _enable, string  _rightlabel, int _fontSize = 12)
            {
                content = _content;
                on = _on;
                func2 = _func;
                userData = _userData;
                funcenable = _enable;
                showcontent = new GUIContent("        " + content.text);
                rightlabel = _rightlabel;
                fontSize = _fontSize;
            }
        }
        public void SetMenuList( List<MenuItem> list)
        {
            menuItems = list;
        }
        /// <summary>
        /// 鼠标显示下拉，一般用于右键功能
        /// </summary>
        public void ShowAsContext(Rect father,Vector2 _currentPosition)
        {
            DropDown(new Rect(_currentPosition.x, _currentPosition.y - itemheight, 0,0),  father);
        }

        public void DropDownChild(Rect position, Rect father, List<MenuItem> list)
        {
            mychildwindow = new CommonMenuWindow();
            mychildwindow.SetMenuList(list);
            //mychildwindow._mousedowneven += MouseDownCallBack;
            //mychildwindow._mousemoveeven += MouseHoverCallBack;
            mychildwindow.DropDown(position, father, MouseDownCallBack, MouseHoverCallBack,null);
        }
        public void DropDown(Rect position, Rect father) {
            DropDown( position,  father, delegate ()
            {
                Close();
            },null,null);
        }
        public void DropDown(Rect position, Rect father, MouseEvent destroyback)
        {
            DropDown(position, father, delegate ()
            {
                Close();
            }, null, destroyback);
        }
        public void DropDown(Rect position, Rect father,MouseEvent down, MouseEvent move, MouseEvent destroyback)
        {

            int maxLang = 5;
            height = 0;
            for (int idx = 0; idx < menuItems.Count; idx++)
            {
                MenuItem item = (MenuItem)menuItems[idx];
                if (item.funcenable != null && !item.funcenable() && item.fontSize == separatorheight) {
                    height += itemheight / separator;
                    continue;
                }
                if ((item.content.text.Length+ (item.rightlabel.Length/5)) > maxLang)
                {
                    maxLang = item.content.text.Length + (item.rightlabel.Length / 5);
                }
                height += itemheight;
            }
            if (down != null) {
                _mousedowneven += down;
            }
            if (move != null)
            {
                _mousemoveeven += move;
            }
            if (destroyback != null)
            {
                _destroyeven += destroyback;
            }
            Rect temprect = position;
            temprect.x = position.x + father.x;
            temprect.y = position.y + father.y - height +( 2)* itemheight;
            temprect.width = maxLang * 5 + 50;
            temprect.height = height;
            width = maxLang * 5 + 50;
            MainWindow = this;
            SetupSeperator();
            ShowAsDropDown(temprect, new Vector2(maxLang * 5 + 50, height));
            Focus();

        }
        public void CloseAndChild()
        {
            if (mychildwindow != null)
            {
                mychildwindow.CloseAndChild();
                mychildwindow.Close();
                mychildwindow = null;
            }
        }

        private void CatchMenu(int selected)
        {
            MenuItem i = (MenuItem)menuItems[selected];
            if (i.func2 != null)
                i.func2(i.userData);
            else if (i.func != null)
                i.func();

            if (i.menuItems.Count == 0)
            {
                MouseDownCallBack();
            }
        }
        #endregion
        /// <summary>
        /// 实现父类方法
        /// </summary>
        #region

        private void SetupSeperator()
        {
            seperator = new GUIStyle();
            
            seperator.fixedWidth = width;
            seperator.fontSize = 12;
            seperator.font = GlobalConfig.FONT;
            seperator.normal.background = (Texture2D)GlobalConfig.MENU_NORMAL_TEX;
            seperator.hover.background = (Texture2D)GlobalConfig.MENU_HOVER_BLUE_TEX;
            seperator.fontStyle = FontStyle.Normal;
            seperator.alignment = TextAnchor.MiddleLeft;
            seperator.focused.background = null;
            seperator.onNormal.background = null;

            enblecolor = seperator.normal.textColor;
            noenblecolor = new Color(0.5f, 0.5f, 0.5f);
        }
        protected override void OnDestroy()
        {
            if (_destroyeven != null) {
                _destroyeven();
                _destroyeven = null;
            }  
            _mousedowneven = null;
            _mousemoveeven = null;
            _destroyeven = null;
            //menuItems = null;
            rectlist = null;
            MainWindow = null;
            base.OnDestroy();
        }
        protected bool ItemIsEnable( int selected)
        {
            MenuItem i = (MenuItem)menuItems[selected];
            //if (i.func2 == null && i.func == null && i.menuItems.Count == 0)
            //{
            //    return false;
            //}
            if (i.funcenable != null ){
                return i.funcenable();
            } 
            return true;
        }
        protected override void OnGUI()
        {

            int id = GUIUtility.GetControlID(FocusType.Passive);
            SirenixEditorGUI.BeginVerticalList();
            {
                rectlist.Clear();
                if (MainWindow != null && menuItems != null)
                {
                    float tempheight = 0;
                    int linenum = 0;
                    for (int idx = 0; idx < menuItems.Count; idx++)
                    {
                        MenuItem item = (MenuItem)menuItems[idx];
                        seperator.normal.background = selectindex == idx ? (Texture2D)GlobalConfig.MENU_HOVER_BLUE_TEX : (Texture2D)GlobalConfig.MENU_NORMAL_TEX;
                        if (ItemIsEnable(idx) ) {
                            if (rectlist != null) { 
                                rectlist.Add(new Rect(0, tempheight, width, itemheight));
                                tempheight += itemheight;
                            }
                            seperator.fontSize = item.fontSize;
                            if (EditorGUILayout.DropdownButton(item.showcontent, FocusType.Passive, seperator, GUILayout.Width(width), GUILayout.Height(itemheight)))
                            {
                                CatchMenu(idx); 
                            }
                            if (item.menuItems.Count() > 0)
                            {
                                GUI.DrawTexture(new Rect(width - 30, idx * itemheight + 6 - linenum *(itemheight / (separator)), 10, 10), GlobalConfig.MENU_ARROW_TEX, ScaleMode.ScaleAndCrop);
                            }
                            if (item.rightlabel != "")
                            {
                                Rect labelrect = new Rect(width -( item.rightlabel.Length * 5) - 35, tempheight - 20, item.rightlabel.Length * 8,15);
                                GUI.Label(labelrect, item.rightlabel);
                            }
                        }
                        else
                        {
                            if (item.fontSize != separatorheight)
                            {
                                if (rectlist != null)
                                {
                                    rectlist.Add(new Rect(0, tempheight, width, itemheight ));
                                    tempheight += itemheight ;
                                }
                                seperator.normal.background = (Texture2D)GlobalConfig.MENU_NORMAL_TEX;
                                seperator.hover.background = null;
                                seperator.fontSize = item.fontSize;
                                Color color = seperator.normal.textColor;
                                seperator.normal.textColor = noenblecolor;
                                EditorGUILayout.DropdownButton(item.showcontent, FocusType.Passive, seperator, GUILayout.Width(width), GUILayout.Height(itemheight ));
                                seperator.normal.textColor = enblecolor;
                                seperator.hover.background = (Texture2D)GlobalConfig.MENU_HOVER_BLUE_TEX;
                            }
                            else
                            {
                                if (rectlist != null)
                                {
                                    rectlist.Add(new Rect(0, tempheight, width, itemheight / separator));
                                    tempheight += itemheight / separator;
                                }
                                seperator.normal.background = (Texture2D)GlobalConfig.MENU_NORMAL_TEX;
                                seperator.hover.background = null;
                                seperator.fontSize = item.fontSize;
                                EditorGUILayout.DropdownButton(item.showcontent, FocusType.Passive, seperator, GUILayout.Width(width), GUILayout.Height(itemheight / (separator * 2)));
                                Rect r = GUILayoutUtility.GetRect(width, itemheight / (separator * 2));
                                r.y = r.y - linenum;
                                linenum++;
                                r.height = (itemheight / 4) + 13;
                                GUI.DrawTexture(r, GlobalConfig.MENU_NORMAL_TEX);
                                seperator.hover.background = (Texture2D)GlobalConfig.MENU_HOVER_BLUE_TEX;
                            }
                        }
                    }
                }
            }

            if (!position.Contains(Event.current.mousePosition))
            {
                MouseHoverCallBack();
            }
            if (rectlist != null)
            {
                for (int i = 0; i < rectlist.Count(); i++)
                {
                    if (rectlist[i].Contains(Event.current.mousePosition) && selectindex != i)
                    {
                        selectindex = i;
                        CloseAndChild();
                        if (menuItems[i].menuItems.Count > 0)
                        {
                            DropDownChild(new Rect(width, (i-2) * itemheight, 0, 0),position, menuItems[i].menuItems);
                        }
                    }

                }
            }
           

            SirenixEditorGUI.EndVerticalList();
            this.Repaint();
        }
 
        #endregion

    }
}