using System;
using NUnit.Framework;

namespace PHmiClientUnitTests {
    public abstract class Specification {
        protected bool CatchExceptionInEstablishContext = false;
        protected Exception ThrownException;

        [SetUp]
        public void SetUp() {
            ThrownException = null;
            try {
                EstablishContext();
            } catch (Exception exception) {
                ThrownException = exception;
                if (!CatchExceptionInEstablishContext)
                    throw;
            }
        }

        protected virtual void EstablishContext() { }

        [TearDown]
        public void TearDown() {
            AfterEach();
        }

        protected virtual void AfterEach() { }
    }
}