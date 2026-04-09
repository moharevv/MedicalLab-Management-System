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

namespace New_AppLabaratory.Buxg
{
    /// <summary>
    /// Логика взаимодействия для Buxgalter.xaml
    /// </summary>
    public partial class Buxgalter : Window
    {
        public string Login { get; }
        public int type { get; set; }
        public Buxgalter(string Login, int type)
        {
            InitializeComponent();
            this.Login = Login;
            this.type = type;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Name.Content = SQLClass.GetName(Login, type);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow back = new MainWindow();
            back.Show();
            this.Close();
        }
    }
}
