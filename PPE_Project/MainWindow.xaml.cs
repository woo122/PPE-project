using System;
using System.Windows;

namespace PPE_Project
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_Main_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Pages/MainPage.xaml", UriKind.Relative));
        }

        private void btn_Record_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Pages/RecordPage.xaml", UriKind.Relative));
        }

        private void btn_Employee_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Pages/EmployeePage.xaml", UriKind.Relative));
        }
    }
}