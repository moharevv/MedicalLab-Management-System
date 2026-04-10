using AppLaboratory.Labs.Laborant.Biomaterial;
using Aspose.BarCode.Generation;
using Aspose.Pdf;
using Aspose.Pdf.Annotations;
using Aspose.Pdf.Facades;
using BarcodeLib;
using New_AppLabaratory;
using System;
using System.Collections.Generic;
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
using System.Xml.Linq;
using Path = System.IO.Path;

namespace AppLaboratory.Labs
{

    /// <summary>
    /// Логика взаимодействия для Biomaterial.xaml
    /// </summary>
    public partial class WelcomeBio : Window
    {
        
        
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


        }
        private void Window_Loaded(object sender, RoutedEventArgs e) 
        { 
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
                // автомат генерация куар кода: 013 (код в бд+1) + 280326 (из дейтайм достать вот такой формат) + 154895 (рандом 6 цифр)
                // Генерация кода
                string randomPart = _rnd.Next(100000, 999999).ToString("D6");
                int nextOrderId = SQLClass.GetLastOrderId() + 1;
                CodeProbirki.Text = nextOrderId.ToString("D3") + DateTime.Now.ToString("ddMMyy") + randomPart;

                _isCodeConfirmed = false;
                CodeProbirki.IsReadOnly = false;

                // сообщаем пользователю что он должен подтвердить сгенерированый куар нажав ентер,
                //либо сканировать сканером
                ShowNotify("Был сгенерирован код пробирки, сохраните его, нажав ENTER, либо воспользуйтесь сканером");

                // Запуск таймера
                if (_timer != null) _timer.Stop();
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(30);
                _timer.Tick += (s, args) =>
                {
                    if (!_isCodeConfirmed)
                    {
                        CodeProbirki.Text = "";
                        _timer.Stop();
                    }
                };
                _timer.Start();

                SQLClass.OpenConnection();
                // загрузка фамилий пациентов и услуг из бд в комбобоксы
                FIOCombo.ItemsSource = SQLClass.Select("select ФИО from Данные_пациентов", SQLClass.str);
                ServicesCombo.ItemsSource = SQLClass.Select("select Service from services", SQLClass.str);
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
            if (GoScanner.IsEnabled == false)
            {
                if (CodeProbirki.Text.Length == 15)
                {
                    if (BioName.Text.Length != 0)
                    {
                        if (FIOCombo.SelectedIndex > -1)
                        {
                            if (ServicesCombo.SelectedIndex > -1)
                            {
                                string service = ServicesCombo.SelectedItem.ToString();
                                string pacient = FIOCombo.SelectedItem.ToString();
                                string codeProbirki = CodeProbirki.Text; // получение кода пробирки
                                string nameBio = BioName.Text;
                                int idService = SQLClass.GetIdService(service);
                                int idPacient = SQLClass.GetIdPacient(pacient);
                                int idBio = SQLClass.GetIdBIO(codeProbirki, nameBio);

                                MessageBox.Show(idService + "- id в услуги");
                                MessageBox.Show(idPacient + "- id в пациента");
                                MessageBox.Show(idBio + "- id в био");
                                SQLClass.InsertInOrder(idBio, idPacient, idService);
                                MessageBox.Show("Заказ оформлен!");
                                GoScanner.IsEnabled = true;

                                // запись в таблицу "услуги в заказе" для API
                                SQLClass.OpenConnection();
                                SQLClass.InsertServicesInOrder(idService, SQLClass.GetLastOrderId());
                                SQLClass.CloseConnection();
                            }
                            else
                            {
                                MessageBox.Show("Выберите услугу!");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Выберите пациента!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Введите биоматериал!");
                    }
                }
                else
                {
                    MessageBox.Show("Код должен содержать 15 цифр!");
                }
            }
            else
            {
                MessageBox.Show("Создайте или отсканируйте штрих-код!");
            }
        }
        private void AddNewPacientBD_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        //private void BIOCombo_Selected(object sender, RoutedEventArgs e)
        //{
        //    CodeProbirki.Text = SQLClass.GetCodeProbirki(BIOCombo.SelectedItem.ToString());
        //}

       

        

  
       

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
                if (CodeProbirki.Text.Length == 15)
                {
                    if (BioName.Text.Length != 0)
                    {

                        _isCodeConfirmed = true;
                        _timer?.Stop();
                        CodeProbirki.IsReadOnly = true;
                        CodeProbirki.Focusable = false;


                        SQLClass.WrtieBioBD(CodeProbirki.Text, BioName.Text);//запись в биоматериалы

                        //создание штриха  
                        string dataDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Barcodes");


                        // Создайте объект линейного штрих-кода, установите текст кода и тип символики для штрих-кода.
                        BarcodeGenerator generator = new BarcodeGenerator(EncodeTypes.Code39, CodeProbirki.Text);

                        // Создать поток памяти и сохранить изображение штрих-кода в поток памяти
                        Stream ms = new MemoryStream();
                        generator.Save(ms, BarCodeImageFormat.Bmp);

                        // Создайте документ PDF и добавьте страницу в документ
                        Document doc = new Document();
                        doc.Pages.Add();

                        // Открыть документ
                        PdfFileMend mender = new PdfFileMend();

                        // Привяжите PDF, чтобы добавить штрих-код
                        mender.BindPdf(doc);

                        // Добавить изображение штрих-кода в файл PDF
                        mender.AddImage(ms, 1, 100, 600, 200, 700);

                        // Сохранить изменения
                        //string filesave = CodeForCreate + ".pdf";
                        mender.Save(dataDir + "barcode.pdf"); //"barcode.pdf"
                        string name = CodeProbirki.Text;
                        System.IO.File.Move(dataDir + "barcode.pdf", name);
                        // Закрыть объект PdfFileMend
                        mender.Close();

                        LockIcon.Text = "🔒"; // закрытый замок
                                              // Выполните подтверждение (сохранение в БД, переход дальше и т.п.)
                        MessageBox.Show("Вы подтвердили ввод сгенерированного кода!");

                        MessageBox.Show("Готово!");
                    }
                    else
                    {
                        MessageBox.Show("Введите биоматериал!");
                    }
                }
                else
                {
                    MessageBox.Show("Введенный код пробирки должен содержать 15 символов!");
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
            //if (CodeProbirki.Text.Length == 15)
            //{
            //    MessageBoxResult result = MessageBox.Show(
            //"Сгенерированный код для пробирки пропадет, вы хотите напечатать вручную/воспользоваться сканером?",
            //"Подтверждение",
            //MessageBoxButton.OKCancel,
            // MessageBoxImage.Question);

            //    if (result == MessageBoxResult.OK)
            //    {
            //        if (!_isCodeConfirmed)
            //        {
            //            CodeProbirki.Text = "";
            //            _timer?.Stop();
            //        }
            //    }
            //    else
            //    {
            //        e.Handled = true; // отменяет уход фокуса
            //    }
            //}    

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

       
    }
}
