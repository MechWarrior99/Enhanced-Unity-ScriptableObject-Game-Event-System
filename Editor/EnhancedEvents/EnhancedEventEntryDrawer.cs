using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;
using Bewildered.Events;

namespace Bewildered.Editors.Events
{
    [CustomPropertyDrawer(typeof(EnhancedEventEntry))]
    internal class EnhancedEventEntryDrawer : PropertyDrawer
    {
        private Rect _objectFieldRect;
        private Rect _popupFieldRect;        
        private Rect[] _parameterRects;
        private MethodsDropdown _methodDropdown;
        private Object _eventTargetObject;
        private DelegateInfo _selectedInfo;       
        private EnhancedEventEntry _targetEventEntry;
        private ParameterInfo[] _parameters;
        private SerializedProperty _property;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (_selectedInfo != null)
            {
                height += _selectedInfo.Method.GetParameters().Length * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            }

            return height;
        }

        public float GetHeight()
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (_selectedInfo != null)
            {
                height += _selectedInfo.Method.GetParameters().Length * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            }

            return height;
        }       

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_targetEventEntry == null)
                Init(property);

            SetRects(position);

            _eventTargetObject = EditorGUI.ObjectField(_objectFieldRect, _eventTargetObject, typeof(Object), true);

            if (_methodDropdown.Target != _eventTargetObject)
            {
                _methodDropdown = new MethodsDropdown(new AdvancedDropdownState(), _eventTargetObject);
                _methodDropdown.OnItemSelected = OnMethodSelected;
            }

            string selectedName = "No Function";
            if (_selectedInfo != null)           
                selectedName = $" {_selectedInfo.Target.GetType().Name}.{_selectedInfo.Method.Name}";
            
            using (new EditorGUI.DisabledScope(_eventTargetObject == null))
            {
                if (GUI.Button(_popupFieldRect, selectedName, "MiniPopup"))
                {
                    _methodDropdown.Show(_popupFieldRect);
                }
            }

            // Draw parameters.
            if (_selectedInfo != null)
            {
                foreach (ParameterInfo parameter in _parameters)
                {
                    DrawParameterField(parameter);
                }
            }
        }

        private void Init(SerializedProperty property)
        {
            _property = property;
            _methodDropdown = new MethodsDropdown(new AdvancedDropdownState(), null);
            _methodDropdown.OnItemSelected = OnMethodSelected;
            _targetEventEntry = property.GetValue<EnhancedEventEntry>();

            if (_targetEventEntry.Delegate != null)
            {
                _eventTargetObject = _targetEventEntry.Delegate.Target as Object;
                _selectedInfo = new DelegateInfo(_eventTargetObject, _targetEventEntry.Delegate.Method);
                _parameters = _selectedInfo.Method.GetParameters();
            }
        }

        /// <summary>
        /// Sets all of the rects for the property.
        /// </summary>
        /// <param name="rect"></param>
        private void SetRects(Rect rect)
        {
            _objectFieldRect = new Rect(rect.x, rect.y, rect.width / 2, EditorGUIUtility.singleLineHeight);
            _popupFieldRect = new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, EditorGUIUtility.singleLineHeight);

            float yOffset = rect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float yOffsetPerParameter = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (_selectedInfo != null)
            {
                _parameterRects = new Rect[_parameters.Length];

                for (int i = 0; i < _parameterRects.Length; i++)
                {
                    _parameterRects[i] = new Rect(rect.x, yOffset, rect.width, EditorGUIUtility.singleLineHeight);
                    yOffset += yOffsetPerParameter;
                }
            }
        }

        private void OnMethodSelected()
        {
            if (_methodDropdown.SelectedInfo != null)
            {
                Undo.RecordObject(_property.serializedObject.targetObject, "Delegate created.");

                _selectedInfo = _methodDropdown.SelectedInfo;
                _parameters = _selectedInfo.Method.GetParameters();
                _targetEventEntry.ParameterValues = new object[_parameters.Length];

                for (int i = 0; i < _targetEventEntry.ParameterValues.Length; i++)
                {
                    if (_parameters[i].ParameterType.BaseType == typeof(Component))
                        _targetEventEntry.ParameterValues[i] = null;
                    else
                        _targetEventEntry.ParameterValues[i] = Activator.CreateInstance(_parameters[i].ParameterType);
                }
                
                CreateDelegate();
            }
            else
            {
                _selectedInfo = null;
                _parameters = null;
                _targetEventEntry.ParameterValues = null;
            }
        }

        /// <summary>
        /// Sets the Event Entry's Delegate using the data from _selectedInfo.
        /// </summary>
        private void CreateDelegate()
        {
            Type[] parameterTypes = _selectedInfo.Method.GetParameters().Select(x => x.ParameterType).ToArray();
            var parameterCount = parameterTypes.Length;

            Type delegateType = null;

            if (_selectedInfo.Method.ReturnType == typeof(void))
            {
                if (parameterCount == 0) delegateType = typeof(Action);
                else if (parameterCount == 1) delegateType = typeof(Action<>).MakeGenericType(parameterTypes);
                else if (parameterCount == 2) delegateType = typeof(Action<,>).MakeGenericType(parameterTypes);
                else if (parameterCount == 3) delegateType = typeof(Action<,,>).MakeGenericType(parameterTypes);
                else if (parameterCount == 4) delegateType = typeof(Action<,,,>).MakeGenericType(parameterTypes);
                else if (parameterCount == 5) delegateType = typeof(Action<,,,,>).MakeGenericType(parameterTypes);
            }
            else
            {
                parameterTypes = parameterTypes.Append(_selectedInfo.Method.ReturnType).ToArray();
                if (parameterCount == 0) delegateType = typeof(Func<>).MakeArrayType();
                else if (parameterCount == 1) delegateType = typeof(Func<,>).MakeGenericType(parameterTypes);
                else if (parameterCount == 2) delegateType = typeof(Func<,,>).MakeGenericType(parameterTypes);
                else if (parameterCount == 3) delegateType = typeof(Func<,,,>).MakeGenericType(parameterTypes);
                else if (parameterCount == 4) delegateType = typeof(Func<,,,,>).MakeGenericType(parameterTypes);
                else if (parameterCount == 5) delegateType = typeof(Func<,,,,,>).MakeGenericType(parameterTypes);
            }

            _targetEventEntry.Delegate = Delegate.CreateDelegate(delegateType, _selectedInfo.Target, _selectedInfo.Method);
        }

        // Draws the correct field for the parameter.
        private void DrawParameterField(ParameterInfo parameter)
        {
            Rect position = _parameterRects[parameter.Position];
            Type type = parameter.ParameterType;
            string name = ObjectNames.NicifyVariableName(parameter.Name);
            int index = parameter.Position;

            if (type == typeof(AnimationCurve))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.CurveField(position, name, (AnimationCurve)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(bool))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.Toggle(position, name, (bool)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(BoundsInt))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.BoundsIntField(position, name, (BoundsInt)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(Bounds))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.BoundsField(position, name, (Bounds)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(Color))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.ColorField(position, name, (Color)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(double))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.DoubleField(position, name, (double)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(float))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.FloatField(position, name, (float)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(int))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.IntField(position, name, (int)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(long))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.LongField(position, name, (long)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(Quaternion))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.Vector4Field(position, name, (Vector4)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(RectInt))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.RectIntField(position, name, (RectInt)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(Rect))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.RectField(position, name, (Rect)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(string))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.TextField(position, name, (string)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(Vector2Int))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.Vector2IntField(position, name, (Vector2Int)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(Vector2))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.Vector2Field(position, name, (Vector2)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(Vector3Int))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.Vector3IntField(position, name, (Vector3Int)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(Vector3))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.Vector3Field(position, name, (Vector3)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(Vector4))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.Vector4Field(position, name, (Vector4)_targetEventEntry.ParameterValues[index]);
            }
            else if (type.IsEnum)
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.EnumPopup(position, name, (Enum)_targetEventEntry.ParameterValues[index]);
            }
            else if (type == typeof(Object))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.ObjectField(position, name, (Object)_targetEventEntry.ParameterValues[index], type, true);
            }
            else if (type.BaseType == typeof(Component))
            {
                _targetEventEntry.ParameterValues[index] = EditorGUI.ObjectField(position, name, (Object)_targetEventEntry.ParameterValues[index], type, true);
            }
        }
    }
}
