using _122_Tsygarin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _122_Tsygarin.Pages
{
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                var currentUsers = Tsygarin_DB_PaymentEntities.GetContext().User.ToList();
                ListUser.ItemsSource = currentUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработчик изменения текста в поле поиска по ФИО
        /// </summary>
        private void fioFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUsers();
        }

        /// <summary>
        /// Обработчик изменения сортировки
        /// </summary>
        private void sortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUsers();
        }

        /// <summary>
        /// Обработчик установки фильтра "Только администраторы"
        /// </summary>
        private void onlyAdminCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        /// <summary>
        /// Обработчик снятия фильтра "Только администраторы"
        /// </summary>
        private void onlyAdminCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        /// <summary>
        /// Очистка всех фильтров
        /// </summary>
        private void clearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            fioFilterTextBox.Text = "";
            sortComboBox.SelectedIndex = 0;
            onlyAdminCheckBox.IsChecked = false;
        }

        /// <summary>
        /// Обновление списка пользователей с применением фильтров
        /// </summary>
        private void UpdateUsers()
        {
            try
            {
                List<User> currentUsers = Tsygarin_DB_PaymentEntities.GetContext().User.ToList();

                // Фильтрация по ФИО
                if (!string.IsNullOrWhiteSpace(fioFilterTextBox.Text))
                {
                    currentUsers = currentUsers
                        .Where(x => x.FIO.ToLower().Contains(fioFilterTextBox.Text.ToLower()))
                        .ToList();
                }

                // Фильтрация по роли (только администраторы)
                if (onlyAdminCheckBox.IsChecked.Value)
                {
                    currentUsers = currentUsers
                        .Where(x => x.Role == "Admin")
                        .ToList();
                }

                // Сортировка по ФИО (возрастание/убывание)
                if (sortComboBox.SelectedIndex == 0)
                {
                    ListUser.ItemsSource = currentUsers.OrderBy(x => x.FIO).ToList();
                }
                else
                {
                    ListUser.ItemsSource = currentUsers.OrderByDescending(x => x.FIO).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении фильтров: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}