using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace TimetableData
{
    public class TimetableModel
    {
        public string Name { get; private set; }

        [JsonProperty("Classes")]
        private List<ClassModel> _classes = new List<ClassModel>();

        [JsonIgnore]
        public int ClassCount => _classes.Count;

        public TimetableModel(string name)
        {
            Name = name;
        }

        public void Rename(string name)
        {
            Name = name;
            foreach (var classModel in _classes)
            {
                classModel.SubjectName = name;
            }
        }

        public IList<ClassModel> GetDay(DayOfWeek day) => _classes.Where(c => c.DayOfWeek == day).ToList();

        public bool AddClass(ClassModel item)
        {
            foreach (var classModel in GetDay(item.DayOfWeek))
            {
                if (item.CollidesWith(classModel))
                    return false;
            }

            _classes.Add(item);
            return true;
        }

        public bool RemoveClass(ClassModel item) => _classes.Remove(item);
        public void ClearClasses() => _classes.Clear();

        public int FixCollisions()
        {
            int counter = 0;
            var classes = _classes;
            _classes = new List<ClassModel>();

            foreach (var classModel in classes)
            {
                if (!AddClass(classModel))
                    counter++;
            }

            return counter;
        }

        public IReadOnlyCollection<ClassModel> GetClasses()
        {
            return _classes.AsReadOnly();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
