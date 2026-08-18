using System;
using System.Collections.Generic;

namespace HappyShoot.Domain.Events
{
    /// <summary>
    /// Marker interface for all domain events.
    /// </summary>
    public interface IDomainEvent
    {
    }

    /// <summary>
    /// High-performance, decoupled event dispatcher for domain-to-view and inter-domain communications.
    /// </summary>
    public class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>(32);

        /// <summary>
        /// Subscribes a handler to a specific event type.
        /// </summary>
        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            Type eventType = typeof(TEvent);
            if (!_subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<Delegate>(4);
                _subscribers[eventType] = handlers;
            }

            if (!handlers.Contains(handler))
            {
                handlers.Add(handler);
            }
        }

        /// <summary>
        /// Unsubscribes a handler from a specific event type.
        /// </summary>
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent
        {
            if (handler == null) return;

            Type eventType = typeof(TEvent);
            if (_subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
                if (handlers.Count == 0)
                {
                    _subscribers.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// Publishes an event to all subscribed handlers.
        /// </summary>
        public void Publish<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
        {
            Type eventType = typeof(TEvent);
            if (_subscribers.TryGetValue(eventType, out var handlers))
            {
                for (int i = handlers.Count - 1; i >= 0; i--)
                {
                    if (handlers[i] is Action<TEvent> action)
                    {
                        action.Invoke(domainEvent);
                    }
                }
            }
        }

        /// <summary>
        /// Clears all event subscriptions.
        /// </summary>
        public void Clear()
        {
            _subscribers.Clear();
        }
    }
}
