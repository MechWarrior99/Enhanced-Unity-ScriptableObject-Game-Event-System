using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Bewildered.Events;

namespace Bewildered.Editors.Events
{
    [CustomPropertyDrawer(typeof(EnhancedEvent))]
    internal class EnhancedEventDrawer : PropertyDrawer
    {
        private ReorderableList _rlEventEntries;
        private SerializedProperty _property;
        private List<EnhancedEventEntryDrawer> _entryDrawers = new List<EnhancedEventEntryDrawer>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {            
            if (_rlEventEntries == null)
                Init(property);

            _rlEventEntries.DoLayoutList();
        }

        private void Init(SerializedProperty property)
        {
            _property = property;

            _rlEventEntries = new ReorderableList(property.serializedObject, property.FindPropertyRelative("_events"));
            _rlEventEntries.draggable = false;
            _rlEventEntries.drawHeaderCallback = DrawHeaderCallback;            
            _rlEventEntries.drawElementCallback = DrawElementCallback;
            _rlEventEntries.elementHeightCallback = ElementHeightCallback;
            _rlEventEntries.onAddCallback = OnAddCallback;
            _rlEventEntries.onRemoveCallback = OnRemoveCallback;

            for (int i = 0; i < _rlEventEntries.count; i++)
            {
                _entryDrawers.Add(new EnhancedEventEntryDrawer());
            }
        }

        private void DrawHeaderCallback(Rect rect)
        {
            GUI.Label(rect, _property.displayName);
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 5;
            rect.height += 10;
            _entryDrawers[index].OnGUI(rect, _property.FindPropertyRelative("_events").GetArrayElementAtIndex(index), GUIContent.none);
        }

        private float ElementHeightCallback(int index)
        {
            return _entryDrawers[index].GetHeight() + 10;
        }

        private void OnAddCallback(ReorderableList list)
        {
            list.serializedProperty.InsertArrayElementAtIndex(list.count);
            _entryDrawers.Add(new EnhancedEventEntryDrawer());
        }

        private void OnRemoveCallback(ReorderableList list)
        {
            int index = list.index;
            list.serializedProperty.DeleteArrayElementAtIndex(index);
            _entryDrawers.RemoveAt(index);
        }
    }
}