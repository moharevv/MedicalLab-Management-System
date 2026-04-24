using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Web.Script.Serialization;
using New_AppLabaratory.Classes;
using New_AppLabaratory;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.Generic;


namespace AppLaboratory.Labs.Laborant2
{
    // Вспомогательный класс для отображения в списке
    public class ServiceItem : INotifyPropertyChanged
    {
        public string ServiceName { get; set; }
        public int ServiceCode { get; set; }
        public int OrderId { get; set; }
        public string AnalyzerName { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        private string _status;
        public string Status { get => _status; set { _status = value; OnPropertyChanged("Status"); } }

        public string StatusColor => Status == "Необходим повторный забор биоматериала" ? "#DC3545" : "#666666";

        private string _resultValue;
        public string ResultValue { get => _resultValue; set { _resultValue = value; OnPropertyChanged("ResultValue"); } }

        private bool _canStart = true;
        public bool CanStart { get => _canStart; set { _canStart = value; OnPropertyChanged("CanStart"); } }

        private bool _showApprove = false;
        public bool ShowApprove { get => _showApprove; set { _showApprove = value; OnPropertyChanged("ShowApprove"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public partial class AnalyserWork : Window
    {
        public ObservableCollection<ServiceItem> Services { get; set; } = new ObservableCollection<ServiceItem>();
        public ObservableCollection<ServiceItem> InProcessServices { get; set; } = new ObservableCollection<ServiceItem>();
        private readonly string login;
        private readonly int userType;

        private bool isMultiSelectMode;

        public AnalyserWork()
            : this(string.Empty, 4)
        {
        }

        public AnalyserWork(string login, int userType)
        {
            InitializeComponent();

            this.login = login;
            this.userType = userType;

            DontServicesList.ItemsSource = Services;
            InProcessList.ItemsSource = InProcessServices;

            SelectColumn.Width = 0;
            BulkSendButton.Visibility = Visibility.Collapsed;
            BulkSendButton.IsEnabled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) { }

        private void Button_Click(object sender, RoutedEventArgs e) => LoadData("Biorad");
        private void Button_Click_1(object sender, RoutedEventArgs e) => LoadData("Ledetect");

        private void LoadData(string analyzerName)
        {
            string code = (analyzerName == "Biorad") ? "1" : "2";

            // 1. Запрос для "Новых" и "Повторных"
            string sqlNew = $@"SELECT s.Service, u.id_Услуги, u.id_Заказа, u.Статус 
                       FROM Услуга_В_заказе u INNER JOIN services s ON u.id_Услуги = s.Code 
                       WHERE (s.Analyser = '{code}' OR s.Analyser = '1;2') 
                       AND (u.Статус = 'Ожидание' OR u.Статус = 'Необходим повторный забор биоматериала')";

            // 2. Запрос для тех, что уже "В работе"
            string sqlProcess = $@"SELECT s.Service, u.id_Услуги, u.id_Заказа, u.Статус, u.Result 
                       FROM Услуга_В_заказе u 
                       INNER JOIN services s ON u.id_Услуги = s.Code 
                       WHERE (s.Analyser = '{code}' OR s.Analyser = '1;2') 
                       AND (u.Статус = 'Отправлена на исследование'
                       OR u.Статус = 'Ожидает подтверждения лаборанта')";

            FillCollection(Services, sqlNew, analyzerName);
            FillCollection(InProcessServices, sqlProcess, analyzerName);
        }
        private void FillCollection(ObservableCollection<ServiceItem> col, string sql, string analyzerName)
        {
            var dt = SQLClass.ExecuteSql(sql);
            col.Clear();
            foreach (System.Data.DataRow row in dt.Rows)
            {
                var item = new ServiceItem
                {
                    ServiceName = row["Service"].ToString(),
                    ServiceCode = Convert.ToInt32(row["id_Услуги"]),
                    OrderId = Convert.ToInt32(row["id_Заказа"]),
                    Status = row["Статус"].ToString(),
                    AnalyzerName = analyzerName
                };
                // Если загружаем из БД уже отправленную услугу, даем возможность увидеть результат
                if (item.Status == "Отправлена на исследование")
                {
                    item.CanStart = false;
                    // Если в базе уже есть результат, показываем кнопки
                    if (row.Table.Columns.Contains("Result") && !string.IsNullOrEmpty(row["Result"].ToString()))
                    {
                        item.ResultValue = row["Result"].ToString();
                        item.ShowApprove = true;
                    }
                }
                else if (item.Status == "Ожидает подтверждения лаборанта")
                {
                    item.CanStart = false;
                    item.ShowApprove = true;

                    // ВАЖНО: подгружаем результат из БД
                    if (row.Table.Columns.Contains("Result") && !string.IsNullOrEmpty(row["Result"].ToString()))
                    {
                        item.ResultValue = row["Result"].ToString();
                    }
                }
                col.Add(item);
            }
        }

        private void RejectRow_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext as ServiceItem;
            if (item == null) return;

            var dr = MessageBox.Show("Вы уверены, что хотите отклонить результат? Потребуется повторный забор биоматериала.",
                                     "Отмена результата", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dr == MessageBoxResult.Yes)
            {
                string newStatus = "Необходим повторный забор биоматериала";
                SQLClass.RejectServiceAndClearResult(item.OrderId, item.ServiceCode, newStatus);

                // Убираем из списка "В работе" и переносим в "Очередь"
                InProcessServices.Remove(item);
                item.Status = newStatus;
                item.ShowApprove = false;
                item.CanStart = true;
                item.ResultValue = "";
                Services.Add(item);
            }
        }

        private async void SendRow_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext as ServiceItem;
            if (item == null) return;

            string previousStatus = item.Status;

            item.CanStart = false;
            item.Status = "Подключение...";

            Services.Remove(item);
            InProcessServices.Add(item);

            try
            {
                await RunAnalyzer(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);

                InProcessServices.Remove(item);
                Services.Add(item);

                // ВОЗВРАЩАЕМ ВСЁ НАЗАД
                item.Status = previousStatus;
                item.CanStart = true;

                // сбрасываем статус в БД обратно на "Ожидание"
                SQLClass.UpdateServiceStatus(item.OrderId, item.ServiceCode, previousStatus);
            }
        }

        private async Task RunAnalyzer(ServiceItem item)
        {
            using (var client = new HttpClient())
            {
                // Формируем JSON
                var payload = new { patient = item.OrderId.ToString(), services = new[] { new { serviceCode = item.ServiceCode } } };
                var jsonReq = new JavaScriptSerializer().Serialize(payload);
                var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");

                HttpResponseMessage resp;
                try
                {
                    // Отправляем POST
                    resp = await client.PostAsync($"http://localhost:5000/api/analyzer/{item.AnalyzerName}", content);
                }
                catch
                {
                    throw new Exception("Не удалось установить соединение с анализатором. Проверьте запуск LIMSAnalyzers.exe");
                }

                if (resp.IsSuccessStatusCode)
                {
                    // ТОЛЬКО ТЕПЕРЬ, когда API принял запрос, обновляем базу
                    item.Status = "В работе...";
                    SQLClass.UpdateServiceStatus(item.OrderId, item.ServiceCode, "Отправлена на исследование");

                    // Запускаем опрос результата
                    await StartPolling(item);
                }
                else
                {
                    // Если анализатор ответил, но не 200 OK (например 400 или 500)
                    string errorInfo = await resp.Content.ReadAsStringAsync();
                    throw new Exception($"Анализатор отклонил запрос: {resp.StatusCode}. {errorInfo}");
                }
            }
        }

        private async Task StartPolling(ServiceItem item)
        {
            ProgressBar pb = item.AnalyzerName == "Biorad" ? ProgressBiorad : ProgressLedetect;
            TextBlock statusText = item.AnalyzerName == "Biorad" ? StatusBioradText : StatusLedetectText;

            using (var client = new HttpClient())
            {
                bool isDone = false;

                while (!isDone)
                {
                    await Task.Delay(10);

                    var resp = await client.GetAsync($"http://localhost:5000/api/analyzer/{item.AnalyzerName}");
                    resp.EnsureSuccessStatusCode();

                    string json = await resp.Content.ReadAsStringAsync();
                    var data = new JavaScriptSerializer().Deserialize<GetAnalizator>(json);

                    if (data == null)
                        continue;

                    if (data.progress.HasValue)
                    {
                        int progress = Math.Max(0, Math.Min(100, data.progress.Value));

                        pb.Value = progress;
                        statusText.Text = $"Статус: {progress}%";
                        item.Status = $"В работе ({progress}%)";
                    }

                    if (data.services != null)
                    {
                        var res = data.services.FirstOrDefault(s => s.serviceCode == item.ServiceCode);
                        if (res != null)
                        {
                            pb.Value = 100;
                            statusText.Text = "Статус: 100%";
                            item.ResultValue = res.Result;

                            SQLClass.SaveServiceResult(item.OrderId, item.ServiceCode, item.ResultValue);

                            item.Status = "Ожидает подтверждения лаборанта";
                            item.ShowApprove = true;

                            SQLClass.UpdateServiceStatus(item.OrderId, item.ServiceCode, "Ожидает подтверждения лаборанта");

                            isDone = true;
                        }
                    }
                }
            }
        }

        private void ApproveRow_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext as ServiceItem;
            if (item == null) return;

            SQLClass.UpdateServiceStatus(item.OrderId, item.ServiceCode, "Выполнена");
            SQLClass.SaveServiceResult(item.OrderId, item.ServiceCode, item.ResultValue);

            InProcessServices.Remove(item);

            if (item.AnalyzerName == "Biorad") ProgressBiorad.Value = 0;
            else ProgressLedetect.Value = 0;
        }

        private void ToggleMultiSelect_Click(object sender, RoutedEventArgs e)
        {
            isMultiSelectMode = !isMultiSelectMode;

            SelectColumn.Width = isMultiSelectMode ? 50 : 0;
            BulkSendButton.Visibility = isMultiSelectMode ? Visibility.Visible : Visibility.Collapsed;
            ToggleMultiSelectButton.Content = isMultiSelectMode ? "Отменить выбор" : "Выбрать несколько услуг";

            foreach (var item in Services)
            {
                item.IsSelected = false;
            }

            UpdateBulkSendButtonState();
        }

        private void UpdateBulkSendButtonState()
        {
            int selectedCount = Services.Count(x => x.IsSelected);

            BulkSendButton.IsEnabled = selectedCount > 0;
            BulkSendButton.Content = selectedCount > 0
                ? $"Исследовать выбранные ({selectedCount})"
                : "Исследовать выбранные";
        }

        private void SelectionCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateBulkSendButtonState();
        }

        private async void BulkSend_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = Services.Where(x => x.IsSelected).ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одну услугу.");
                return;
            }

