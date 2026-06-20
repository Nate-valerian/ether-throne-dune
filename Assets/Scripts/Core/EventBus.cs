using System;
using System.Collections.Generic;

// Decoupled event system — any system can publish or subscribe without knowing others
public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public static void Subscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type)) _handlers[type] = new List<Delegate>();
        _handlers[type].Add(handler);
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (_handlers.ContainsKey(type)) _handlers[type].Remove(handler);
    }

    public static void Publish<T>(T evt)
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type)) return;
        foreach (var handler in _handlers[type])
            (handler as Action<T>)?.Invoke(evt);
    }
}

// Events
public record GameStartedEvent();
public record TurnAdvancedEvent(int Turn);
public record RouteOpenedEvent(string FromSystemId, string ToSystemId, string FactionId);
public record RouteClosedEvent(string FromSystemId, string ToSystemId);
public record WarDeclaredEvent(string AttackerFactionId, string DefenderFactionId);
public record BattleResolvedEvent(string WinnerId, string LoserId, string SystemId);
public record CharacterMetEvent(string CharacterId);
public record RelationshipChangedEvent(string CharacterId, float Delta);
public record SaveLoadedEvent();
