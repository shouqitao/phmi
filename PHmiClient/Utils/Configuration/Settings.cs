using System;

namespace PHmiClient.Utils.Configuration {
    public class Settings : ISettings {
        private readonly IStringKeeper _stringKeeper;

        public Settings(IStringKeeper stringKeeper) {
            _stringKeeper = stringKeeper;
        }

        public Settings(string settingsPrefix) {
            _stringKeeper = new StringKeeper(settingsPrefix);
        }

        public void Reload() {
            _stringKeeper.Reload();
        }

        public void Save() {
            _stringKeeper.Save();
        }

        public string GetString(string key) {
            return _stringKeeper.Get(key);
        }

        public void SetString(string key, string value) {
            _stringKeeper.Set(key, value);
        }

        public void Set<T>(string key, T value, Func<T, byte[]> convertFunc) where T : new() {
            var bytes = convertFunc.Invoke(value);
            string str = ByteConverter.BytesToString(bytes);
            SetString(key, str);
        }

        public T Get<T>(string key, Func<byte[], T> convertFunc) where T : new() {
            string str = GetString(key);
            if (str == null) return default(T);
            byte[] bytes;
            try {
                bytes = ByteConverter.StringToBytes(str);
            } catch (FormatException) {
                return default(T);
            }

            try {
                return convertFunc.Invoke(bytes);
            } catch (Exception) {
                return default(T);
            }
        }

        public void SetDouble(string key, double? value) {
            Set(key, value,
                val => val.HasValue ? BitConverter.GetBytes(val.Value) : null);
        }

        public double? GetDouble(string key) {
            return Get<double?>(key, bytes => BitConverter.ToDouble(bytes, 0));
        }

        public void SetBoolean(string key, bool? value) {
            Set(key, value,
                boolean => boolean.HasValue ? BitConverter.GetBytes(boolean.Value) : null);
        }

        public bool? GetBoolean(string key) {
            return Get<bool?>(key, bytes => BitConverter.ToBoolean(bytes, 0));
        }

        public void SetInt32(string key, int? value) {
            Set(key, value,
                i => i.HasValue ? BitConverter.GetBytes(i.Value) : null);
        }

        public int? GetInt32(string key) {
            return Get<int?>(key, bytes => BitConverter.ToInt32(bytes, 0));
        }

        public void SetInt64(string key, long? value) {
            Set(key, value,
                i => i.HasValue ? BitConverter.GetBytes(i.Value) : null);
        }

        public long? GetInt64(string key) {
            return Get<long?>(key, bytes => BitConverter.ToInt64(bytes, 0));
        }
    }
}