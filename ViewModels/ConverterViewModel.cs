using Microsoft.Win32;
using Nti.XlsxReader;
using Nti.XlsxReader.Types;
using NtiConverter.Functions;
using NtiConverter.Views;
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
            OpenXlsxFileCmd = new RelayCommand(() => SelectAndOpenXlsxFile());
            SaveXmlCmd = new RelayCommand(() => SaveXml());
            ShowSettingsWindowCmd = new RelayCommand(() => ShowSettingsWindow());
            AnalyzeXmlCmd = new RelayCommand(() => AnalyzeXml());
        }

        #region Select XLSX

        public ICommand OpenXlsxFileCmd { get; }
        private void SelectAndOpenXlsxFile()
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Nti Database (*.xlsx) | *.xlsx"
            };
            var dRes = ofd.ShowDialog();
            if (!dRes.HasValue || !dRes.Value) return;
            OpenXlsxFile(ofd.FileName);
        }

        public void OpenXlsxFile(string fileName)
        {
            try
            {
                Reader.OpenFile(fileName);
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}\r\n\r\n{e.StackTrace}",
                    "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public ICommand ShowSettingsWindowCmd { get; }
        private void ShowSettingsWindow()
        {
            var win = new SettingsWindow();
            win.ShowDialog();
        }

        #endregion

        #region Create XML

        public ICommand SaveXmlCmd { get; }
        private void SaveXml()
        {            
            var sfd = new SaveFileDialog
            {
                DefaultExt = "xml",
                AddExtension = true
            };
            var dialogRes = sfd.ShowDialog();
            if (!dialogRes.HasValue || !dialogRes.Value) return;            
            try
            {
                XmlFunctions.SaveXml(DataBase, sfd.FileName);
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}\r\n\r\n{e.StackTrace}",
                    "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private string _xmlAlarmAnalyzeResult;
        public string XmlAlarmAnalyzeResult
        {
            get => _xmlAlarmAnalyzeResult;
            set
            {
                _xmlAlarmAnalyzeResult = value;
                OnPropertyChanged();
            }
        }

        public ICommand AnalyzeXmlCmd { get; }
        private void AnalyzeXml()
        {
            XmlAlarmAnalyzeResult = string.Empty;
            var ofd = new OpenFileDialog
            {
                Filter = "XML | *.xml"
            };
            var dRes = ofd.ShowDialog();
            if (!dRes.HasValue || !dRes.Value) return;
            try
            {
                XmlAlarmAnalyzeResult = XmlFunctions.AnalyzeXml(ofd.FileName);
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}\r\n\r\n{e.StackTrace}", 
                    "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
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
