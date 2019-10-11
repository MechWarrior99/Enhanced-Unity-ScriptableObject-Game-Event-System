using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [SerializeField]
    private GameEvent _event;
    [SerializeField]
    private UnityEvent _response;

    /// <summary>
    /// The event to listen for to be raised.
    /// </summary>
    public GameEvent Event
    {
        get { return _event; }
        set { _event = value; }
    }

    /// <summary>
    /// The action to take when the GameEvent is raised.
    /// </summary>
    public UnityEvent Response
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
