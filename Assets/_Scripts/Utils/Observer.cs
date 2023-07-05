using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Observer
{
    private static Dictionary<string, List<Action>> eventsCollection;

    public static void RegisterCustomEvent(string eventName, Action action)
    {
        if (eventsCollection == null)
            eventsCollection = new Dictionary<string, List<Action>>();

        if (!eventsCollection.ContainsKey(eventName))
            eventsCollection[eventName] = new List<Action>();

        eventsCollection[eventName].Add(action);
    }

    public static void DispatchCustomEvent(string eventName)
    {
        if (!eventsCollection.ContainsKey(eventName))
            return;

        foreach(Action action in eventsCollection[eventName])
        {
            action?.Invoke();
        }
    }

    public static void RemoveCustomEvent(string eventName, Action action)
    {
        eventsCollection[eventName].Remove(action);

        if (eventsCollection[eventName].Count == 0)
            eventsCollection.Remove(eventName);
    }
}
