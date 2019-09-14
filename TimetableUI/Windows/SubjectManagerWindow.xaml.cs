using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using TimetableData;
using TimetableUI.Controls;
using TimetableUI.Dialogs;

namespace TimetableUI.Windows
{
    /// <summary>
    /// Interaction logic for SubjectManagerWindow.xaml
    /// </summary>
    public partial class SubjectManagerWindow : Window
    {
        private List<TimetableModel> _subjects;

        private bool _changed;

        public SubjectManagerWindow(Window owner = null)
        {
            Owner = owner;
            InitializeComponent();
            // Deep copy, remove all references
            _subjects = Utils.LoadFile<List<TimetableModel>>(Utils.FileSubjects) ?? new List<TimetableModel>();

            LvSubjects.ItemsSource = _subjects;
        }

        private void UpdateTimetable()
        {
            TimetableControl.Update();
            _subjects[LvSubjects.SelectedIndex] = TimetableControl.Model;
            LvSubjects.Items.Refresh();
            _changed = true;
        }

        private void SortSubjectList(bool refresh = true)
        {
            _subjects.Sort((t1, t2) => string.Compare(t1.Name, t2.Name, StringComparison.OrdinalIgnoreCase));
            if (refresh)
                LvSubjects.Items.Refresh();
        }

        #region Dialog Events
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Utils.SaveFile(Utils.FileSubjects, _subjects);
            DialogResult = _changed;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_changed || DialogResult.HasValue) return;

            var result = MessageBox.Show("Do you want to save changes?", "Unsaved changes",
                MessageBoxButton.YesNoCancel, MessageBoxImage.None);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    DialogResult = true;
                    break;
                case MessageBoxResult.No:
                    DialogResult = false;
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
            }
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
                    _subjects.Clear();
                    _subjects.AddRange(JsonConvert.DeserializeObject<List<TimetableModel>>(json));
                    SortSubjectList();
                    _changed = true;
                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid data format.\nAll subjects will now reset.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    _subjects.Clear();
                    LvSubjects.Items.Clear();
                    LvSubjects.Items.Refresh();
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
                var json = JsonConvert.SerializeObject(_subjects, Formatting.Indented);
                File.WriteAllText(dialog.FileName, json);
            }
        }
        #endregion

        #region Subject List Events
        private void LvSubjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool enabled = LvSubjects.SelectedItem != null;
            TimetableControl.IsEnabled = enabled;
            TimetableControl.Model = LvSubjects.SelectedItem as TimetableModel;
            BtnAddClass.IsEnabled = enabled;
            BtnClearClasses.IsEnabled = TimetableControl.ClassCount > 0;
        }

        private void BtnAddSubject_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new StringDialog("New Subject", "Enter name:")
            {
                Icon = (BtnAddSubject.Content as Image)?.Source
            };
            if (dialog.ShowDialog() == true)
            {
                _subjects.Add(new TimetableModel(dialog.Value));
                SortSubjectList();
                _changed = true;
            }
        }

        private void BtnRenameSubject_Click(object sender, RoutedEventArgs e)
        {
            if (LvSubjects.SelectedItem == null) return;

            var dialog = new StringDialog("Rename Subject", "Enter name:", TimetableControl.Model.Name)
            {
                Icon = (BtnRenameSubject.Content as Image)?.Source
            };
            if (dialog.ShowDialog() == true && TimetableControl.Model.Name != dialog.Value)
            {
                TimetableControl.Model.Rename(dialog.Value);
                UpdateTimetable();
                SortSubjectList();
                _changed = true;
            }
        }

        private void BtnRemoveSubject_Click(object sender, RoutedEventArgs e)
        {
            if (LvSubjects.SelectedItem == null) return;

            _changed |= _subjects.Remove(LvSubjects.SelectedItem as TimetableModel);
            LvSubjects.Items.Refresh();
        }

        private void BtnClearSubjects_Click(object sender, RoutedEventArgs e)
        {
            if (LvSubjects.Items.Count <= 0) return;

            _subjects.Clear();
            LvSubjects.Items.Refresh();
            _changed = true;
        }
        #endregion

        #region Timetable Events
        private void TimetableControl_SelectionChanged(object sender, ClassSelectionChangedEventArgs e)
        {
            bool selected = TimetableControl.SelectedItems.Count > 0;

            BtnEditClass.IsEnabled = selected;
            BtnRemoveClass.IsEnabled = selected;
        }

        private void BtnAddClass_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EditClassDialog(TimetableControl.Model);
            if (dialog.ShowDialog() == true)
            {
                UpdateTimetable();
            }
        }

        private void BtnEditClass_Click(object sender, RoutedEventArgs e)
        {
            if (TimetableControl.SelectedItem == null)
                return;

            var classModel = TimetableControl.SelectedItem;

            TimetableControl.Model.RemoveClass(classModel);
            var dialog = new EditClassDialog(TimetableControl.Model, classModel);
            if (dialog.ShowDialog() == true)
            {
                UpdateTimetable();
            }
        }

        private void BtnRemoveClass_Click(object sender, RoutedEventArgs e)
        {
            if (TimetableControl.SelectedItem == null)
                return;
            
            TimetableControl.Model.RemoveClass(TimetableControl.SelectedItem);
            UpdateTimetable();

            BtnClearClasses.IsEnabled = TimetableControl.ClassCount > 0;
        }

        private void BtnClearClasses_Click(object sender, RoutedEventArgs e)
        {
            if (TimetableControl.ClassCount <= 0)
                return;
            
            TimetableControl.Model.ClearClasses();
            UpdateTimetable();

            BtnClearClasses.IsEnabled = TimetableControl.ClassCount > 0;
        }
        #endregion
    }
}
