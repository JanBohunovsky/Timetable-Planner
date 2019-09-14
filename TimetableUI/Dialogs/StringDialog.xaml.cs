using System.Windows;

namespace TimetableUI.Dialogs
{
    /// <summary>
    /// Interaction logic for StringDialog.xaml
    /// </summary>
    public partial class StringDialog : Window
    {
        public string Value => TxtInput.Text;

        public StringDialog(string title, string message, string defaultValue = null)
        {
            InitializeComponent();
            
            Title = title;
            if (!string.IsNullOrEmpty(message))
                LblMessage.Text = message;

            if (!string.IsNullOrEmpty(defaultValue))
            {
                TxtInput.Text = defaultValue;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtInput.Focus();
            TxtInput.SelectAll();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtInput.Text))
            {
                MessageBox.Show("Value cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
        }
    }
}
