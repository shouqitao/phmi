using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PHmiTools.Utils.Npg {
    public class NpgScriptHelper : INpgScriptHelper {
        public string[] ExtractScriptLines(string script) {
            var uncommentRegex = new Regex(@"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/");
            string uncommented = uncommentRegex.Replace(script, string.Empty);
            var trimmRegex = new Regex(@"[\s]+");
            string trimmed = trimmRegex.Replace(uncommented, " ");
            var splited = trimmed
                .Split(';')
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => s.Trim()).ToArray();
            var result = new List<string>(splited.Length);
            var opened = false;
            string row = string.Empty;
            foreach (string s in splited) {
                row += s + ";";
                int count = s.Count(ch => ch == '\'');
                if (count % 2 != 0)
                    opened = !opened;
                if (!opened) {
                    result.Add(row);
                    row = string.Empty;
                }
            }

            return result.ToArray();
        }

        public string[] ExtractScriptLines(Stream stream) {
            using (stream) {
                var streamReader = new StreamReader(stream, Encoding.UTF8);
                return ExtractScriptLines(streamReader.ReadToEnd());
            }
        }
    }
}