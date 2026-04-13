using AppLaboratory.Labs.Laborant.Biomaterial;
using Aspose.BarCode.Generation;
using Aspose.Pdf;
using Aspose.Pdf.Annotations;
using Aspose.Pdf.Facades;
using Aspose.Pdf.Operators;
using BarcodeLib;
using New_AppLabaratory;
using New_AppLabaratory.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
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
using System.Xaml;
using System.Xml.Linq;
using Path = System.IO.Path;

namespace AppLaboratory.Labs
{

    /// <summary>
    /// Логика взаимодействия для Biomaterial.xaml
    /// </summary>
    public partial class WelcomeBio : Window
    {
        // ВАЖНО: Используй ObservableCollection, чтобы UI обновлялся автоматически
        public ObservableCollection<Service> SelectedServices { get; set; } = new ObservableCollection<Service>();
        private List<Service> AllServices { get; set; } = new List<Service>(); // Загрузи сюда всё из БД при старте окна

        private bool _isCodeConfirmed = false;
        private System.Windows.Threading.DispatcherTimer _timer;

        public string ShtrikhCode { get; set; }
        private bool CodeFromCamera { get; set; }
        public string Login { get; set; }

        private static readonly Random _rnd = new Random();
        public WelcomeBio(string Login, string ShtrikhCode, bool CodeFromCamera)
        {
            InitializeComponent();
            this.Login = Login;
            this.ShtrikhCode = ShtrikhCode;
            this.CodeFromCamera = CodeFromCamera;
            SelectedChipsControl.ItemsSource = SelectedServices;


        }
        private void Window_Loaded(object sender, RoutedEventArgs e) 
        {
            SqlConnection sqlConnection = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;" +
                "AttachDbFilename=|DataDirectory|\\Database\\BASE_LABORATORY.mdf;" +
                "Integrated Security=True");
            try
            {
                if (sqlConnection.State != System.Data.ConnectionState.Open)
                    sqlConnection.Open();

                AllServices = SQLClass.GetServicesList(sqlConnection);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки услуг: " + ex.Message);
            }
        }

