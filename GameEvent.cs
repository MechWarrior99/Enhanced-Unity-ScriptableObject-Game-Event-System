using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace Bewildered.Events
{
    [CreateAssetMenu(order = 100)]
    public class GameEvent : ScriptableObject
    {
        private List<GameEventListener> _listeners = new List<GameEventListener>();
#if UNITY_EDITOR
        private List<StackTrace> _stackTraces = new List<StackTrace>();
#endif

        /// <summary>
        /// Raises the event. Invoking the Response from all registered GameEventListeners.
        /// </summary>
        public void Raise()
        {
#if UNITY_EDITOR
            _stackTraces.Add(new StackTrace(true));
#endif

            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[0].OnEventRaised();
            }
        }

        internal void RegisterListener(GameEventListener listener)
        {
            _listeners.Add(listener);
        }

        internal void UnregisterListener(GameEventListener listener)
        {
            _listeners.Remove(listener);
        }
    }
}