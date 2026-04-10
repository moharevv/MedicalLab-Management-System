using New_AppLabaratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AppLaboratory.Labs
{
    /// <summary>
    /// Логика взаимодействия для Lab.xaml
    /// </summary>
    public partial class Lab : Window
    {
        private DispatcherTimer timer;
        private TimeSpan remainingTime = TimeSpan.FromHours(2.5);
        public Lab(string login, int type)
        {
            InitializeComponent();
            this.login = login;
            this.type = type;
            // Создаем таймер
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Вычитаем одну секунду из оставшегося времени
            remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));

            // Проверяем, если оставшееся время истекло, останавливаем таймер
            if (remainingTime <= TimeSpan.Zero)
            {
                timer.Stop();
                MessageBox.Show("Время истекло!");
            }
            // Обновляем время
            lblTime.Text = remainingTime.ToString(@"hh\:mm\:ss");
        }
        public string login { get; set; }
        public int type { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Name.Text = SQLClass.GetName(login, type);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow back = new MainWindow();
            back.Show();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WelcomeBio biomaterial = new WelcomeBio(login, "", false);    
            biomaterial.Show();
            this.Close();
        }

    }
}
