using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;

namespace AvaloniaXKCD.Browser;

public delegate void CSharpGenericEventHandler<T>(T data);

public readonly struct Handler<T>
{
    public object OriginalHandler { get; }

    public Action<T> WrappedHandler { get; }

    private Handler(object original, Action<T> wrapped)
    {
        OriginalHandler = original;
        WrappedHandler = wrapped;
    }

    public static implicit operator Handler<T>(Action<T> action) => new(action, action);

    public static implicit operator Handler<T>(EventHandler<T> eventHandler) =>
        new(eventHandler, data => eventHandler(null, data));

    public static implicit operator Handler<T>(CSharpGenericEventHandler<T> csharpGenericEventHandler) =>
        new(csharpGenericEventHandler, data => csharpGenericEventHandler(data));
}

public abstract class EventBridge
{
    public abstract void Invoke(object data);
}

public class CSharpGenericEventBridge<T> : EventBridge
{
    public event CSharpGenericEventHandler<T>? Handler;

    private readonly ConditionalWeakTable<object, CSharpGenericEventHandler<T>> _subscriptions = new();

    public string Add(Action<T> callback)
    {
        var subscriptionId = Guid.NewGuid().ToString();
        var handler = new CSharpGenericEventHandler<T>(callback);
        _subscriptions.Add(subscriptionId, handler);
        Handler += handler;
        return subscriptionId;
    }

    public void Remove(string subscriptionId)
    {
        if (_subscriptions.TryGetValue(subscriptionId, out var handler))
        {
            Handler -= handler;
            _subscriptions.Remove(subscriptionId);
        }
    }

    public void Add(Handler<T> handler)
    {
        var delegateToAdd = new CSharpGenericEventHandler<T>(handler.WrappedHandler);
        if (_subscriptions.TryAdd(handler.OriginalHandler, delegateToAdd))
        {
            Handler += delegateToAdd;
        }
    }

    public void Remove(Handler<T> handler)
    {
        if (_subscriptions.TryGetValue(handler.OriginalHandler, out var storedDelegate))
        {
            Handler -= storedDelegate;
            _subscriptions.Remove(handler.OriginalHandler);
        }
    }

    public void Invoke(T data)
    {
        Handler?.Invoke(data);
    }

    public override void Invoke(object data)
    {
        if (data is T typedData)
        {
            Invoke(typedData);
        }
        else
        {
            throw new ArgumentException($"Data is not of the expected type {typeof(T).Name}", nameof(data));
        }
    }
}

public static partial class CSharpEventBridgeFactory
{
    private static readonly Dictionary<string, EventBridge> _bridges = new();
    private static readonly object _lock = new();

    public static CSharpGenericEventBridge<T>? GetBridge<T>(string handlerName)
    {
        lock (_lock)
        {
            if (!_bridges.TryGetValue(handlerName, out var bridge))
            {
                bridge = new CSharpGenericEventBridge<T>();
                _bridges[handlerName] = bridge;
            }
            return bridge as CSharpGenericEventBridge<T>;
        }
    }

    public static string? Add<T>(string handlerName, Action<T> callback) => GetBridge<T>(handlerName)?.Add(callback);

    public static void Remove<T>(string handlerName, string subscription) =>
        GetBridge<T>(handlerName)?.Remove(subscription);

    public static void Invoke<T>(string handlerName, T data) => GetBridge<T>(handlerName)?.Invoke(data);
}
