using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _122_Tsygarin.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;
        private User currentUser;

        public AuthPage()
        {
            InitializeComponent();
        }

        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLoginHitn.Visibility = TextBoxLogin.Text.Length > 0
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void lblLoginHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBoxLogin.Focus();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassHitn.Visibility = PasswordBox.Password.Length > 0
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void lblPassHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Focus();
        }

        public static string GetHash(String password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password))
                    .Select(x => x.ToString("X2")));
            }
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage());
        }

        public void CaptchaSwitch()
        {
            if (captcha.Visibility == Visibility.Visible)
            {
                // Скрыть капчу, показать поля входа
                TextBoxLogin.Clear();
                PasswordBox.Clear();

                captcha.Visibility = Visibility.Hidden;
                captchaInput.Visibility = Visibility.Hidden;
                captchaInput.Clear();
                labelCaptcha.Visibility = Visibility.Hidden;
                submitCaptcha.Visibility = Visibility.Hidden;

                labelLogin.Visibility = Visibility.Visible;
                labelPass.Visibility = Visibility.Visible;
                TextBoxLogin.Visibility = Visibility.Visible;
                lblLoginHitn.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Visible;
                lblPassHitn.Visibility = Visibility.Visible;

                ButtonChangePassword.Visibility = Visibility.Visible;
                ButtonEnter.Visibility = Visibility.Visible;
                ButtonReg.Visibility = Visibility.Visible;
            }
            else
            {
                // Показать капчу, скрыть поля входа
                captcha.Visibility = Visibility.Visible;
                captchaInput.Visibility = Visibility.Visible;
                labelCaptcha.Visibility = Visibility.Visible;
                submitCaptcha.Visibility = Visibility.Visible;

                labelLogin.Visibility = Visibility.Hidden;
                labelPass.Visibility = Visibility.Hidden;
                TextBoxLogin.Visibility = Visibility.Hidden;
                lblLoginHitn.Visibility = Visibility.Hidden;
                PasswordBox.Visibility = Visibility.Hidden;
                lblPassHitn.Visibility = Visibility.Hidden;

                ButtonChangePassword.Visibility = Visibility.Hidden;
                ButtonEnter.Visibility = Visibility.Hidden;
                ButtonReg.Visibility = Visibility.Hidden;
            }
        }

        public void CaptchaChange()
        {
            string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                 "abcdefghijklmnopqrstuvwxyz" +
                                 "0123456789";

            StringBuilder pwd = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < 6; i++)
            {
                pwd.Append(allowedChars[random.Next(allowedChars.Length)]);
            }

            captcha.Text = pwd.ToString();
            captchaInput.Clear();
        }

        private void submitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (captchaInput.Text != captcha.Text)
            {
                MessageBox.Show("Неверно введена капча", "Ошибка");
                CaptchaChange();
            }
            else
            {
                MessageBox.Show("Капча введена успешно, можете продолжить авторизацию", "Успех");
                CaptchaSwitch();
                failedAttempts = 0;
            }
        }

        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxLogin.Text) ||
                string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            string hashedPassword = GetHash(PasswordBox.Password);

            using (var db = new Tsygarin_DB_PaymentEntities())
            {
                var user = db.User
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Login == TextBoxLogin.Text &&
                                        u.Password == hashedPassword);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден!");
                    failedAttempts++;

                    if (failedAttempts >= 3)
                    {
                        if (captcha.Visibility != Visibility.Visible)
                        {
                            CaptchaSwitch();
                            CaptchaChange();
                        }
                    }
                    return;
                }

                MessageBox.Show("Пользователь успешно найден!");

                switch (user.Role)
                {
                    case "User":
                        NavigationService?.Navigate(new Pages.UserPage());
                        break;
                    case "Admin":
                        NavigationService?.Navigate(new Pages.AdminPage());
                        break;
                    default:
                        MessageBox.Show("Неизвестная роль пользователя");
                        break;
                }
            }
        }
    }
}