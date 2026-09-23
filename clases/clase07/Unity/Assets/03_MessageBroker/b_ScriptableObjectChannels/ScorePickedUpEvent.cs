namespace Clase07.MessageBroker.ScriptableObjectChannels
{
    public readonly struct ScorePickedUpEvent
    {
        public readonly int Amount;
        public ScorePickedUpEvent(int amount) => Amount = amount;
    }
}
