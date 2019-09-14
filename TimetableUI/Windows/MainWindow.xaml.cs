using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimetableData;
using TimetableUI.Controls;
using TimetableUI.Dialogs;

namespace TimetableUI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainTimetable.Model = Utils.LoadFile<TimetableModel>(Utils.FileTimetable) ?? new TimetableModel("");
            InitTimetables(false);
        }

        public new void Show()
        {
            base.Show();
            ValidateTimetables();
        }

        /// <summary>
        /// Updates <see cref="MainTimetable"/> and creates <see cref="TimetableControl"/> for each subject (subjects are loaded from file).
        /// </summary>
        private void InitTimetables(bool validate = true, bool showValidationErrors = true)
        {
            MainTimetable.Update();

            // Create TimetableControl for each subject
            var subjects = Utils.LoadFile<List<TimetableModel>>(Utils.FileSubjects) ?? new List<TimetableModel>();

            subjects.Sort((t1, t2) => string.Compare(t1.Name, t2.Name, StringComparison.Ordinal));

            var controls = subjects.Select(_ => new TimetableControl()).ToList();
            foreach (var (model, control) in subjects.Zip(controls, (model, control) => (model, control)))
            {
                control.SubjectNameVisibility = Visibility.Collapsed;
                control.SelectionMode = SelectionMode.PerType;
                control.Model = model;
                control.SelectionChanged += SubjectTimetable_SelectionChanged;
            }

            ItemsSubjects.ItemsSource = controls;

            if (!validate)
                return;

            ValidateTimetables(showValidationErrors);
        }

        /// <summary>
        /// Fixes collisions, removes invalid classes from main timetable and checks all selected classes.
        /// </summary>
        /// <returns>false if there were compatibility issues, otherwise true</returns>
        private bool ValidateTimetables(bool showErrors = true)
        {
            bool ok = true;

            // Fix collisions
            ok &= MainTimetable.Model.FixCollisions() <= 0;
            foreach (TimetableControl timetable in ItemsSubjects.Items)
            {
                if (timetable.Model.FixCollisions() > 0)
                {
                    ok = false;
                    timetable.Update();
                }
            }

            // Check selected classes
            var classesToRemove = new List<ClassModel>();
            foreach (var classModel in MainTimetable.Model.GetClasses())
            {
                // Find class' subject's timetable
                var subjectTimetable = ItemsSubjects.Items.Cast<TimetableControl>()
                    .FirstOrDefault(t => t.Model.Name == classModel.SubjectName);

                // Timetable not found -> Invalid class
                if (subjectTimetable == null)
                {
                    ok = false;
                    classesToRemove.Add(classModel);
                    continue;
                }

                // Find and check selected class
                bool found = false;
                foreach (ClassControl classControl in subjectTimetable.ItemsClasses.Items)
                {
                    if (classControl.Model == classModel)
                    {
                        classControl.CheckBox.IsChecked = true;
                        subjectTimetable.SelectedItems.Add(classControl.Model);
                        found = true;
                        break;
                    }
                }

                // Class not found -> Invalid class
                if (!found)
                {
                    ok = false;
                    classesToRemove.Add(classModel);
                }
            }

            // Remove invalid classes
            foreach (var classModel in classesToRemove)
            {
                MainTimetable.Model.RemoveClass(classModel);
            }

            // Update main timetable
            if (ok) return true;

            MainTimetable.Update();
            if (showErrors)
            {
                MessageBox.Show(
                    "An incompatibility has been detected between timetable and subjects.\nSome classes may be missing from the timetable.",
                    "Incompatible Timetables",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            return false;
        }

        #region Timetable Events

        private void SubjectTimetable_SelectionChanged(object sender, ClassSelectionChangedEventArgs e)
        {
            var timetable = (TimetableControl)sender;

            foreach (var classModel in e.Added)
            {
                if (!MainTimetable.Model.AddClass(classModel))
                {
                    timetable.RemoveSelection(classModel);
                    SystemSounds.Asterisk.Play();
                }
            }

            foreach (var classModel in e.Removed)
            {
                MainTimetable.Model.RemoveClass(classModel);
            }

            MainTimetable.Update();
        }

        private void TimetableClear_Click(object sender, RoutedEventArgs e)
        {
            MainTimetable.Model.ClearClasses();
            MainTimetable.Update();
            foreach (TimetableControl timetable in ItemsSubjects.Items)
            {
                timetable.ClearSelection();
            }
        }
        #endregion

        #region App Events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ValidateTimetables(false);
            Utils.SaveFile(Utils.FileTimetable, MainTimetable.Model);
        }
        #endregion

        #region Menu Events
        private void FileImport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "JSON file (*.json)|*.json",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var json = File.ReadAllText(dialog.FileName);
                    var data = JsonConvert.DeserializeObject<JObject>(json);

                    Utils.SaveFile(Utils.FileSubjects, data["subjects"].ToObject<List<TimetableModel>>());
                    MainTimetable.Model = data["timetable"].ToObject<TimetableModel>();

                    InitTimetables();
                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid data format.\nAll timetables will now reset.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    Utils.SaveFile(Utils.FileSubjects, new List<TimetableModel>());
                    MainTimetable.Model = new TimetableModel("");

                    InitTimetables(showValidationErrors: false);
                }
            }
        }

        private void FileExport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON file (*.json)|*.json",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            };
            if (dialog.ShowDialog() == true)
            {
                var data = new JObject();
                data["timetable"] = JToken.FromObject(MainTimetable.Model);
                data["subjects"] =
                    JToken.FromObject(ItemsSubjects.Items.Cast<TimetableControl>().Select(c => c.Model).ToList());

                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(dialog.FileName, json);
            }
        }


        private void MenuEditSubjects_Click(object sender, RoutedEventArgs e)
        {
            var window = new SubjectManagerWindow(this);
            if (window.ShowDialog() == true)
            {
                InitTimetables(showValidationErrors: false);
            }
        }


        private void MenuHelpAbout_Click(object sender, RoutedEventArgs e)
        {
            new AboutDialog().ShowDialog();
        }
        #endregion
    }
}
