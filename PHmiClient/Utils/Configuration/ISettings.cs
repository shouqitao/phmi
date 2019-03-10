using System;

namespace PHmiClient.Utils.Configuration {
    public interface ISettings {
        void Reload();

        void Save();

        string GetString(string key);

        void SetString(string key, string value);

        T Get<T>(string key, Func<byte[], T> convertFunc) where T : new();

        void Set<T>(string key, T value, Func<T, byte[]> convertFunc) where T : new();

        double? GetDouble(string key);

        void SetDouble(string key, double? value);

        bool? GetBoolean(string key);

        void SetBoolean(string key, bool? value);

        int? GetInt32(string key);

        void SetInt32(string key, int? value);

        long? GetInt64(string key);

        void SetInt64(string key, long? value);
    }
}