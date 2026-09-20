namespace Clase07.MessageBroker.Shared
{
    public readonly struct PlayerDamagedEvent
    {
        public readonly int Amount;
        public PlayerDamagedEvent(int amount) => Amount = amount;
    }
}
