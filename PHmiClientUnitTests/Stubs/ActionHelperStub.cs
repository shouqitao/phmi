using System;
using PHmiClient.Utils;

namespace PHmiClientUnitTests.Stubs {
    public class ActionHelperStub : IActionHelper {
        public void Async(Action action) {
            action.Invoke();
        }

        public void Dispatch(Action action) {
            action.Invoke();
        }

        public void DispatchAsync(Action action) {
            throw new NotImplementedException();
        }
    }
}