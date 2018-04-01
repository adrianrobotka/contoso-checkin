using System.Windows;
using System.Reflection;
using System.Windows.Input;

namespace ContosoCheckIn.Windows
{
    /// <summary>
    /// Interaction logic for MenuWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (emailInput.Text.Length < 3)
                {
                    FeedbackMessage.Text = "Email field is short.";
                    return;
                }

                if (passwordInput.Password.Length < 3)
                {
                    FeedbackMessage.Text = "Password field is short.";
                    return;
                }

                if (gateInput.Text.Length < 3)
                {
                    FeedbackMessage.Text = "Gate's name field is short.";
                    return;
                }

                LoginResult login = await ApiClient.LoginAsync(emailInput.Text, passwordInput.Password, gateInput.Text);

                if (!login.isValid)
                {
                    FeedbackMessage.Text = "Cannot login. Bad credantials.";
                    return;
                }

                // valid login

                ApiClient.GateName = gateInput.Text;


                MenuWindow menu = new MenuWindow();
                menu.Show();
                Hide();
            }
            catch
            {
                FeedbackMessage.Text = "Server error";
            }

        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Handled == false && e.Key == Key.Enter)
            {
                LoginButton_Click(null, null);
            }
        }
    }
}