using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.Serialization;

namespace Bewildered.Events
{
    [Serializable]
    public class EnhancedEventEntry : ISerializationCallbackReceiver
    {
        [NonSerialized, HideInInspector]
        private Delegate _delegate;

        [NonSerialized, HideInInspector]
        private object[] _parameterValues;

        public Delegate Delegate
        {
            get { return _delegate; }
            set { _delegate = value; }
        }

        public object[] ParameterValues
        {
            get { return _parameterValues; }
            set { _parameterValues = value; }
        }

        public EnhancedEventEntry()
        {

        }

        public EnhancedEventEntry(Delegate del)
        {
            if (del != null && del.Method != null)
            {
                _delegate = del;
                _parameterValues = new object[del.Method.GetParameters().Length];
            }
        }

        public void Invoke()
        {
            if (_delegate != null && _parameterValues != null)
            {
                _delegate.Method.Invoke(_delegate.Target, _parameterValues);
            }
        }

        #region OdinSerialization
        [SerializeField, HideInInspector]
        private List<UnityEngine.Object> _unityReferences;

        [SerializeField, HideInInspector]
        private byte[] bytes;

        public void OnAfterDeserialize()
        {
            SerializedData val = SerializationUtility.DeserializeValue<SerializedData>(bytes, DataFormat.Binary, _unityReferences);
            _delegate = val.Delegate;
            _parameterValues = val.ParameterValues;
        }

        public void OnBeforeSerialize()
        {
            SerializedData val = new SerializedData() { Delegate = _delegate, ParameterValues = _parameterValues };
            bytes = SerializationUtility.SerializeValue(val, DataFormat.Binary, out _unityReferences);
        }

        private struct SerializedData
        {
            public Delegate Delegate;
            public object[] ParameterValues;
        }
        #endregion
    }
}
