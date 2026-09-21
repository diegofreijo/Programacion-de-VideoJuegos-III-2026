using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Clase07.MessageBroker.ScriptableObjectChannels;
using Clase07.MessageBroker.Shared;

namespace Clase07.MessageBroker.Tests
{
    public class ScoreEventChannelSOTests
    {
        [Test]
        public void Raise_NotifiesAllListeners()
        {
            var channel = ScriptableObject.CreateInstance<ScoreEventChannelSO>();
            var received = new List<int>();
            channel.OnRaised += evt => received.Add(evt.Amount);
            channel.OnRaised += evt => received.Add(evt.Amount * 10);

            channel.Raise(new ScorePickedUpEvent(5));

            CollectionAssert.AreEqual(new[] { 5, 50 }, received);
        }

        [Test]
        public void Raise_WithNoListeners_DoesNotThrow()
        {
            var channel = ScriptableObject.CreateInstance<ScoreEventChannelSO>();
            Assert.DoesNotThrow(() => channel.Raise(new ScorePickedUpEvent(5)));
        }
    }
}
