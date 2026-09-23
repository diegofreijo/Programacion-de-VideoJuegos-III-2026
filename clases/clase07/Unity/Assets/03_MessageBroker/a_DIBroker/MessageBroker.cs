using System;
using System.Collections.Generic;

namespace Clase07.MessageBroker.DIBroker
{
    public class MessageBroker : IMessageBroker
    {
        private class Subscription : IDisposable
        {
            private readonly Action _onDispose;
            public Subscription(Action onDispose) => _onDispose = onDispose;
            public void Dispose() => _onDispose();
        }

        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public IDisposable Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _handlers[type] = list;
            }
            list.Add(handler);
            return new Subscription(() => list.Remove(handler));
        }

        public void Publish<T>(T message)
        {
            if (!_handlers.TryGetValue(typeof(T), out var list)) return;
            // ToArray() copia la lista antes de iterar: si un handler se desuscribe
            // (Dispose) durante su propio callback, no rompe la iteración en curso.
            foreach (var handler in list.ToArray())
            {
                ((Action<T>)handler).Invoke(message);
            }
        }
    }
}
