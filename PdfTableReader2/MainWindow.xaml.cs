using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Data;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using PdfTableReaderDll;

namespace PdfTableReader2
{
   

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<PlayerResult> Results { get; set; } = new List<PlayerResult>();

        public MainWindow()
        {
            InitializeComponent();
            var tableReader = new PdfTableReader("C:\\projects\\PdfTableReader\\PdfTableReader\\chufo_tumen_2023 (1) (1) (2).pdf\\chufo_tumen_2023 (1) (1) (2)-001.pdf");
            Results = tableReader.LoadDataFromPdf();
            DataContext = this;
        }

        
    }

    

}
