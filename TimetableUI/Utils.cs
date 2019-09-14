using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TimetableUI
{
    public static class Utils
    {
        public const string FileTimetable = "timetable.json";
        public const string FileSubjects = "subjects.json";

        public static string ConvertToAppDataPath(string filename)
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"BohushTimetable\");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return Path.Combine(folder, filename);
        }

        public static void SaveFile(string filename, object value)
        {
            filename = ConvertToAppDataPath(filename);
            var json = JsonConvert.SerializeObject(value, Formatting.Indented);
            File.WriteAllText(filename, json);
        }

        public static T LoadFile<T>(string filename)
        {
            filename = ConvertToAppDataPath(filename);
            if (!File.Exists(filename))
            {
                return default(T);
            }

            var json = File.ReadAllText(filename);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
