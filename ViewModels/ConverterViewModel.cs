using Microsoft.Win32;
using Nti.XlsxReader;
using Nti.XlsxReader.Types;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Vab.WpfCommands.Commands;

namespace NtiConverter.ViewModels
{
    internal class ConverterViewModel : INotifyPropertyChanged
    {
        public XlsxReader Reader { get; }
        public NtiBase DataBase => Reader.DataBase;

        public ConverterViewModel()
        {
            Reader = new XlsxReader();
            OpenXlsxFileCmd = new RelayCommand(() => OpenXlsxFile());
        }

        public ICommand OpenXlsxFileCmd { get; }
        private void OpenXlsxFile()
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Nti Database (*.xlsx) | *.xlsx"
            };
            var dRes = ofd.ShowDialog();
            if (!dRes.HasValue || !dRes.Value) return;
            var reader = new XlsxReader();
            try
            {
                reader.OpenFile(ofd.FileName);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region PropertyChanged Impllementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
