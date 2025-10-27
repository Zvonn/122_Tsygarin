using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _122_Tsygarin.Pages
{
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Хеширование пароля
        /// </summary>
        private string GetHash(string password)
        {
            using (var hash = SHA256.Create())
            {
                byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// Проверка корректности пароля
        /// </summary>
        private bool ValidatePassword(string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Минимальная длина
            if (password.Length < 6)
            {
                errorMessage = "Пароль должен содержать минимум 6 символов!";
                return false;
            }

            // Наличие заглавной буквы
            bool hasUpperCase = password.Any(char.IsUpper);
            if (!hasUpperCase)
            {
                errorMessage = "Пароль должен содержать хотя бы одну заглавную букву!";
                return false;
            }

            // Наличие цифры
            bool hasDigit = password.Any(char.IsDigit);
            if (!hasDigit)
            {
                errorMessage = "Пароль должен содержать хотя бы одну цифру!";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Обработка нажатия на кнопку "Сохранить"
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка заполнения всех полей
                if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                    string.IsNullOrEmpty(NewPasswordBox.Password) ||
                    string.IsNullOrEmpty(ConfirmPasswordBox.Password) ||
                    string.IsNullOrEmpty(TbLogin.Text))
                {
                    MessageBox.Show("Все поля обязательны к заполнению!",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка правильности текущего логина и пароля
                string hashedPass = AuthPage.GetHash(CurrentPasswordBox.Password);
                var user = Tsygarin_DB_PaymentEntities.GetContext().User
                    .FirstOrDefault(u => u.Login == TbLogin.Text && u.Password == hashedPass);

                if (user == null)
                {
                    MessageBox.Show("Текущий пароль/Логин неверный!",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка совпадения нового пароля и подтверждения
                if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    MessageBox.Show("Новый пароль и подтверждение не совпадают!",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка корректности нового пароля
                if (!ValidatePassword(NewPasswordBox.Password, out string errorMessage))
                {
                    MessageBox.Show(errorMessage,
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка, что новый пароль отличается от старого
                if (CurrentPasswordBox.Password == NewPasswordBox.Password)
                {
                    MessageBox.Show("Новый пароль должен отличаться от текущего!",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Сохранение нового пароля
                user.Password = AuthPage.GetHash(NewPasswordBox.Password);
                Tsygarin_DB_PaymentEntities.GetContext().SaveChanges();

                MessageBox.Show("Пароль успешно изменен!",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService?.Navigate(new AuthPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработка нажатия на кнопку "Отмена"
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}