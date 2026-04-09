using Aspose.BarCode.Generation;
using Aspose.Pdf;
using Aspose.Pdf.Facades;
using New_AppLabaratory;
using BarcodeLib;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AppLaboratory.Labs.Laborant.Biomaterial
{
    /// <summary>
    /// Логика взаимодействия для CreateBarcode.xaml
    /// </summary>
    public partial class CreateBarcode : Window
    {
        public CreateBarcode()
        {
            InitializeComponent();
        }

        private void CreateBar_Click(object sender, RoutedEventArgs e)
        {
            if (CodeForCreate.Text.Length == 15)
            {
                if (Biomaterial.Text.Length != 0)
                {

                    SQLClass.WrtieBioBD(CodeForCreate.Text, Biomaterial.Text);//запись в биоматериалы

                    //создание штриха  
                    string dataDir = @"C:\Users\DNS\Desktop\LAST\AppLaboratory\AppLaboratory\Barcodes\";


                    // Создайте объект линейного штрих-кода, установите текст кода и тип символики для штрих-кода.
                    BarcodeGenerator generator = new BarcodeGenerator(EncodeTypes.Code39, CodeForCreate.Text);

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
                    string name = CodeForCreate.Text;
                    System.IO.File.Move(dataDir + "barcode.pdf", name);
                    // Закрыть объект PdfFileMend
                    mender.Close();

                    System.Windows.MessageBox.Show("Готово!");
                }
                else
                {
                    System.Windows.MessageBox.Show("Введите биоматериал!");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Код должен содержать 15 цифр!");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
