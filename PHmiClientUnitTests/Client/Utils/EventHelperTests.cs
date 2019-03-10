using System;
using System.ComponentModel;
using NUnit.Framework;
using PHmiClient.Utils;

namespace PHmiClientUnitTests.Client.Utils {
    [TestFixture]
    public class EventHelperTests {
        [SetUp]
        public void SetUp() {
            _raised = false;
        }

        private bool _raised;

        private event EventHandler Event;

        private event EventHandler<EventArgs> EventGeneric;

        private event PropertyChangedEventHandler PropertyChangedEvent;

        [Test]
        public void RaiseGenericTest() {
            EventHandler<EventArgs> handler = (sender, args) => { _raised = true; };
            EventGeneric += handler;
            bool result = EventHelper.Raise(ref EventGeneric, this, EventArgs.Empty);
            EventGeneric -= handler;
            Assert.IsTrue(result);
            Assert.IsTrue(_raised);
        }

        [Test]
        public void RaiseNotGenericTest() {
            EventHandler<EventArgs> handler = (sender, args) => { _raised = true; };
            EventGeneric += handler;
            EventGeneric -= handler;
            bool result = EventHelper.Raise(ref EventGeneric, this, EventArgs.Empty);
            Assert.IsFalse(result);
            Assert.IsFalse(_raised);
        }

        [Test]
        public void RaiseNotPropertyChangedTest() {
            PropertyChangedEventHandler handler = (sender, args) => { _raised = true; };
            PropertyChangedEvent += handler;
            PropertyChangedEvent -= handler;
            bool result = EventHelper.Raise(
                ref PropertyChangedEvent, this, new PropertyChangedEventArgs(null));
            Assert.IsFalse(result);
            Assert.IsFalse(_raised);
        }

        [Test]
        public void RaiseNotTest() {
            EventHandler handler = (sender, args) => { _raised = true; };
            Event += handler;
            Event -= handler;
            bool result = EventHelper.Raise(ref Event, this, EventArgs.Empty);
            Assert.IsFalse(result);
            Assert.IsFalse(_raised);
        }

        [Test]
        public void RaisePropertyChangedTest() {
            PropertyChangedEventHandler handler = (sender, args) => { _raised = true; };
            PropertyChangedEvent += handler;
            bool result = EventHelper.Raise(
                ref PropertyChangedEvent, this, new PropertyChangedEventArgs(null));
            PropertyChangedEvent -= handler;
            Assert.IsTrue(result);
            Assert.IsTrue(_raised);
        }

        [Test]
        public void RaiseTest() {
            EventHandler handler = (sender, args) => { _raised = true; };
            Event += handler;
            bool result = EventHelper.Raise(ref Event, this, EventArgs.Empty);
            Event -= handler;
            Assert.IsTrue(result);
            Assert.IsTrue(_raised);
        }
    }
}