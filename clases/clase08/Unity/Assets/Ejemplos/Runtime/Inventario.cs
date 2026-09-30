using System.Collections.Generic;

namespace Clase08
{
    public class Inventario
    {
        private readonly HashSet<string> llaves = new HashSet<string>();

        public void RecogerLlave(string id)
        {
            llaves.Add(id);
        }

        public bool TieneLlave(string id)
        {
            return llaves.Contains(id);
        }
    }
}
