using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TimetableData;

namespace TimetableUI.Controls
{
    /// <summary>
    /// Interaction logic for TimetableControl.xaml
    /// </summary>
    public partial class TimetableControl : UserControl
    {
        public static DependencyProperty SubjectNameVisibilityProperty = DependencyProperty.Register("SubjectNameVisibility",
            typeof(Visibility), typeof(TimetableControl), new PropertyMetadata(Visibility.Visible));
        public static DependencyProperty SelectionModeProperty = DependencyProperty.Register("SelectionMode",
            typeof(SelectionMode), typeof(TimetableControl), new PropertyMetadata(SelectionMode.PerType));

        public Visibility SubjectNameVisibility
        {
            get => (Visibility)GetValue(SubjectNameVisibilityProperty);
            set => SetValue(SubjectNameVisibilityProperty, value);
        }

        public SelectionMode SelectionMode
        {
            get => (SelectionMode)GetValue(SelectionModeProperty);
            set => SetValue(SelectionModeProperty, value);
        }
        
        public int ClassCount => Model?.ClassCount ?? 0;

        public List<ClassModel> SelectedItems { get; } = new List<ClassModel>();
        public ClassModel SelectedItem => SelectedItems.FirstOrDefault();

        public event EventHandler<ClassSelectionChangedEventArgs> SelectionChanged;

        private TimetableModel _model;
        public TimetableModel Model
        {
            get => _model;
            set
            {
                _model = value;
                Update();
            } 
        }

        public TimetableControl()
        {
            InitializeComponent();
            
            ItemsDayHeaders.ItemsSource = ClassModel.Days.Select(d => d.ToString());
            ItemsTimeHeaders.ItemsSource =
                ClassModel.TimeSegments.Select(t => $"{t:h\\:mm}\n{(t + ClassModel.SegmentDuration):h\\:mm}");
        }

        /// <summary>
        /// Refreshes the control.
        /// </summary>
        public void Update()
        {
            // Update name
            TxtName.Text = Model?.Name;

            if (Model == null)
            {
                // Remove all classes
                ItemsClasses.ItemsSource = null;
            }
            else
            {
                // Create ClassControl for each class in timetable
                // and subscribe to its click event
                var controls = Model.GetClasses().Select(c => new ClassControl(c)).ToList();
                foreach (var control in controls)
                {
                    control.Click += ClassControl_Click;
                    control.TxtSubject.Visibility = SubjectNameVisibility;
                    control.CheckBox.Visibility = SelectionMode != SelectionMode.None ? Visibility.Visible : Visibility.Hidden;
                }

                // Remove classes from selection and replace items source
                var eventArgs = new ClassSelectionChangedEventArgs();
                eventArgs.Removed.AddRange(SelectedItems);
                SelectedItems.Clear();

                ItemsClasses.ItemsSource = controls;

                SelectionChanged?.Invoke(this, eventArgs);
            }
            // Refresh the control
            ItemsClasses.UpdateLayout();
        }

        public void RemoveSelection(ClassModel classModel)
        {
            if (!SelectedItems.Contains(classModel))
                return;

            var eventArgs = new ClassSelectionChangedEventArgs();
            eventArgs.Removed.Add(classModel);
            SelectedItems.Remove(classModel);

            foreach (ClassControl item in ItemsClasses.Items)
            {
                if (item.Model == classModel)
                {
                    item.CheckBox.IsChecked = false;
                }
            }

            SelectionChanged?.Invoke(this, eventArgs);
        }

        public void ClearSelection()
        {
            var eventArgs = new ClassSelectionChangedEventArgs();
            eventArgs.Removed.AddRange(SelectedItems);
            SelectedItems.Clear();

            foreach (ClassControl item in ItemsClasses.Items)
            {
                item.CheckBox.IsChecked = false;
            }

            SelectionChanged?.Invoke(this, eventArgs);
        }

        private void ClassControl_Click(object sender, EventArgs e)
        {
            // Ignore this event if selection mode is None
            if (!(sender is ClassControl control) || SelectionMode == SelectionMode.None)
                return;
            
            var eventArgs = new ClassSelectionChangedEventArgs();

            // Deselect the class
            if (control.CheckBox.IsChecked == false)
            {
                eventArgs.Removed.Add(control.Model);
                SelectedItems.Remove(control.Model);

                SelectionChanged?.Invoke(this, eventArgs);
                return;
            }

            // If SelectionMode is Single, then we want to select only one class
            //   so we need to uncheck every other checkbox.
            // But if SelectionMode is PerType, then we want to select only one class for each type (Exercise and Lecture)
            //   so we need to uncheck every other checkbox with the same class type.

            if (SelectionMode == SelectionMode.Single)
            {
                eventArgs.Removed.AddRange(SelectedItems);
                SelectedItems.Clear();
            }

            foreach (ClassControl other in ItemsClasses.Items)
            {
                if (other == control)
                    continue;

                switch (SelectionMode)
                {
                    case SelectionMode.Single:
                        other.CheckBox.IsChecked = false;
                        break;
                    case SelectionMode.PerType when other.Model.ClassType == control.Model.ClassType:
                        other.CheckBox.IsChecked = false;
                        eventArgs.Removed.Add(other.Model);
                        SelectedItems.Remove(other.Model);
                        break;
                }
            }

            // Select the class and invoke SelectionChanged event
            eventArgs.Added.Add(control.Model);
            SelectedItems.Add(control.Model);

            SelectionChanged?.Invoke(this, eventArgs);
        }
    }

    public enum SelectionMode
    {
        None,
        Single,
        PerType
    }

    /// <summary>
    /// Contains information about which classes were added or removed from selection.
    /// </summary>
    public class ClassSelectionChangedEventArgs : EventArgs
    {
        public List<ClassModel> Added { get; set; } = new List<ClassModel>();
        public List<ClassModel> Removed { get; set; } = new List<ClassModel>();
    }

    #region Binding Value Converters
    public class MultiplyValuesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return 0;

            var result = (int)values[0] * (int)values[1];
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    #endregion
}
