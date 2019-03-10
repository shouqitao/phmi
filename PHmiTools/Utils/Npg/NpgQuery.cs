using System.Data;
using Npgsql;

namespace PHmiTools.Utils.Npg {
    public class NpgQuery {
        private readonly NpgsqlParameter[] _parameters;

        public NpgQuery(string text, params NpgsqlParameter[] parameters) {
            Text = text;
            _parameters = parameters ?? new NpgsqlParameter[0];
        }

        public string Text { get; }

        public NpgsqlParameter[] Parameters {
            get {
                if (_parameters != null)
                    foreach (NpgsqlParameter p in _parameters) {
                        var strValue = p.Value as string;
                        if (strValue != null && string.IsNullOrEmpty(strValue)) {
                            p.Value = null;
                            p.DbType = DbType.String;
                        }
                    }

                return _parameters;
            }
        }
    }
}