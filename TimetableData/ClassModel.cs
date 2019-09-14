using System;
using Newtonsoft.Json;

namespace TimetableData
{
    public class ClassModel : IComparable<ClassModel>
    {
        #region Day and Time configuration
        public static TimeSpan SegmentDuration = TimeSpan.FromMinutes(45);
        public static TimeSpan[] TimeSegments = {
            new TimeSpan(7, 15, 0),
            new TimeSpan(8, 0, 0),
            new TimeSpan(9, 0, 0),
            new TimeSpan(9, 45, 0),
            new TimeSpan(10, 45, 0),
            new TimeSpan(11, 30, 00),
            new TimeSpan(12, 30, 0),
            new TimeSpan(13, 15, 0),
            new TimeSpan(14, 15, 0),
            new TimeSpan(15, 0, 0),
            new TimeSpan(16, 0, 0),
            new TimeSpan(16, 45, 0),
            new TimeSpan(17, 45, 0),
            new TimeSpan(18, 30, 0)
        };
        public static DayOfWeek[] Days =
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday
        };
    #endregion

        public string SubjectName { get; set; }
        public string TeacherName { get; set; }
        public ClassType ClassType { get; set; }

        public int DayIndex { get; set; }
        public int TimeSegmentStart { get; set; }
        public int TimeSegmentCount { get; set; }

        [JsonIgnore]
        public DayOfWeek DayOfWeek => Days[DayIndex];
        [JsonIgnore]
        public TimeSpan TimeStart => TimeSegments[TimeSegmentStart];
        [JsonIgnore]
        public TimeSpan TimeEnd => TimeSegments[TimeSegmentStart + TimeSegmentCount - 1] + SegmentDuration;

        public bool CollidesWith(ClassModel other)
        {
            if (DayIndex != other.DayIndex)
                return false;

            // Stolen from http://www.webdevbros.net/2007/08/24/timerange-object-for-c/ :)
            return (other.TimeStart < TimeStart && other.TimeEnd > TimeEnd) ||
                   (other.TimeStart < TimeStart && other.TimeEnd > TimeStart) ||
                   (other.TimeEnd > TimeEnd && other.TimeStart < TimeEnd) ||
                   (other.TimeStart >= TimeStart && other.TimeEnd <= TimeEnd);
        }

        public override int GetHashCode()
        {
            // https://www.loganfranken.com/blog/692/overriding-equals-in-c-part-2/
            unchecked
            {
                const int hashingBase = (int)2166136261;
                const int hashingMultiplier = 16777619;

                int hash = hashingBase;
                hash = (hash * hashingMultiplier) ^
                       (!ReferenceEquals(null, SubjectName) ? SubjectName.GetHashCode() : 0);
                hash = (hash * hashingMultiplier) ^
                       (!ReferenceEquals(null, TeacherName) ? TeacherName.GetHashCode() : 0);
                hash = (hash * hashingMultiplier) ^
                       (!ReferenceEquals(null, ClassType) ? ClassType.GetHashCode() : 0);
                hash = (hash * hashingMultiplier) ^
                       (!ReferenceEquals(null, DayOfWeek) ? DayOfWeek.GetHashCode() : 0);
                hash = (hash * hashingMultiplier) ^
                       (!ReferenceEquals(null, TimeSegmentStart) ? TimeSegmentStart.GetHashCode() : 0);
                hash = (hash * hashingMultiplier) ^
                       (!ReferenceEquals(null, TimeSegmentCount) ? TimeSegmentCount.GetHashCode() : 0);

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (!(obj is ClassModel other))
                return false;

            return SubjectName == other.SubjectName && TeacherName == other.TeacherName &&
                   ClassType == other.ClassType && DayOfWeek == other.DayOfWeek &&
                   TimeSegmentStart == other.TimeSegmentStart && TimeSegmentCount == other.TimeSegmentCount;
        }

        public static bool operator ==(ClassModel modelA, ClassModel modelB)
        {
            if (modelA is null && modelB is null)
                return true;

            if (modelA is null)
                return false;
            if (modelB is null)
                return false;

            return modelA.Equals(modelB);
        }

        public static bool operator !=(ClassModel modelA, ClassModel modelB)
        {
            return !(modelA == modelB);
        }

        public int CompareTo(ClassModel other)
        {
            if (SubjectName != other.SubjectName)
                return string.Compare(SubjectName, other.SubjectName, StringComparison.OrdinalIgnoreCase);

            if (DayOfWeek < other.DayOfWeek)
                return -1;
            if (DayOfWeek > other.DayOfWeek)
                return 1;

            return TimeStart.CompareTo(other.TimeStart);
        }

        public override string ToString()
        {
            return $"{SubjectName} - {DayOfWeek.ToString().Substring(0, 3)} {TimeStart:hh\\:mm}";
        }
    }
}
