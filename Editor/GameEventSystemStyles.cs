using UnityEngine;

namespace Bewildered.Editors.Events
{
    internal static class GameEventStyles
    {
        public static GUIStyle ActiveToolbarButton { get; private set; }
        public static GUIStyle ListBackgroundOdd { get; private set; }
        public static GUIStyle ListBackgroundEven { get; private set; }
        public static GUIStyle Box { get; private set; }

        static GameEventStyles()
        {
            ActiveToolbarButton = new GUIStyle("toolbarbutton");
            ActiveToolbarButton.normal = ActiveToolbarButton.onNormal;

            ListBackgroundOdd = new GUIStyle("OL EntryBackOdd");
            ListBackgroundOdd.margin = new RectOffset(0, 0, 0, 0);
            ListBackgroundOdd.padding = new RectOffset(5, 5, 5, 5);
            ListBackgroundOdd.richText = true;

            ListBackgroundEven = new GUIStyle("OL EntryBackEven");
            ListBackgroundEven.margin = new RectOffset(0, 0, 0, 0);
            ListBackgroundEven.padding = new RectOffset(5, 5, 5, 5);
            ListBackgroundEven.richText = true;

            Box = new GUIStyle("CN Box");
            Box.padding = new RectOffset(1, 1, 1, 1);
        }
    }
}