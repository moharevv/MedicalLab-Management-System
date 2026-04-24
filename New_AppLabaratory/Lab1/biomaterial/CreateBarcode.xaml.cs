using Aspose.BarCode.Generation;
using Aspose.Pdf;
using Aspose.Pdf.Facades;
using New_AppLabaratory;
using System;
using System.IO;
using System.Windows;

namespace AppLaboratory.Labs.Laborant.Biomaterial
{
    public partial class CreateBarcode : Window
    {
        public CreateBarcode()
        {
            InitializeComponent();
        }

        private void CreateBar_Click(object sender, RoutedEventArgs e)
        {
            string barcode = this.CodeForCreate.Text.Trim();
            string biomaterial = this.Biomaterial.Text.Trim();

            if (barcode.Length != 15)
            {
                MessageBox.Show("Код должен содержать 15 цифр!");
                return;
            }

            if (string.IsNullOrWhiteSpace(biomaterial))
            {
                MessageBox.Show("Введите биоматериал!");
                return;
            }

            try
            {
                SQLClass.WriteBiomaterial(barcode, biomaterial);

                string barcodeDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Barcodes");
                Directory.CreateDirectory(barcodeDirectory);

                string filePath = Path.Combine(barcodeDirectory, barcode + ".pdf");

                BarcodeGenerator generator = new BarcodeGenerator(EncodeTypes.Code39, barcode);
                using (MemoryStream stream = new MemoryStream())
                {
                    generator.Save(stream, BarCodeImageFormat.Bmp);
                    stream.Position = 0;

                    Document document = new Document();
                    document.Pages.Add();

                    PdfFileMend pdfMender = new PdfFileMend();
                    try
                    {
                        pdfMender.BindPdf(document);
                        pdfMender.AddImage(stream, 1, 100, 600, 200, 700);
                        pdfMender.Save(filePath);
                    }
                    finally
                    {
                        pdfMender.Close();
                    }
                }

                MessageBox.Show("Штрих-код сохранен.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось создать штрих-код: " + ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
