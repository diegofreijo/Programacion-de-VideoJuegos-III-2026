using System;
using System.Collections.Generic;

namespace Clase07.DI.ServiceLocatorPattern
{
    // Registro central: los consumidores piden lo que necesitan por tipo en vez
    // de recibirlo. A diferencia de VContainer, las dependencias de una clase
    // quedan ocultas — no aparecen en su constructor.
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

        public static void Register<T>(T instance) => Services[typeof(T)] = instance;

        public static T Resolve<T>()
        {
            if (!Services.TryGetValue(typeof(T), out var instance))
            {
                throw new InvalidOperationException($"No hay un servicio registrado para {typeof(T)}");
            }
            return (T)instance;
        }

        public static void Clear() => Services.Clear();
    }
}
