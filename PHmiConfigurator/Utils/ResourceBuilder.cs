using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;

namespace PHmiConfigurator.Utils {
    public class ResourceBuilder : IResourceBuilder {
        private readonly IDictionary<string, object> _resourceEntries = new Dictionary<string, object>();
        private readonly IResourceWriter _resourceWriter;

        public ResourceBuilder(IResourceWriter resourceWriter) {
            _resourceWriter = resourceWriter;
        }

        public string Add(string resource) {
            if (resource == null)
                resource = string.Empty;
            string initialkey = GetKey(resource);
            var index = 1;
            string key = initialkey;
            while (true) {
                object value;
                if (_resourceEntries.TryGetValue(key, out value)) {
                    if (value as string == resource)
                        return key;
                    key = initialkey + index;
                    index++;
                } else {
                    _resourceEntries.Add(key, resource);
                    return key;
                }
            }
        }

        public void Build() {
            foreach (var i in _resourceEntries) _resourceWriter.AddResource(i.Key, i.Value);
            _resourceWriter.Generate();
            _resourceWriter.Close();
        }

        public void Dispose() {
            _resourceWriter.Dispose();
        }

        private static string GetKey(string resource) {
            if (resource == string.Empty)
                return "empty";
            var key = new StringBuilder(resource.Length);
            foreach (char c in resource) key.Append(char.IsLetterOrDigit(c) ? c : '_');
            string result = key.ToString().ToLower();
            return result.Any() && char.IsDigit(result.First()) ? "n" + result : result;
        }
    }
}