        //Событие для того, чтобы проверка поля КодПробирки происходила после загрузки Окна, а не во время 
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (CodeFromCamera == true)
            {
                CodeProbirki.Text = ShtrikhCode;
            }
            else
            {
                // Генерация кода
                string randomPart = _rnd.Next(100000, 999999).ToString("D6");
                int nextOrderId = SQLClass.GetLastOrderId() + 1;
                CodeProbirki.Text = nextOrderId.ToString("D3") + DateTime.Now.ToString("ddMMyy") + randomPart;

                _isCodeConfirmed = false;
                CodeProbirki.IsReadOnly = false;

                // сообщаем пользователю что он должен подтвердить сгенерированый куар нажав ентер,
                //либо сканировать сканером
                ShowNotify("Был сгенерирован код пробирки, сохраните его, нажав ENTER, либо воспользуйтесь сканером");

                SQLClass.OpenConnection();
                FIOCombo.ItemsSource = SQLClass.Select("select ФИО from Данные_пациентов", SQLClass.str);
                SQLClass.CloseConnection();
            }
            
        }
        private void GoScanner_Click(object sender, RoutedEventArgs e)
        {
            ScannerBarcode scanner = new ScannerBarcode();
            scanner.Show();
            this.Close();
            CodeProbirki.Text = ShtrikhCode;
        }
        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            if (GoScanner.IsEnabled == true)
            {
                MessageBox.Show("Создайте или отсканируйте штрих-код!");
                return; 
            }

            if (CodeProbirki.Text.Length != 15)
            {
                MessageBox.Show("Код должен содержать 15 цифр!");
                return;
            }

            if (string.IsNullOrWhiteSpace(BioName.Text))
            {
                MessageBox.Show("Введите биоматериал!");
                return;
            }

            if (FIOCombo.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите пациента!");
                return;
            }

            if (SelectedServices.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одну услугу для заказа!");
                return;
            }

            try
            {
                string pacient = FIOCombo.SelectedItem.ToString();
                string codeProbirki = CodeProbirki.Text;
                string nameBio = BioName.Text;

                int idPacient = SQLClass.GetIdPacient(pacient);
                int idBio = SQLClass.GetIdBIO(codeProbirki, nameBio);

                // 3. Создание основного заказа (БЕЗ УСЛУГИ)
                SQLClass.InsertInOrder(idBio, idPacient);

                // 4. Получаем ID только что созданного заказа
                int idOrder = SQLClass.GetLastOrderId();

                // 5. Запись выбранных услуг в новую таблицу
                SQLClass.OpenConnection(); // Открываем соединение один раз перед циклом
                foreach (var service in SelectedServices)
                {
                    // Берем ID услуги прямо из объекта (в нашем классе это свойство Code)
                    int currentServiceId = service.Id;

                    // Вставляем запись в таблицу "Услуги_в_заказе"
                    // Судя по твоему скриншоту, туда нужно передать: id_Услуги, id_Заказа и Статус (по умолчанию 'Ожидание')
                    SQLClass.InsertServicesInOrder(currentServiceId, idOrder);
                }
                SQLClass.CloseConnection();

                MessageBox.Show($"Заказ №{idOrder} успешно оформлен!\nКоличество услуг: {SelectedServices.Count}");

                // Очистка формы для следующего заказа
                SelectedServices.Clear(); // Очищаем список чипсов
                CodeProbirki.Clear();
                BioName.Clear();
                FIOCombo.SelectedIndex = -1;
                TotalSumText.Text = "0 ₽";
                GoScanner.IsEnabled = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка при оформлении заказа: " /*+ ex.Message*/);
                SQLClass.CloseConnection(); 
            }

            
        }

  
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Lab lab = new Lab(Login, 3);
            lab.Show();
            this.Close();

        }

        private void CodeProbirki_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text,0))
            {
                e.Handled = true;
            }
        }

        private void AddPacientBD_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddPacient add = new AddPacient();
            add.Show();
            this.Close();
        }

        private void CodeProbirki_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Используем метод Early Return (ранний выход), чтобы код не превращался в "лесенку"
                if (CodeProbirki.Text.Length != 15)
                {
                    MessageBox.Show("Введенный код пробирки должен содержать 15 символов!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(BioName.Text))
                {
                    MessageBox.Show("Введите биоматериал!");
                    return;
                }

                try
                {
                    // 1. Блокировка интерфейса
                    _isCodeConfirmed = true;
                    CodeProbirki.IsReadOnly = true;
                    CodeProbirki.Focusable = false;
                    GoScanner.IsEnabled = false; // 
                    LockIcon.Text = "🔒";

                    // 2. Запись в БД
                    SQLClass.WrtieBioBD(CodeProbirki.Text, BioName.Text);

                    // 3. Работа с путями (Архитектурно лучше брать из настроек: Properties.Settings.Default.BarcodePath)
                    // Пока оставим твою папку Barcodes, но сделаем её правильно:
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string dataDir = System.IO.Path.Combine(baseDir, "Barcodes");

                    // Создаем папку, если её нет
                    if (!System.IO.Directory.Exists(dataDir))
                        System.IO.Directory.CreateDirectory(dataDir);

                    // Формируем имя файла: "КОД.pdf"
                    string fileName = CodeProbirki.Text + ".pdf";
                    string fullPath = System.IO.Path.Combine(dataDir, fileName);

                    // 4. Генерация штрих-кода
                    using (BarcodeGenerator generator = new BarcodeGenerator(EncodeTypes.Code39, CodeProbirki.Text))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            generator.Save(ms, BarCodeImageFormat.Bmp);

                            // 5. Создание PDF
                            using (Document doc = new Document())
                            {
                                doc.Pages.Add();

                                using (PdfFileMend mender = new PdfFileMend())
                                {
                                    mender.BindPdf(doc);

                                    // Добавляем изображение штрих-кода
                                    mender.AddImage(ms, 1, 100, 600, 200, 700);

                                    // Сохраняем сразу по нужному пути с расширением .pdf
                                    mender.Save(fullPath);

                                    // mender.Close(); <-- ЭТУ СТРОКУ УДАЛИЛИ. Блок using сделает всё сам.
                                }
                            }
                        }
                    }

                    MessageBox.Show($"Штрих-код сохранен в папку Barcodes как {fileName}", "Готово!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при генерации штрих-кода: {ex.Message}");
                    // Если произошла ошибка, можно разблокировать кнопку обратно
                    GoScanner.IsEnabled = true;
                }
            }
        }


        private void CodeProbirki_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CodeProbirki.IsReadOnly && _isCodeConfirmed)
            {
                var result = MessageBox.Show("Хотите изменить код пробирки?", "Изменение кода", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    CodeProbirki.IsReadOnly = false;
                    _isCodeConfirmed = false;
                    CodeProbirki.Focusable = true;
                    CodeProbirki.Focus();
                    LockIcon.Text = "🔓"; // открытый замок

                    // Перезапускаем таймер
                    if (_timer != null) _timer.Stop();
                    _timer = new DispatcherTimer();
                    _timer.Interval = TimeSpan.FromSeconds(30);
                    _timer.Tick += (timerSender, args) =>
                    {
                        if (!_isCodeConfirmed)
                        {
                            CodeProbirki.Text = "";
                            _timer.Stop();
                        }
                    };
                    _timer.Start();

                }

            }
        }

        private void CodeProbirki_PreviewLostKeyboardFocus(object sender, RoutedEventArgs e)
        {

        }
        private void ShowNotify(string message, int durationSeconds = 5)
        {
            NotificationText.Text = message;
            NotificationBorder.Visibility = Visibility.Visible;
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(durationSeconds);
            timer.Tick += (s, e) =>
            {
                NotificationBorder.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }

        
        // 1. Логика поиска
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchTextBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(query))
            {
                SuggestionsPopup.IsOpen = false;
                return;
            }

            // Ищем услуги, которые подходят под запрос, но которых ЕЩЕ НЕТ в выбранных
            var matches = AllServices
                .Where(s => (s.Name.ToLower().Contains(query) || s.Id.ToString().Contains(query))
                            && !SelectedServices.Any(selected => selected.Id == s.Id))
                .ToList();

            if (matches.Any())
            {
                SuggestionsList.ItemsSource = matches;
                SuggestionsPopup.IsOpen = true;
            }
            else
            {
                SuggestionsPopup.IsOpen = false;
            }
        }

        // 2. Добавление услуги (клик по выпадающему списку)
        private void SuggestionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuggestionsList.SelectedItem is Service selectedService)
            {
                SelectedServices.Add(selectedService);

                // Очищаем поиск и закрываем попап
                SearchTextBox.Text = string.Empty;
                SuggestionsPopup.IsOpen = false;

                UpdateTotalSum();
            }
        }

        // 3. Удаление услуги (клик по крестику на чипсе)
        private void RemoveChip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                // Берем код услуги из Tag кнопки
                int code = int.Parse(btn.Tag.ToString());
                var serviceToRemove = SelectedServices.FirstOrDefault(s => s.Id == code);

                if (serviceToRemove != null)
                {
                    SelectedServices.Remove(serviceToRemove);
                    UpdateTotalSum();
                }
            }
        }

        // 4. Пересчет суммы
        private void UpdateTotalSum()
        {
            decimal total = SelectedServices.Sum(s => s.Price);
            TotalSumText.Text = $"{total} ₽";
        }
    }
}
