using System;
using Npgsql;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiModel.Entities;
using PHmiTools.Utils.Npg;

namespace PHmiRunner.Utils.Alarms {
    public class AlarmsRunTargetFactory : IAlarmsRunTargetFactory {
        public IAlarmsRunTarget Create(string connectionString, IProject project, AlarmCategory alarmCategory,
            ITimeService timeService) {
            var npgsqlConnectionFactory = new NpgsqlConnectionFactory(connectionString);
            var alarmsRepository = new AlarmsRepository(alarmCategory.Id);
            using (NpgsqlConnection connection = npgsqlConnectionFactory.Create()) {
                alarmsRepository.EnsureTable(connection);
            }

            return new AlarmsRunTarget(
                alarmCategory,
                new NotificationReporter(timeService) {LifeTime = TimeSpan.FromTicks(0)},
                alarmsRepository,
                project,
                timeService,
                npgsqlConnectionFactory);
        }
    }
}