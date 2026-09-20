using System;

namespace Clase07.MessageBroker.DIBroker
{
    public interface IMessageBroker
    {
        IDisposable Subscribe<T>(Action<T> handler);
        void Publish<T>(T message);
    }
}
