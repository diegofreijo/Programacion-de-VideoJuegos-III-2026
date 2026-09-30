using System;

namespace Clase08
{
    public static class CalculadoraDeDanio
    {
        public static int Calcular(int ataque, int defensa)
        {
            return Math.Max(0, ataque - defensa);
        }
    }
}
