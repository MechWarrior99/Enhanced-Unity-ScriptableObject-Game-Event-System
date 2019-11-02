using System.Collections.Generic;
using System;
using UnityEngine;

namespace Bewildered.Events
{
    /// <summary>
    /// A persistant callback that can be saved with the scene.
    /// </summary>
    [Serializable]
    public class EnhancedEvent
    {
        [SerializeField, HideInInspector]
        private List<EnhancedEventEntry> _events = new List<EnhancedEventEntry>();

        public void Invoke()
        {
            if (_events == null)
                return;

            for (int i = 0; i < _events.Count; i++)
            {
                _events[i].Invoke();
            }
        }
    }
}