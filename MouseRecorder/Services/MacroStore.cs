using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using MouseRecorder.Models;

namespace MouseRecorder.Services
{
    public class MacroStore
    {
        private readonly string _storeDir;
        private readonly string _filePath;

        public MacroStore()
        {
            _storeDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MouseRecorder");
            Directory.CreateDirectory(_storeDir);
            _filePath = Path.Combine(_storeDir, "macros.json");
        }

        public List<Macro> Load()
        {
            if (!File.Exists(_filePath))
                return new List<Macro>();

            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonConvert.DeserializeObject<List<Macro>>(json) ?? new List<Macro>();
            }
            catch
            {
                return new List<Macro>();
            }
        }

        public void Save(IEnumerable<Macro> macros)
        {
            var json = JsonConvert.SerializeObject(macros, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        public void ExportToFile(IEnumerable<Macro> macros, string path)
        {
            var json = JsonConvert.SerializeObject(macros, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public List<Macro> ImportFromFile(string path)
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<List<Macro>>(json) ?? new List<Macro>();
        }
    }
}
