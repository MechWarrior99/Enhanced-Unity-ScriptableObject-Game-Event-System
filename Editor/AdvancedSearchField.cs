using UnityEngine;
using UnityEditor;
using System;

namespace Bewildered.Editors
{
    /// <summary>
    /// The AdvancedSearchField creates a text field for the user to input text to be used for searching. Along with a popup enum for filtering searches.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    public class AdvancedSearchField<TEnum> where TEnum : Enum
    {
        private GenericMenu _filterTypesMenu = new GenericMenu();

        public TEnum FilterType { get; private set; }

        public AdvancedSearchField()
        {
            BuildMenu();
        }

        public string OnGUI(string searchFilter, params GUILayoutOption[] options)
        {
            using (EditorGUILayout.HorizontalScope h = new EditorGUILayout.HorizontalScope(options))
            {
                // Button to show the filter type popup. Needs to overlap the start of the search text field in order to look right. 
                // Seems like this is how the ToolbarSeachTextFieldPopup style was meant to be used sense this is how Unity uses it.
                if (GUI.Button(new Rect(h.rect.x, h.rect.y, 16, 17), "", "ToolbarSeachTextFieldPopup"))
                    _filterTypesMenu.ShowAsContext();

                // Search field.
                GUI.SetNextControlName("advancedSearchField");
                searchFilter = GUILayout.TextField(searchFilter, "ToolbarSeachTextFieldPopup");

                // Shows the name of the filter type if the search filter is empty, and the field is not focused.
                if (searchFilter == "" && GUI.GetNameOfFocusedControl() != "advancedSearchField")
                {
                    GUI.enabled = false;
                    GUI.Label(new Rect(h.rect.x + 14, h.rect.y, 50, 17), FilterType.ToString());
                    GUI.enabled = true;
                }

                // Displays the clear button if there is text to clear from the search filter. Otherwise displays the empty button.
                if (searchFilter == "")
                {
                    // For some reason this style is 1 pixel off from the FieldPopup style.
                    GUILayout.Button("", new GUIStyle("ToolbarSeachCancelButtonEmpty") { margin = new RectOffset(0, 0, 1, 0) });
                }
                else
                {
                    // For some reason this style is 1 pixel off from the FieldPopup style.
                    if (GUILayout.Button("", new GUIStyle("ToolbarSeachCancelButton") { margin = new RectOffset(0, 0, 1, 0) }))
                    {
                        searchFilter = "";
                    }
                }
            }

            return searchFilter;
        }

        private void BuildMenu()
        {
            _filterTypesMenu = new GenericMenu();

            Array values = Enum.GetValues(typeof(TEnum));
            foreach (int value in values)
            {
                string name = Enum.GetName(typeof(TEnum), value);
                bool isSelected = false;

                if (Enum.GetName(typeof(TEnum), FilterType) == Enum.GetName(typeof(TEnum), value))
                    isSelected = true;

                _filterTypesMenu.AddItem(new GUIContent(name), isSelected, OnFilterTypeSelected, value);
            }
        }

        private void OnFilterTypeSelected(object filterType)
        {
            FilterType = (TEnum)filterType;
            BuildMenu();
        }
    }
}