using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor.IMGUI.Controls;
using Object = UnityEngine.Object;

namespace Bewildered.Editors.Events
{
    /// <summary>
    /// A dropdown of all avalible methods in a GameObject and it's components.
    /// </summary>
    internal class MethodsDropdown : AdvancedDropdown
    {
        private Object _target;
        private GameObject _targetGameObject;
        private Component[] _components;
        private DelegateInfo _selectedInfo;
        private Action _onItemSelected;

        public Object Target
        {
            get { return _target; }
        }
        public DelegateInfo SelectedInfo
        {
            get { return _selectedInfo; }
            set { _selectedInfo = value; }
        }
        public Action OnItemSelected
        {
            get { return _onItemSelected; }
            set { _onItemSelected = value; }
        }

        private class MethodDropdownItem : AdvancedDropdownItem
        {
            private DelegateInfo _delegateInfo;

            public DelegateInfo DelegateInfo
            {
                get { return _delegateInfo; }
            }

            public MethodDropdownItem(string name) : base(name)
            {
                this.name = name;
            }

            public MethodDropdownItem(string name, DelegateInfo delegateInfo) : base(name)
            {
                this.name = name;
                _delegateInfo = delegateInfo;
            }
        }


        public MethodsDropdown(AdvancedDropdownState state, Object target) : base(state)
        {
            _target = target;
            minimumSize = new Vector2(minimumSize.x, 300);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new MethodDropdownItem("Components");
            root.AddChild(new MethodDropdownItem("No Function"));

            SetTargetGameObject();

            if (_targetGameObject)
            {
                foreach (Component component in _components)
                {
                    MethodDropdownItem componentItem = new MethodDropdownItem(component.GetType().Name);
                    SetMethodsForObject(component, componentItem);
                    root.AddChild(componentItem);
                }
            }

            return root;
        }

        // Sets _targetGameObject, and populates _components.
        private void SetTargetGameObject()
        {
            if (_target is GameObject)
            {
                _targetGameObject = _target as GameObject;
            }
            else if (_target is Component)
            {
                _targetGameObject = (_target as Component).gameObject;
            }
            else
            {
                return;
            }

            _components = _targetGameObject.GetComponents<Component>();
        }

        private void SetMethodsForObject(Object target, MethodDropdownItem parentItem)
        {
            MethodInfo[] methods = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName).ToArray();

            foreach (MethodInfo method in methods)
            {
                string name = method.Name + "(";

                ParameterInfo[] parameters = method.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    name += parameters[i].ParameterType.Name;

                    if (i < parameters.Length - 1)
                        name += ", ";
                }
                name += ")";

                parentItem.AddChild(new MethodDropdownItem(name, new DelegateInfo(target, method)));
            }
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            _selectedInfo = (item as MethodDropdownItem).DelegateInfo;
            _onItemSelected.Invoke();
        }
    }
}
