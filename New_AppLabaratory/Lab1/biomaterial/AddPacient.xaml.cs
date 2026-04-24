using New_AppLabaratory;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;

namespace AppLaboratory.Labs.Laborant.Biomaterial
{
    public partial class AddPacient : Window
    {
        private static readonly Regex PhoneRegex = new Regex(@"^\+?\d{11,15}$");
        private static readonly Regex PassportRegex = new Regex(@"^\d{4}\s?\d{6}$");

        public AddPacient()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SQLClass.OpenConnection();
            try
            {
                this.TypePolis.ItemsSource = SQLClass.Select("SELECT Тип_страхового_полиса FROM Тип_страхового_полиса", SQLClass.str);
                this.Company.ItemsSource = SQLClass.Select("SELECT Название FROM Страховые_компании", SQLClass.str);
            }
            finally
            {
                SQLClass.CloseConnection();
            }
        }

        private void AddBD_Click(object sender, RoutedEventArgs e)
        {
            string fio = this.FIO.Text.Trim();
            string birthDateText = this.WasBorn.Text.Trim();
            string passport = this.Pass.Text.Trim();
            string phone = this.Phone.Text.Trim();
            string email = this.Email.Text.Trim();
            string polis = this.NumPolis.Text.Trim();

            if (fio.Length < 3)
            {
                MessageBox.Show("Введите полное ФИО.");
                return;
            }

            DateTime birthDate;
            if (!DateTime.TryParseExact(birthDateText, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthDate))
            {
                MessageBox.Show("Введите дату рождения в формате дд.мм.гггг.");
                return;
            }

            if (!PassportRegex.IsMatch(passport))
            {
                MessageBox.Show("Паспорт должен содержать 10 цифр.");
                return;
            }

            if (!PhoneRegex.IsMatch(phone))
            {
                MessageBox.Show("Телефон должен содержать от 11 до 15 цифр.");
                return;
            }

            if (!email.Contains("@") || email.Length < 6)
            {
                MessageBox.Show("Почта введена некорректно.");
                return;
            }

            if (polis.Length != 16)
            {
                MessageBox.Show("Полис должен содержать 16 символов.");
                return;
            }

            if (this.TypePolis.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип полиса.");
                return;
            }

            if (this.Company.SelectedItem == null)
            {
                MessageBox.Show("Выберите страховую компанию.");
                return;
            }

            int idTypePolis = SQLClass.GetId(
                "SELECT id FROM Тип_страхового_полиса WHERE Тип_страхового_полиса = @Value",
                this.TypePolis.SelectedItem.ToString());
            int idCompany = SQLClass.GetId(
                "SELECT id FROM Страховые_компании WHERE Название = @Value",
                this.Company.SelectedItem.ToString());

            SQLClass.WritePacientBD(fio, birthDateText, passport, phone, email, polis, idTypePolis, idCompany);
            MessageBox.Show("Пациент добавлен.");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WelcomeBio welcome = new WelcomeBio("", "", false);
            welcome.Show();
            Close();
        }
    }
}
