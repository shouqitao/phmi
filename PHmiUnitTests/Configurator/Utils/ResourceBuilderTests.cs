using System.Resources;
using Moq;
using PHmiClientUnitTests;
using PHmiConfigurator.Utils;

namespace PHmiUnitTests.Configurator.Utils {
    public class WhenUsingResourceBuilder : Specification {
        protected IResourceBuilder ResourceBuilder;
        protected Mock<IResourceWriter> ResourceWriter;

        protected override void EstablishContext() {
            base.EstablishContext();
            ResourceWriter = new Mock<IResourceWriter>();
            ResourceBuilder = new ResourceBuilder(ResourceWriter.Object);
        }
    }
}