            if (selectedItems.Select(x => x.OrderId).Distinct().Count() > 1)
            {
                MessageBox.Show("Сейчас можно отправлять несколько услуг только в рамках одного заказа.");
                return;
            }

            foreach (var item in selectedItems)
            {
                item.CanStart = false;
                item.Status = "Подключение...";
                item.IsSelected = false;

                Services.Remove(item);
                InProcessServices.Add(item);
            }

            UpdateBulkSendButtonState();

            try
            {
                await RunAnalyzerBatch(selectedItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);

                foreach (var item in selectedItems)
                {
                    InProcessServices.Remove(item);
                    Services.Add(item);
                    item.Status = "Ожидание";
                    item.CanStart = true;
                    SQLClass.UpdateServiceStatus(item.OrderId, item.ServiceCode, "Ожидание");
                }
            }
        }

        private async Task RunAnalyzerBatch(List<ServiceItem> items)
        {
            if (items == null || items.Count == 0)
                return;

            string analyzerName = items[0].AnalyzerName;

            using (var client = new HttpClient())
            {
                var payload = new
                {
                    patient = items[0].OrderId.ToString(),
                    services = items.Select(x => new { serviceCode = x.ServiceCode }).ToArray()
                };

                var jsonReq = new JavaScriptSerializer().Serialize(payload);
                var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");

                HttpResponseMessage resp;
                try
                {
                    resp = await client.PostAsync($"http://localhost:5000/api/analyzer/{analyzerName}", content);
                }
                catch
                {
                    throw new Exception("Не удалось установить соединение с анализатором. Проверьте запуск LIMSAnalyzers.exe");
                }

                if (!resp.IsSuccessStatusCode)
                {
                    string errorInfo = await resp.Content.ReadAsStringAsync();
                    throw new Exception($"Анализатор отклонил запрос: {resp.StatusCode}. {errorInfo}");
                }

                foreach (var item in items)
                {
                    item.Status = "В работе...";
                    SQLClass.UpdateServiceStatus(item.OrderId, item.ServiceCode, "Отправлена на исследование");
                }

                await StartPollingBatch(items);
            }
        }

