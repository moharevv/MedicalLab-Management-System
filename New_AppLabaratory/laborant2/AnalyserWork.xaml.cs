using Nancy.Json;
using New_AppLabaratory;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static AppLaboratory.Labs.Laborant2.AnalyserWork;

namespace AppLaboratory.Labs.Laborant2
{
    /// <summary>
    /// Логика взаимодействия для AnalyserWork.xaml
    /// </summary>
    public partial class AnalyserWork : Window
    {
        public AnalyserWork()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UslugiBioradCombo.IsEnabled = false;
            UslugiLedetectCombo.IsEnabled = false;
            OprosAnalyzerBiorad.IsEnabled = false;
            OprosAnalyzerLedetect.IsEnabled = false;

           
        }
        //нажати по анализатору Biorad
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SQLClass.OpenConnection();
            DataTable dt = SQLClass.ExecuteSql("select id_Услуги, id_Заказа, Статус\r\nFROM Услуга_В_заказе\r\ninner join services\r\non id_Услуги = services.Code where Analyser = '1' or Analyser = '1;2'");
            DontServicesList.ItemsSource = dt.DefaultView;
            UslugiBioradCombo.ItemsSource = SQLClass.Select("select Service \r\nfrom services\r\ninner join Услуга_В_заказе U\r\non Code = U.id_Услуги where Статус = 'Ожидание' and Analyser = '1' or Analyser = '1;2'", SQLClass.str);
            UslugiBioradCombo.IsEnabled = true;
            SQLClass.CloseConnection();
        }
        //нажати по анализатору Ledetect
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SQLClass.OpenConnection();
            DataTable dt = SQLClass.ExecuteSql("select id_Услуги, id_Заказа, Статус\r\nFROM Услуга_В_заказе\r\ninner join services\r\non id_Услуги = services.Code where Analyser = '2' or Analyser = '1;2'");
            DontServicesList.ItemsSource = dt.DefaultView;
            UslugiLedetectCombo.ItemsSource = SQLClass.Select("select Service \r\nfrom services\r\ninner join Услуга_В_заказе U\r\non Code = U.id_Услуги where Статус = 'Ожидание' and Analyser = '2' or Analyser = '1;2'", SQLClass.str);
            UslugiLedetectCombo.IsEnabled = true;
            SQLClass.CloseConnection();
        }
        public class Services
        {
            public int serviceCode { get; set; }
            public string result { get; set; }
        }
        public class GetAnalizator
        {
            public string patient { get; set; } // хранит пациента
            public List<Services> services { get; set; } // хранит услуги
            public int progress { get; set; }
        }
        //private void Sent_Reserch_Biorad_Click(object sender)
        //{
        //    DispatcherTimer timer = new DispatcherTimer();
        //    timer.Interval = TimeSpan.FromSeconds(1);
        //    int count = 0;
        //    timer.Tick += (_, __) =>
        //    {
        //        count++;
        //        ProgressBiorad.Value = (count / 30.0) * 100; // Обновляем значение ProgressBar
        //        if (ProgressBiorad.Value >= 100) // Проверяем значение ProgressBar
        //        {
        //            timer.Stop(); // Останавливаем таймер
        //            MessageBox.Show("ProgressBar заполнен!"); // Выводим сообщение
        //        }
        //    };
        //    timer.Start();

        //}

        private void Sent_Reserch_Biorad_Click(object sender, RoutedEventArgs e)
        {
            if (UslugiBioradCombo.SelectedItem != null)
            {
                Services services1 = new Services();
                services1.serviceCode = SQLClass.GetIdService(UslugiBioradCombo.SelectedItem.ToString());//получение кода из комбо
                List<Services> services = new List<Services>();
                services.Add(services1);

                string patient = "1";

                //SQLClass.GetIdPIZDEC(UslugiBioradCombo.SelectedItem.ToString());
                //string patient = SQLClass.GetIdPIZDEC(UslugiBioradCombo.SelectedItem.ToString()).ToString();

                // работа с API

                var httpWebRequest = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/Biorad");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = new JavaScriptSerializer().Serialize(new
                    {
                        patient,
                        services
                    });

                    streamWriter.Write(json);
                }

                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse(); // отправка запроса на API

                // cпращиваем хорошо ли  отиправлен запрось
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    MessageBox.Show("Услуги успешно отправлены!");

                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(1);
                    int count = 0;
                    timer.Tick += (_, __) =>
                    {
                        count++;
                        ProgressBiorad.Value = (count / 3.0) * 100; // Обновляем значение ProgressBar
                        if (ProgressBiorad.Value >= 100) // Проверяем значение ProgressBar
                        {
                            timer.Stop(); // Останавливаем таймер
                            MessageBox.Show("Done!"); // Выводим сообщение
                            OprosAnalyzerBiorad.IsEnabled = true;
                            Sent_Reserch_Biorad.IsEnabled = false;
                        }
                    };
                    timer.Start();
                }
                else
                {
                    MessageBox.Show("Ошибка отправки!");
                }
            }
            else
            {
                MessageBox.Show("Выберите услугу");
            }
            
        }

        private void OprosAnalyzerBiorad_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            int count = 0;
            timer.Tick += (_, __) =>
            {
                count++;
                ProgressBiorad.Value = (count / 3.0) * 100; // Обновляем значение ProgressBar
                if (ProgressBiorad.Value >= 100) // Проверяем значение ProgressBar
                {
                    timer.Stop(); // Останавливаем таймер
                    MessageBox.Show("Done!"); // Выводим сообщение



                    GetAnalizator getAnalizators = new GetAnalizator();

                    var httpWebRequest1 = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/Biorad");
                    httpWebRequest1.ContentType = "application/json";
                    httpWebRequest1.Method = "GET";
                    try
                    {
                        var httpResponse1 = (HttpWebResponse)httpWebRequest1.GetResponse();
                        if (httpResponse1.StatusCode == HttpStatusCode.OK)
                        {
                            using (Stream stream = httpResponse1.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream);
                                string json = reader.ReadToEnd();
                                JavaScriptSerializer serializer = new JavaScriptSerializer();
                                getAnalizators = serializer.Deserialize<GetAnalizator>(json);

                                foreach ( var services in getAnalizators.services)
                                {
                                    string patient = getAnalizators.patient;

                                    MessageBox.Show($"Получение результата :{getAnalizators.patient} , {services.result}");
                                }

                                MessageBoxButton btnMessageBox = MessageBoxButton.YesNoCancel;
                                MessageBoxImage icnMessageBox = MessageBoxImage.Question;

                                MessageBoxResult rsltMessageBox = MessageBox.Show("Принять результат?", "Окно", btnMessageBox, icnMessageBox);

                                switch (rsltMessageBox)
                                {
                                    case MessageBoxResult.Yes:
                                        MessageBox.Show("Изменение статуса на выполнено..");
                                        break;

                                    case MessageBoxResult.No:
                                        /* ... */
                                        break;

                                    case MessageBoxResult.Cancel:
                                        /* ... */
                                        break;
                                }

                                // MessageBox.Show($"Данные:{getAnalizators.patient} , {getAnalizators.services[0].ToString()}");

                            }
                        }
                        else
                        {
                            MessageBox.Show("ЧО errrors");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }

                }

            };
            timer.Start();
            
        }

        private void Sent_Reserch_Ledetect_Click(object sender, RoutedEventArgs e)
        {
            if (UslugiLedetectCombo.SelectedItem != null)
            {
                Services services1 = new Services();
                services1.serviceCode = SQLClass.GetIdService(UslugiLedetectCombo.SelectedItem.ToString());//получение кода из комбо
                List<Services> services = new List<Services>();
                services.Add(services1);

                string patient = "2";

                //SQLClass.GetIdPIZDEC(UslugiBioradCombo.SelectedItem.ToString());
                //string patient = SQLClass.GetIdPIZDEC(UslugiBioradCombo.SelectedItem.ToString()).ToString();

                // работа с API

                var httpWebRequest = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/Ledetect");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = new JavaScriptSerializer().Serialize(new
                    {
                        patient,
                        services
                    });

                    streamWriter.Write(json);
                }

                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse(); // отправка запроса на API

                // cпращиваем хорошо ли  отиправлен запрось
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    MessageBox.Show("Услуги успешно отправлены!");

                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(1);
                    int count = 0;
                    timer.Tick += (_, __) =>
                    {
                        count++;
                        ProgressLedetect.Value = (count / 3.0) * 100; // Обновляем значение ProgressBar
                        if (ProgressLedetect.Value >= 100) // Проверяем значение ProgressBar
                        {
                            timer.Stop(); // Останавливаем таймер
                            MessageBox.Show("Done!"); // Выводим сообщение
                            OprosAnalyzerLedetect.IsEnabled = true;
                            Sent_Reserch_Ledetect.IsEnabled = false;
                        }
                    };
                    timer.Start();
                }
                else
                {
                    MessageBox.Show("Ошибка отправки!");
                }
            }
            else
            {
                MessageBox.Show("Выберите услугу");
            }
        }
        private void OprosAnalyzerLedetect_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            int count = 0;
            timer.Tick += (_, __) =>
            {
                count++;
                ProgressLedetect.Value = (count / 3.0) * 100; // Обновляем значение ProgressBar
                if (ProgressLedetect.Value >= 100) // Проверяем значение ProgressBar
                {
                    timer.Stop(); // Останавливаем таймер
                    MessageBox.Show("Done!"); // Выводим сообщение

                    GetAnalizator getAnalizators = new GetAnalizator();

                    var httpWebRequest1 = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/Ledetect");
                    httpWebRequest1.ContentType = "application/json";
                    httpWebRequest1.Method = "GET";
                    try
                    {
                        var httpResponse1 = (HttpWebResponse)httpWebRequest1.GetResponse();
                        if (httpResponse1.StatusCode == HttpStatusCode.OK)
                        {
                            using (Stream stream = httpResponse1.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream);
                                string json = reader.ReadToEnd();
                                JavaScriptSerializer serializer = new JavaScriptSerializer();
                                getAnalizators = serializer.Deserialize<GetAnalizator>(json);

                                foreach (var services in getAnalizators.services)
                                {
                                    string patient = getAnalizators.patient;

                                    MessageBox.Show($"Получение результата :{getAnalizators.patient} , {services.result}");
                                }

                                MessageBoxButton btnMessageBox = MessageBoxButton.YesNoCancel;
                                MessageBoxImage icnMessageBox = MessageBoxImage.Question;

                                MessageBoxResult rsltMessageBox = MessageBox.Show("Принять результат?", "Окно", btnMessageBox, icnMessageBox);

                                switch (rsltMessageBox)
                                {
                                    case MessageBoxResult.Yes:
                                        MessageBox.Show("Изменение статуса на выполнено..");
                                        UslugiLedetectCombo.SelectedItem.ToString();
                                        break;

                                    case MessageBoxResult.No:
                                        OprosAnalyzerLedetect.IsEnabled = false;
                                        ProgressLedetect.Value = 0;
                                        Sent_Reserch_Ledetect.IsEnabled = true;

                                        break;

                                    case MessageBoxResult.Cancel:
                                        OprosAnalyzerLedetect.IsEnabled = false;
                                        ProgressLedetect.Value = 0;
                                        Sent_Reserch_Ledetect.IsEnabled = true;
                                        break;
                                }

                                // MessageBox.Show($"Данные:{getAnalizators.patient} , {getAnalizators.services[0].ToString()}");

                            }
                        }
                        else
                        {
                            MessageBox.Show("ЧО errrors");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            };
            timer.Start();
        }
    }
}
