using New_AppLabaratory;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace AppLaboratory.Labs.Laborant.Biomaterial
{
    /// <summary>
    /// Логика взаимодействия для AddPacient.xaml
    /// </summary>
    public partial class AddPacient : Window
    {
        public AddPacient()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SQLClass.OpenConnection();
            TypePolis.ItemsSource = SQLClass.Select("select Тип_страхового_полиса from Тип_страхового_полиса", SQLClass.str);
            Company.ItemsSource = SQLClass.Select("select Название from Страховые_компании", SQLClass.str);
            SQLClass.CloseConnection();
        }

        private void AddBD_Click(object sender, RoutedEventArgs e)
        {
            if (FIO.Text.Length > 2)
            {
                if (WasBorn.Text.Length >= 10)
                {
                    if (Pass.Text.Length >= 11)
                    {
                        if (Phone.Text.Length >= 11)
                        {
                            if (Email.Text.Length > 5)
                            {
                                if (NumPolis.Text.Length == 16)
                                {
                                    if (TypePolis.SelectedItem != null)
                                    {
                                        if (Company.SelectedItem != null)
                                        {
                                            DateTime dateTime;
                                            if (DateTime.TryParseExact(WasBorn.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                                            {
                                                int idTypePolis = SQLClass.GetId("select id from Тип_страхового_полиса where Тип_страхового_полиса = @Value", TypePolis.SelectedItem.ToString());
                                                int idCompany = SQLClass.GetId("SELECT id from Страховые_компании where Название = @Value", Company.SelectedItem.ToString());
                                                SQLClass.WritePacientBD(FIO.Text, WasBorn.Text, Pass.Text, Phone.Text, Email.Text, NumPolis.Text, idTypePolis, idCompany);
                                                MessageBox.Show("Вы добавили пациента!");
                                            }
                                            else
                                            {
                                                MessageBox.Show("Пожвлуйста, введите дату в формате: dd:MM:yy");
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("Выберите страховую компанию!");
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Выберите тип полиса!");
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Полис введен некорректно!");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Почта введена некорректно!");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Номер телефона введен некорректно!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Паспорт введен некорректно!");
                    }
                }
                else
                {
                    MessageBox.Show("Дата рождения введена некорректно!");
                }    
            }
            else
            {
                MessageBox.Show("Введите Фио!");
            }    
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WelcomeBio welcome = new WelcomeBio("", "", false);
            welcome.Show();
            this.Close();
        }
    }
}
