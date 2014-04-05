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
            if (usernameBox.Text == "patient1" && passwordBox.Password == "password")
            {
                var newWindow = new MainWindow();
                newWindow.Show();
                this.Close();
            }
            else
            {
                loginBlock.Foreground = Brushes.Red;
                loginBlock.Text = "Username or password incorrect - Please try again!";
                passwordBox.Password = "";
                FocusManager.SetFocusedElement(grid1, passwordBox);
            }
        }

        private void grid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                loginButton_Click(sender, e); //here LoginButton_Click is click eventhandler
            }
        }


    }
}
