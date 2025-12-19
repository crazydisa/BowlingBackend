using PdfTableReader2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PdfTableReader2
{
   

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<PlayerResult> Results { get; set; } = new ObservableCollection<PlayerResult>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var reader = new PdfTableReader();
            var allTables = await reader.GetAllTablesFromPdf(@"C:\Projects\BowlingBackend\pdf\chufo_tumen_2023 (1) (1) (2)-005.pdf");
            //foreach (var table in allTables)
            //{
                foreach(var result in allTables[0])
                {
                    Results.Add(result); 
                }
            //}
        }
    }

    

}
