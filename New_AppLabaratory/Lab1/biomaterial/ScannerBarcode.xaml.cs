using AForge.Video;
using AForge.Video.DirectShow;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using ZXing;
using ZXing;

namespace AppLaboratory.Labs.Laborant.Biomaterial
{
    public partial class ScannerBarcode : Window
    {
        // --- Переменные для работы с камерой ---
        private FilterInfoCollection videoDevices;       // Список всех доступных веб-камер
        private VideoCaptureDevice videoSource;          // Выбранное устройство (камера)
        private BarcodeReader barcodeReader;             // Объект для распознавания штрих-кодов
        private bool isScanning = true;                  // Флаг, разрешающий распознавание

        // --- Переменные для PDF (из вашего кода) ---
        public string fileDir;
        public string Codee;

        public ScannerBarcode()
        {
            InitializeComponent();
            barcodeReader = new BarcodeReader
            {
                Options = new ZXing.Common.DecodingOptions
                {
                    PossibleFormats = new List<BarcodeFormat>
        {
            BarcodeFormat.CODE_39,
            BarcodeFormat.CODE_128,
            BarcodeFormat.EAN_13
        },
                    TryHarder = true   // заставляет ZXing более тщательно анализировать кадр
                }
            };
            barcodeReader = new BarcodeReader(); // Инициализируем ридер
        }

        // --- Запуск камеры ---
        private void StartCameraButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Получаем список всех видеоустройств (веб-камер)
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0)
                {
                    System.Windows.MessageBox.Show("На вашем устройстве не найдено веб-камер.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 2. Выбираем первую камеру из списка. Чтобы выбрать другую, можно изменить индекс [0]
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

                // 3. Подписываемся на событие получения нового кадра
                videoSource.NewFrame += VideoSource_NewFrame;

                // 4. Запускаем видеопоток
                videoSource.Start();

                // 5. Обновляем состояние кнопок в UI
                StartCameraButton.IsEnabled = false;
                StopCameraButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при запуске камеры: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Остановка камеры ---
        private void StopCameraButton_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }

        // --- Событие обработки нового кадра с камеры ---
        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Этот метод выполняется в фоновом потоке, поэтому для обновления UI используем Dispatcher
            try
            {
                // 1. Получаем изображение с камеры
                using (Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    // 2. Отображаем видео на PictureBox. Обязательно через Invoke, т.к. PictureBox - WinForms элемент.
                    Dispatcher.Invoke(() =>
                    {
                        if (videoPlayer.Image != null)
                            videoPlayer.Image.Dispose(); // Освобождаем предыдущее изображение
                        videoPlayer.Image = (Bitmap)bitmap.Clone();
                    });

                    // 3. Пытаемся распознать штрих-код на кадре (если флаг разрешает)
                    if (isScanning)
                    {
                        var result = barcodeReader.Decode(bitmap);
                        if (result != null && !string.IsNullOrEmpty(result.Text))
                        {
                            // Останавливаем дальнейшее распознавание на этом кадре
                            isScanning = false;

                            // 4. Передаём результат в главный поток для обработки
                            Dispatcher.Invoke(() =>
                            {
                                // Здесь вы можете вставить полученный код в нужное поле, например:
                               

                                // Или, как у вас было, передать в глобальную переменную и показать сообщение
                                Codee = result.Text;
                                System.Windows.MessageBox.Show($"Сканирование успешно! Код: {result.Text}");
                                txtBarcodeCode.Text = result.Text;
                                // Не останавливаем камеру, а сбрасываем флаг для следующего сканирования
                                isScanning = true;
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в обработчике кадров: {ex.Message}");
            }
        }

        // --- Метод для корректной остановки камеры и освобождения ресурсов ---
        private void StopCamera()
        {
            Dispatcher.Invoke(() =>
            {
                if (videoSource != null && videoSource.IsRunning)
                {
                    videoSource.NewFrame -= VideoSource_NewFrame;
                    videoSource.SignalToStop();
                    videoSource.WaitForStop();
                    videoSource = null;
                }

                // Очищаем изображение в PictureBox
                if (videoPlayer.Image != null)
                {
                    videoPlayer.Image.Dispose();
                    videoPlayer.Image = null;
                }

                // Обновляем состояние кнопок
                StartCameraButton.IsEnabled = true;
                StopCameraButton.IsEnabled = false;
                isScanning = true; // Сбрасываем флаг для следующего сканирования
            });
        }

        // --- Ваши старые методы (оставлены без изменений) ---
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OpenFileDialog = new OpenFileDialog();
            if (OpenFileDialog.ShowDialog() == true)
            {
                txtFilePath.Text = OpenFileDialog.FileName;
                fileDir = txtFilePath.Text;
            }
        }

        private void GetCodeScanner_Click(object sender, RoutedEventArgs e)
        {
            // Здесь был ваш код для сканирования из PDF
            // Я его оставил без изменений, так как вы его не просили трогать.
            // Но, возможно, вы захотите его удалить или переработать, так как теперь у вас есть сканер с камеры.
            string file1 = fileDir;
            if (string.IsNullOrEmpty(file1))
            {
                System.Windows.MessageBox.Show("Пожалуйста, сначала выберите файл PDF.");
                return;
            }
            try
            {
                Aspose.Pdf.Document pdfDoc = new Aspose.Pdf.Document(file1);
                for (int i = 1; i <= pdfDoc.Pages.Count; ++i)
                {
                    MemoryStream ms = pdfDoc.Pages[i].ConvertToPNGMemoryStream();
                    ms.Position = 0;
                    Aspose.BarCode.BarCodeRecognition.BarCodeReader reader = new Aspose.BarCode.BarCodeRecognition.BarCodeReader(ms);
                    foreach (Aspose.BarCode.BarCodeRecognition.BarCodeResult result in reader.ReadBarCodes())
                    {
                        Codee = result.CodeText;
                        System.Windows.MessageBox.Show("Сканирование.. " + result.CodeText);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при сканировании PDF: {ex.Message}");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            bool CodeFromCamera = true;
            StopCamera();
            WelcomeBio biomaterial = new WelcomeBio("", Codee, CodeFromCamera);
            biomaterial.Show();
            this.Close();
        }
    }
}