using UnityEngine;

namespace Bewildered.Events
{
    public class GameEventListener : MonoBehaviour
    {
        [SerializeField]
        private GameEvent _event;
        [SerializeField]
        private EnhancedEvent _response;

        /// <summary>
        /// The event to listen for to be raised.
        /// </summary>
        public GameEvent Event
        {
            get { return _event; }
            set { _event = value; }
        }

        /// <summary>
        /// The action(s) to take when the GameEvent is raised.
        /// </summary>
        public EnhancedEvent Response
        {
            get { return _response; }
            set { _response = value; }
        }


        /// <summary>
        /// Called from the registered GameEvent when the event is raised.
        /// </summary>
        internal void OnEventRaised()
        {
            _response.Invoke();
        }

        private void OnEnable()
        {
            _event.RegisterListener(this);
        }

        private void OnDisable()
        {
            _event.UnregisterListener(this);
        }
    }
}
