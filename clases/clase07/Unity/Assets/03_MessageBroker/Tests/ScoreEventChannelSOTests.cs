using NUnit.Framework;
using UnityEngine;
using Clase07.MessageBroker.ScriptableObjectChannels;

namespace Clase07.MessageBroker.Tests
{
    public class ScoreEventChannelSOTests
    {
        [Test]
        public void Raise_NotifiesAllListeners()
        {
            var channel = ScriptableObject.CreateInstance<ScoreEventChannelSO>();
            var received = new System.Collections.Generic.List<int>();
            channel.OnRaised += amount => received.Add(amount);
            channel.OnRaised += amount => received.Add(amount * 10);

            channel.Raise(5);

            CollectionAssert.AreEqual(new[] { 5, 50 }, received);
        }

        [Test]
        public void Raise_WithNoListeners_DoesNotThrow()
        {
            var channel = ScriptableObject.CreateInstance<ScoreEventChannelSO>();
            Assert.DoesNotThrow(() => channel.Raise(5));
        }
    }
}
