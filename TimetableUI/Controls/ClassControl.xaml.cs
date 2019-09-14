using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TimetableData;

namespace TimetableUI.Controls
{
    /// <summary>
    /// Interaction logic for ClassControl.xaml
    /// </summary>
    public partial class ClassControl : UserControl
    {
        [ReadOnly(true)]
        public ClassModel Model { get; }
        
        public bool IsLecture => Model.ClassType == ClassType.Lecture;

        public event EventHandler Click;

        public ClassControl(ClassModel model)
        {
            Model = model;

            InitializeComponent();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, EventArgs.Empty);
        }
    }
}
