using System;

namespace Clase07.MessageBroker.DIBroker
{
    // El contrato completo de un broker pub/sub cabe en dos métodos: Subscribe
    // devuelve un IDisposable que hay que guardar y disponer para cancelar la
    // suscripción (si no, la suscripción queda viva para siempre).
    public interface IMessageBroker
    {
        IDisposable Subscribe<T>(Action<T> handler);
        void Publish<T>(T message);
    }
}
