using UnityEngine;
using System.Reflection;

namespace Bewildered.Editors.Events
{
    [System.Serializable]
    internal class DelegateInfo
    {
        public Object Target;
        public MethodInfo Method;

        public DelegateInfo(Object target, MethodInfo method)
        {
            Target = target;
            Method = method;
        }
    }
}