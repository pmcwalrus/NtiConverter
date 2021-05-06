using NtiConverter.ViewModels;
using System.Linq;
using System.Windows;

namespace NtiConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Drop(object sender, DragEventArgs e)
        {
            if (this.DataContext is not ConverterViewModel vm) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var xlsxFile = files.FirstOrDefault(x => x.EndsWith(".xlsx"));
            if (xlsxFile == null) return;
            vm.OpenXlsxFile(xlsxFile);
        }
    }
}
