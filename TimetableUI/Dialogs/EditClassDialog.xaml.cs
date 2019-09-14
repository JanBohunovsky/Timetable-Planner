using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TimetableData;

namespace TimetableUI.Dialogs
{
    /// <summary>
    /// Interaction logic for EditClassDialog.xaml
    /// </summary>
    public partial class EditClassDialog : Window
    {
        private TimetableModel _timetable;

        public EditClassDialog(TimetableModel timetable, ClassModel model = null)
        {
            _timetable = timetable;
            InitializeComponent();

            if (!string.IsNullOrEmpty(_timetable.Name))
            {
                TxtSubject.Text = _timetable.Name;
                TxtSubject.IsEnabled = false;
            }

            ComboType.ItemsSource = Enum.GetValues(typeof(ClassType)).Cast<ClassType>();
            ComboType.SelectedIndex = 0;
            
            ComboDay.ItemsSource = ClassModel.Days;
            ComboTimeStart.ItemsSource = ClassModel.TimeSegments;
            UpdateComboDuration();

            if (model != null)
            {
                TxtTeacher.Text = model.TeacherName;
                ComboType.SelectedItem = model.ClassType;
                ComboDay.SelectedIndex = model.DayIndex;
                ComboTimeStart.SelectedIndex = model.TimeSegmentStart;
                ComboDuration.SelectedIndex = model.TimeSegmentCount - 1;
            }

            var firstTextBox = TxtSubject.IsEnabled ? TxtSubject : TxtTeacher;
            firstTextBox.Focus();
            firstTextBox.SelectAll();
        }

        private void UpdateComboDuration()
        {
            // Default list is 1-4 segments, 2nd segment is selected by default (most popular)
            if (ComboTimeStart.SelectedItem == null)
            {
                ComboDuration.IsEnabled = false;
                ComboDuration.ItemsSource = new[] { "1 segment", "2 segments", "3 segments", "4 segments" };
                ComboDuration.SelectedIndex = 1;
                return;
            }
            ComboDuration.IsEnabled = true;

            var index = ComboTimeStart.SelectedIndex;
            if (index < 0)
                index = 0;

            // Generate list with up to 4 segments that show ending time
            var endTimes = ClassModel.TimeSegments.Skip(index).Take(4).Select(t => t + ClassModel.SegmentDuration);
            var items = new List<string>();
            int duration = 1;
            foreach (var time in endTimes)
            {
                items.Add($"{duration} segment{(duration != 1 ? "s" : "")} ({time:h\\:mm})");
                duration++;
            }

            var oldIndex = ComboDuration.SelectedIndex;
            ComboDuration.ItemsSource = items;
            ComboDuration.SelectedIndex = oldIndex;
        }

        private void ComboTimeStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateComboDuration();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Check input
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(TxtTeacher.Text))
                errors.Add("Teacher");
            if (ComboDay.SelectedItem == null)
                errors.Add("Day");
            if (ComboTimeStart.SelectedItem == null)
                errors.Add("Start");
            if (ComboDuration.SelectedItem == null)
                errors.Add("Duration");

            if (errors.Count > 0)
            {
                MessageBox.Show($"Missing required fields:\n{string.Join("\n", errors)}", "Incomplete form", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Build class model and try to add it into timetable
            var classModel = new ClassModel()
            {
                SubjectName = TxtSubject.Text,
                TeacherName = TxtTeacher.Text,
                ClassType = (ClassType)ComboType.SelectedItem,
                DayIndex = ComboDay.SelectedIndex,
                TimeSegmentStart = ComboTimeStart.SelectedIndex,
                TimeSegmentCount = ComboDuration.SelectedIndex + 1
            };

            if (!_timetable.AddClass(classModel))
            {
                MessageBox.Show("Class collides with another class.", "Collision detected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }
    }

    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"/TimetableUI;component/Icons/class-{value.ToString().ToLower()}.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
