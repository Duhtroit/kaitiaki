using UnityEngine;
using UnityEngine.Events;

public class abstractEventListener<T> : MonoBehaviour
{
    public abstractEvent<T> eventToListen;
    public UnityEvent<T> onEvent = new UnityEvent<T>();

    public void OnEnable()
    {
        eventToListen.register(this);
    }

    public void OnDisable()
    {
        eventToListen.deregister(this);
    }

    public void listen(T value)
    {
        onEvent?.Invoke(value);
    }
}
