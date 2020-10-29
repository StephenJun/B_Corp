using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dispatcher
{
    private Dictionary<string, List<Action>> listeners;

	public Dispatcher()
    {
		listeners = new Dictionary<string, List<Action>>();
	}

	public void Add(string eventName, Action callback)
    {
		listeners[eventName].Add(callback);
	}

	public void Remove(string eventName, Action callback)
    {
        if (listeners.ContainsKey(eventName))
        {
			listeners[eventName].Remove(callback);
		}
	}

	public bool HasListener(string eventName, Action listener)
    {
		if (listeners.ContainsKey(eventName))
			return listeners[eventName].Contains(listener);
		return false;
	}

	public void Dispatch(string eventName)
    {
        foreach (var listener in listeners[eventName])
        {
			listener();
		}
    }
}
