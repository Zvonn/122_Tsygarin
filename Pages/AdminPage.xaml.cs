using System.Windows;
using System.Windows.Controls;
using _122_Tsygarin.Pages;

namespace _122_Tsygarin.Pages
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Переход на страницу таблицы пользователей
        /// </summary>
        private void BtnTab1_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new UsersTabPage());
        }

        /// <summary>
        /// Переход на страницу таблицы категорий
        /// </summary>
        private void BtnTab2_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CategoryTabPage());
        }

        /// <summary>
        /// Переход на страницу таблицы платежей
        /// </summary>
        private void BtnTab3_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PaymentTabPage());
        }

        /// <summary>
        /// Переход на страницу с диаграммами
        /// </summary>
        private void BtnTab4_Click(object sender, RoutedEventArgs e)
        {
           NavigationService?.Navigate(new DiagrammPage());
        }
    }
}