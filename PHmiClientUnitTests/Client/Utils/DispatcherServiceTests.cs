using System.Threading;
using NUnit.Framework;
using PHmiClient.Utils;

namespace PHmiClientUnitTests.Client.Utils {
    public class WhenWorkingWithDispatcherService : Specification {
        protected Thread CurrentThread;
        protected IDispatcherService DispatcherService;

        protected Thread ExecutionThread;

        protected override void EstablishContext() {
            base.EstablishContext();
            DispatcherService = new DispatcherService();
            CurrentThread = Thread.CurrentThread;
        }

        public class NotInWpfApplication : WhenWorkingWithDispatcherService {
            protected override void EstablishContext() {
                base.EstablishContext();
                DispatcherService.Invoke(Execute);
            }

            [Test]
            public void ItShouldExecuteOnCurrentThread() {
                Assert.That(CurrentThread, Is.EqualTo(ExecutionThread));
            }

            private void Execute() {
                ExecutionThread = Thread.CurrentThread;
            }
        }
    }
}