        private async Task StartPollingBatch(List<ServiceItem> items)
        {
            if (items == null || items.Count == 0)
                return;

            ProgressBar pb = items[0].AnalyzerName == "Biorad" ? ProgressBiorad : ProgressLedetect;
            TextBlock statusText = items[0].AnalyzerName == "Biorad" ? StatusBioradText : StatusLedetectText;

            using (var client = new HttpClient())
            {
                bool isDone = false;

                while (!isDone)
                {
                    await Task.Delay(2000);

                    var resp = await client.GetAsync($"http://localhost:5000/api/analyzer/{items[0].AnalyzerName}");
                    resp.EnsureSuccessStatusCode();

                    string json = await resp.Content.ReadAsStringAsync();
                    var data = new JavaScriptSerializer().Deserialize<GetAnalizator>(json);

                    if (data == null)
                        continue;

                    if (data.progress.HasValue)
                    {
                        int progress = Math.Max(0, Math.Min(100, data.progress.Value));
                        pb.Value = progress;
                        statusText.Text = $"Статус: {progress}%";

                        foreach (var item in items)
                        {
                            item.Status = $"В работе ({progress}%)";
                        }
                    }

                    if (data.services != null)
                    {
                        int completedCount = 0;

                        foreach (var item in items)
                        {
                            var res = data.services.FirstOrDefault(s => s.serviceCode == item.ServiceCode);
                            if (res == null)
                                continue;

                            item.ResultValue = res.Result;
                            item.Status = "Ожидает подтверждения лаборанта";
                            item.ShowApprove = true;
                            item.CanStart = false;

                            SQLClass.SaveServiceResult(item.OrderId, item.ServiceCode, item.ResultValue);
                            SQLClass.UpdateServiceStatus(item.OrderId, item.ServiceCode, "Ожидает подтверждения лаборанта");

                            completedCount++;
                        }

                        if (completedCount == items.Count)
                        {
                            pb.Value = 100;
                            statusText.Text = "Статус: 100%";
                            isDone = true;
                        }
                    }
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Lab_res labResWindow = new Lab_res(login, userType);
            labResWindow.Show();
            Close();
        }
    }
}
