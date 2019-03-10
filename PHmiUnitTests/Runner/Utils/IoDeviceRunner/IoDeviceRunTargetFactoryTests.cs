using Moq;
using NUnit.Framework;
using PHmiClient.Utils;
using PHmiClientUnitTests;
using PHmiModel.Entities;
using PHmiRunner.Utils.IoDeviceRunner;

namespace PHmiUnitTests.Runner.Utils.IoDeviceRunner {
    public class WhenUsingIoDeviceRunTargetFactory : Specification {
        protected IIoDeviceRunTargetFactory Factory;

        protected override void EstablishContext() {
            base.EstablishContext();
            Factory = new IoDeviceRunTargetFactory();
        }

        public class AndCallingCreate : WhenUsingIoDeviceRunTargetFactory {
            protected IIoDeviceRunTarget Target;
            protected Mock<ITimeService> TimeService;

            protected override void EstablishContext() {
                base.EstablishContext();
                TimeService = new Mock<ITimeService>();
                Target = Factory.Create(TimeService.Object, new IoDevice {Type = "Type"});
            }

            public class ThenTargetIsCreated : AndCallingCreate {
                [Test]
                public void Test() {
                    Assert.That(Target, Is.Not.Null);
                }
            }
        }
    }
}