using System.Collections.Generic;
using UnityEngine;

public abstract class abstractEvent<T> : ScriptableObject
{
    public T testingValue;

    // The list of the listeners to this event
    private List<abstractEventListener<T>> listeners = new List<abstractEventListener<T>>();


    // How new listeners are registered
    public void register(abstractEventListener<T> listener)
    {

        if (listeners == null)
        {
            listeners = new List<abstractEventListener<T>>();
        }
        if (listener == null)
        {
            return;
        }
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }

    // How new listeners are deregistered
    public void deregister(abstractEventListener<T> listener)
    {

        if (listeners == null)
        {
            listeners = new List<abstractEventListener<T>>();
        }
        if (listener == null)
        {
            return;
        }
        if (listeners.Contains(listener) && listeners != null)
        {
            listeners.Remove(listener);
        }
    }

    // How new listeners are notified
    public void Invoke(T value)
    {
        if (listeners != null && value != null)
        {
            foreach (abstractEventListener<T> listener in listeners)
            {
                listener.listen(value);
            }
        }
    }


}
