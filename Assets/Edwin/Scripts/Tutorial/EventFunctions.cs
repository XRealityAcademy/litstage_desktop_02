using System;
using System.Collections.Generic;
using UnityEngine; // Needed for Debug.Log

public static class EventFunctions
{
    private static Dictionary<string, Action> eventTable = new Dictionary<string, Action>();

    public static void SendEvent(string eventName)
    {
        Debug.Log($"EDWIN_DEBUG: Sending event: {eventName}");
        if (eventTable.TryGetValue(eventName, out var thisEvent))
        {
            var count = thisEvent?.GetInvocationList().Length ?? 0;
            Debug.Log($"EDWIN_DEBUG: Invoking event: {eventName} with {count} listeners");
            try
            {
                thisEvent.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"EDWIN_DEBUG: Exception while invoking event '{eventName}': {ex}");
            }
        }
        else
        {
            Debug.Log($"EDWIN_DEBUG: No listeners found for event: {eventName}");
        }
    }

    public static void ListenEvent(string eventName, Action listener)
    {
        if (eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] += listener;
            Debug.Log($"EDWIN_DEBUG: Added listener to event: {eventName} ({listener.Method.DeclaringType}.{listener.Method.Name}) - Listener count: {eventTable[eventName]?.GetInvocationList().Length}");
        }
        else
        {
            eventTable[eventName] = listener;
            Debug.Log($"EDWIN_DEBUG: Created new event entry for: {eventName} ({listener.Method.DeclaringType}.{listener.Method.Name})");
        }
    }

    public static void RemoveListener(string eventName, Action listener)
    {
        if (eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] -= listener;
            var count = eventTable[eventName]?.GetInvocationList().Length ?? 0;
            Debug.Log($"EDWIN_DEBUG: Removed listener for event: {eventName} ({listener.Method.DeclaringType}.{listener.Method.Name}) - Remaining listeners: {count}");
            if (eventTable[eventName] == null)
            {
                Debug.Log($"EDWIN_DEBUG: No more listeners for event: {eventName}, removing from table.");
                eventTable.Remove(eventName);
            }
        }
        else
        {
            Debug.Log($"EDWIN_DEBUG: Tried to remove listener for event not in table: {eventName} ({listener.Method.DeclaringType}.{listener.Method.Name})");
        }
    }

    // ---- GENERIC EVENTS ----
    private static Dictionary<string, Delegate> genericEventTable = new Dictionary<string, Delegate>();

    public static void SendEvent<T>(string eventName, T data)
    {
        Debug.Log($"EDWIN_DEBUG: Sending event with data: {eventName} - Type: {typeof(T)}");
        if (genericEventTable.TryGetValue(eventName, out var thisEvent))
        {
            var typedEvent = thisEvent as Action<T>;
            if (typedEvent != null)
            {
                try
                {
                    typedEvent.Invoke(data);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"EDWIN_DEBUG: Exception while invoking event '{eventName}': {ex}");
                }
            }
        }
        else
        {
            Debug.Log($"EDWIN_DEBUG: No listeners found for data event: {eventName}");
        }
    }

    public static void ListenEvent<T>(string eventName, Action<T> listener)
    {
        if (genericEventTable.TryGetValue(eventName, out var thisEvent))
        {
            genericEventTable[eventName] = Delegate.Combine(thisEvent, listener);
            Debug.Log($"EDWIN_DEBUG: Added data-listener to event: {eventName} - Listener count: {genericEventTable[eventName]?.GetInvocationList().Length}");
        }
        else
        {
            genericEventTable[eventName] = listener;
            Debug.Log($"EDWIN_DEBUG: Created new data-event entry for: {eventName}");
        }
    }

    public static void RemoveListener<T>(string eventName, Action<T> listener)
    {
        if (genericEventTable.TryGetValue(eventName, out var thisEvent))
        {
            genericEventTable[eventName] = Delegate.Remove(thisEvent, listener);
            Debug.Log($"EDWIN_DEBUG: Removed data-listener for event: {eventName} - Remaining listeners: {genericEventTable[eventName]?.GetInvocationList().Length}");
            if (genericEventTable[eventName] == null)
            {
                genericEventTable.Remove(eventName);
                Debug.Log($"EDWIN_DEBUG: No more data-listeners for event: {eventName}, removed from table.");
            }
        }
        else
        {
            Debug.Log($"EDWIN_DEBUG: Tried to remove data-listener for event not in table: {eventName}");
        }
    }
}