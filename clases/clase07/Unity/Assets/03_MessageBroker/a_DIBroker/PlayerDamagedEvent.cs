namespace Clase07.MessageBroker.DIBroker
{
    public readonly struct PlayerDamagedEvent
    {
        public readonly int Amount;
        public PlayerDamagedEvent(int amount) => Amount = amount;
    }
}
