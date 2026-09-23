namespace Clase07.MessageBroker.DIBroker
{
    public readonly struct ScorePickedUpEvent
    {
        public readonly int Amount;
        public ScorePickedUpEvent(int amount) => Amount = amount;
    }
}
