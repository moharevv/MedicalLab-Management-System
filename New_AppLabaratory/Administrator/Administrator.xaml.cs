using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

namespace New_AppLabaratory
{
    /// <summary>
    /// Логика взаимодействия для Administrator.xaml
    /// </summary>
    public partial class Administrator : Window
    {
        public string Login { get; set; }
        public int type { get; set; }
        public Administrator(string Login, int type)
        {
            InitializeComponent();
            this.Login = Login;
            this.type = type;
        }

        private void CheckHistoryEntr_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = SQLClass.ExecuteSql("SELECT UserName, Date, EnterSucces from AuthHistory");
            Listik.ItemsSource = dt.DefaultView;
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

        private void GridViewColumnHeader_Click(object sender, MouseButtonEventArgs e)
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(Listik.ItemsSource);
            if (view != null)
            {
                // 1. Запоминаем текущее направление (если оно было)
                ListSortDirection newDirection = ListSortDirection.Ascending;

                if (view.SortDescriptions.Count > 0 && view.SortDescriptions[0].PropertyName == "Date")
                {
                    // Если уже сортировали по дате — меняем направление на противоположное
                    newDirection = view.SortDescriptions[0].Direction == ListSortDirection.Ascending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }

                // 2. Очищаем старые правила
                view.SortDescriptions.Clear();

                // 3. Добавляем новое правило сортировки
                view.SortDescriptions.Add(new SortDescription("Date", newDirection));

                // 4. Обновляем список на экране
                view.Refresh();
            }
        }
    }
}
