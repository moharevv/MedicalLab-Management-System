using AppLaboratory.Labs.Laborant2;
using New_AppLabaratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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

namespace AppLaboratory
{
    /// <summary>
    /// Логика взаимодействия для Lab_res.xaml
    /// </summary>
    public partial class Lab_res : Window
    {
        // static System.Windows.Timer myTimer = new System.Windows.Timer();
        private DispatcherTimer timer;
        private TimeSpan remainingTime = TimeSpan.FromHours(2.5);

        public string login { get; set; }
        public int type { get; set; }
        public Lab_res(string login, int type)
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Name.Text = SQLClass.GetName(login,type);
            
            
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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow back = new MainWindow();
            back.Show();
            this.Close();
        }

        private void WorkAnalyser_Click(object sender, RoutedEventArgs e)
        {
            AnalyserWork analyserWindow = new AnalyserWork();
            analyserWindow.Show();
            Close();
        }
    }
}
