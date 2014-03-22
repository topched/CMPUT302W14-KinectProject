using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LifeCycle
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            FocusManager.SetFocusedElement(grid1, usernameBox);
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO - this should check in the database of users - for now, dummy users.
            if (usernameBox.Text == "patient1" && passwordBox.Text == "password")
            {
                var newWindow = new MainWindow();
                newWindow.Show();
                this.Close();
            }
            else if (usernameBox.Text == "clinician" && passwordBox.Text == "password")
            {
                // open a different window
            }
            else
            {
                loginBlock.Foreground = Brushes.Red;
                loginBlock.Text = "Username and/or password was incorrect.";
            }
        }
    }
}
