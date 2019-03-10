using System.Globalization;
using System.Linq;
using Npgsql;
using PHmiClient.Converters;
using PHmiClient.Users;
using PHmiClient.Utils.Pagination;
using PHmiModel.Interfaces;
using PHmiTools.Utils.Npg;

namespace PHmiRunner.Utils.Users {
    public class UsersRunner : IUsersRunner {
        private readonly INpgsqlConnectionFactory _connectionFactory;
        private readonly IUsersRepository _repository;
        private IModelContext _context;

        public UsersRunner(
            IModelContext context,
            INpgsqlConnectionFactory connectionFactory,
            IUsersRepository repository) {
            _context = context;
            _connectionFactory = connectionFactory;
            _repository = repository;
        }

        public void Start() {
            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                _repository.EnsureTable(connection);
                InsertContextUsers(connection);
                _context = null;
            }
        }

        public void Stop() { }

        public User LogOn(string name, string password) {
            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                var users = _repository.GetByNameAndPassword(connection, name, password);
                User user = users.FirstOrDefault();
                if (user != null && user.Enabled)
                    return user;
                return null;
            }
        }

        public bool ChangePassword(string name, string oldPassword, string newPassword) {
            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                User user = LogOn(name, oldPassword);
                if (user != null && user.CanChange) {
                    bool result = _repository.SetPassword(connection, user.Id, newPassword);
                    return result;
                }

                return false;
            }
        }

        public int? GetPrivilege(Identity identity) {
            if (identity == null)
                return null;

            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                var pnp = _repository.GetPrivilegeAndNameAndPassword(connection, identity.UserId);
                if (pnp == null)
                    return null;

                if (!identity.Equals(new Identity(identity.UserId, pnp.Item2, pnp.Item3)))
                    return null;

                return pnp.Item1;
            }
        }

        public User[] GetUsers(Identity identity, CriteriaType criteriaType, string name, int count) {
            if (!IsUserViewer(identity))
                return new User[0];
            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                return _repository.Get(connection, criteriaType, name, count);
            }
        }

        public bool SetPassword(Identity identity, long id, string password) {
            if (!IsUserEditor(identity))
                return false;
            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                return _repository.SetPassword(connection, id, password);
            }
        }

        public UpdateUserResult UpdateUser(Identity identity, User user) {
            if (!IsUserEditor(identity))
                return UpdateUserResult.Fail;
            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                return _repository.UpdateUser(connection, user);
            }
        }

        public InsertUserResult InsertUser(Identity identity, User user) {
            if (!IsUserEditor(identity))
                return InsertUserResult.Fail;
            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                return _repository.InsertUser(connection, user);
            }
        }

        public User[] GetUsers(Identity identity, long[] ids) {
            if (!IsUserViewer(identity))
                return new User[0];
            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                return _repository.Get(connection, ids);
            }
        }

        private void InsertContextUsers(NpgsqlConnection connection) {
            var users = _context.Get<PHmiModel.Entities.User>().ToList();
            if (!users.Any())
                return;
            int minId = -users.Select(u => u.Id).Max();
            int maxId = -users.Select(u => u.Id).Min();
            var repoUsersIds = _repository.GetIds(connection, minId, maxId);
            foreach (PHmiModel.Entities.User user in repoUsersIds
                .Select(t => users.FirstOrDefault(u => u.Id == -t.Item1 || u.Name == t.Item2))
                .Where(user => user != null))
                users.Remove(user);
            if (!users.Any())
                return;
            var usersToInsert = users.Select(u => new User {
                Id = -u.Id,
                Name = u.Name,
                Description = u.Description,
                Photo = u.Photo,
                Enabled = u.Enabled,
                CanChange = u.CanChange,
                Privilege = u.Privilege
            }).ToArray();
            var usersPasswordsToInsert = users.Select(u => u.Password).ToArray();
            _repository.Insert(connection, usersToInsert, usersPasswordsToInsert);
        }

        private bool IsUserPrivileged(Identity identity, int privilege) {
            var userPrivilege = GetPrivilege(identity);
            if (!userPrivilege.HasValue)
                return false;
            var userAdminPrivelege =
                Int32ToPrivilegedConverter.ConvertBack(privilege.ToString(CultureInfo.InvariantCulture));
            if (!userAdminPrivelege.HasValue)
                return false;
            return (userPrivilege.Value & userAdminPrivelege.Value) != 0;
        }

        private bool IsUserViewer(Identity identity) {
            return IsUserPrivileged(identity, 31);
        }

        private bool IsUserEditor(Identity identity) {
            return IsUserPrivileged(identity, 30);
        }
    }
}