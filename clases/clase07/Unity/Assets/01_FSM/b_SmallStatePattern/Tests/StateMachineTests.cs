using System.Collections.Generic;
using NUnit.Framework;
using Clase07.FSM.SmallStatePattern;

namespace Clase07.FSM.SmallStatePattern.Tests
{
    public class StateMachineTests
    {
        private class RecordingState : IState
        {
            public readonly List<string> Calls = new List<string>();
            private readonly string _name;
            public RecordingState(string name) => _name = name;
            public void OnEnter() => Calls.Add($"{_name}:Enter");
            public void OnUpdate(float deltaTime) => Calls.Add($"{_name}:Update");
            public void OnExit() => Calls.Add($"{_name}:Exit");
        }

        [Test]
        public void Constructor_CallsOnEnterOnInitialState()
        {
            var a = new RecordingState("A");
            new StateMachine<IState>(a);
            CollectionAssert.AreEqual(new[] { "A:Enter" }, a.Calls);
        }

        [Test]
        public void ChangeState_ExitsPreviousThenEntersNext()
        {
            var a = new RecordingState("A");
            var b = new RecordingState("B");
            var machine = new StateMachine<IState>(a);

            machine.ChangeState(b);

            CollectionAssert.AreEqual(new[] { "A:Enter", "A:Exit" }, a.Calls);
            CollectionAssert.AreEqual(new[] { "B:Enter" }, b.Calls);
            Assert.AreSame(b, machine.CurrentState);
        }

        [Test]
        public void ChangeState_ToSameState_IsNoOp()
        {
            var a = new RecordingState("A");
            var machine = new StateMachine<IState>(a);

            machine.ChangeState(a);

            CollectionAssert.AreEqual(new[] { "A:Enter" }, a.Calls);
        }

        [Test]
        public void Tick_CallsOnUpdateOnCurrentState()
        {
            var a = new RecordingState("A");
            var machine = new StateMachine<IState>(a);

            machine.Tick(0.016f);

            CollectionAssert.AreEqual(new[] { "A:Enter", "A:Update" }, a.Calls);
        }

        [Test]
        public void StateChanged_FiresWithPreviousAndNext()
        {
            var a = new RecordingState("A");
            var b = new RecordingState("B");
            var machine = new StateMachine<IState>(a);
            IState firedPrevious = null, firedNext = null;
            machine.StateChanged += (prev, next) => { firedPrevious = prev; firedNext = next; };

            machine.ChangeState(b);

            Assert.AreSame(a, firedPrevious);
            Assert.AreSame(b, firedNext);
        }
    }
}
