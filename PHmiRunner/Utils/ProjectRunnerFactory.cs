using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.Runner;
using PHmiModel.Interfaces;
using PHmiRunner.Utils.Alarms;
using PHmiRunner.Utils.IoDeviceRunner;
using PHmiRunner.Utils.Logs;
using PHmiRunner.Utils.Trends;
using PHmiRunner.Utils.Users;
using PHmiRunner.Utils.Wcf;
using PHmiTools;
using PHmiTools.Utils.Npg;

namespace PHmiRunner.Utils {
    public class ProjectRunnerFactory : IProjectRunnerFactory {
        private readonly IModelContextFactory _contextFactory;
        private readonly INpgHelper _npgHelper;
        private readonly IReporter _reporter;
        private readonly ITimeService _timeService;

        public ProjectRunnerFactory(
            ITimeService timeService,
            IReporter reporter,
            IModelContextFactory contextFactory,
            INpgHelper npgHelper) {
            _timeService = timeService;
            _reporter = reporter;
            _contextFactory = contextFactory;
            _npgHelper = npgHelper;
        }

        public IProjectRunner Create(string projectName, string connectionString) {
            string db = _npgHelper.GetDatabase(connectionString);
            string dataDb = db + PHmiConstants.DataDatabaseEnding;
            string dataDbConStr = connectionString.Replace("Database=" + db, "Database=" + dataDb);
            IModelContext context = _contextFactory.Create(connectionString, false);
            return new ProjectRunner(
                projectName,
                context,
                _timeService,
                _reporter,
                dataDbConStr,
                new DataDbCreatorFactory(),
                new UsersRunnerFactory(),
                new ServiceRunnerFactory(),
                new CyclicRunnerFactory(),
                new IoDeviceRunTargetFactory(),
                new AlarmsRunTargetFactory(),
                new TrendsRunTargetFactory(),
                new LogMaintainerFactory());
        }
    }
}