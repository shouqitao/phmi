using PHmiClient.Utils;
using PHmiModel.Entities;

namespace PHmiRunner.Utils.IoDeviceRunner {
    public interface IIoDeviceRunTargetFactory {
        IIoDeviceRunTarget Create(ITimeService timeService, IoDevice ioDevice);
    }
}