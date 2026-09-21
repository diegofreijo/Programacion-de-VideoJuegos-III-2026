namespace Clase07.MessageBroker.Shared
{
    public readonly struct ScorePickedUpEvent
    {
        public readonly int Amount;
        public ScorePickedUpEvent(int amount) => Amount = amount;
    }
}
