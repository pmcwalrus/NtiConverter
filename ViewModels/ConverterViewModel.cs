using Microsoft.Win32;
using Nti.XlsxReader;
using Nti.XlsxReader.Types;
using NtiConverter.Functions;
using NtiConverter.Views;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
            CheckLayoutCmd = new RelayCommand(() => CheckLayout());
            CheckSignalListCmd = new RelayCommand(() => CheckSignalList());
            GetDoubleParamsCmd = new RelayCommand(() => GetDoubleParams());
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

        public async void OpenXlsxFile(string fileName)
        {
            ProgressValue = 0;
            Progress<int> progress = new Progress<int>();
            progress.ProgressChanged += Progress_ProgressChanged;
            try
            {
                await Task.Run(() => Reader.OpenFile(fileName, progress));                
                ProgressValue = 0;
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}\r\n\r\n{e.StackTrace}",
                    "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
                ProgressValue = 0;
            }
        }

        private int _progressValue = 0;
        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                OnPropertyChanged();
            }
        }

        private void Progress_ProgressChanged(object sender, int e)
        {
            ProgressValue = e;
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
                return;
            }
            MessageBox.Show($"Успешно!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
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

        public ICommand GetDoubleParamsCmd { get; }
        private void GetDoubleParams()
        {
            var ofd = new OpenFileDialog
            {
                Filter = "XML | *.xml"
            };
            var dRes = ofd.ShowDialog();
            if (!dRes.HasValue || !dRes.Value) return;
            try
            {
                var result = XmlFunctions.FindDoubleParams(ofd.FileName);
                if (string.IsNullOrWhiteSpace(result))
                {
                    MessageBox.Show($"Нет дублирующих параметров",
                        "GOOD JOB!", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var win = new MessageTextWindow();
                win.Tb.Text = result.ToString();
                win.ShowDialog();
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}\r\n\r\n{e.StackTrace}",
                    "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public ICommand CheckLayoutCmd { get; }
        private void CheckLayout()
        {
            var signals = XlsxFunctions.CheckLayout(DataBase);
            if (signals == null || signals.Count == 0)
            {
                MessageBox.Show($"Все сигналы перечня есть в раскладке.",
                    "GOOD JOB!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var sb = new StringBuilder();
            sb.AppendLine("Индексы сигналов в перечне, которых нет в раскладке:");
            foreach (var s in  signals)
            {
                sb.AppendLine(s.Index);
            }
            var win = new MessageTextWindow();
            win.Tb.Text = sb.ToString();
            win.ShowDialog();
        }

        public ICommand CheckSignalListCmd { get; }
        private void CheckSignalList()
        {
            var signals = XlsxFunctions.CheckSignalList(DataBase);
            if (signals == null || signals.Count == 0)
            {
                MessageBox.Show($"Все сигналы раскладки есть в перечне.",
                    "GOOD JOB!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var sb = new StringBuilder();
            sb.AppendLine("Индексы сигналов в раскладке, которых нет в перечне:");
            foreach (var s in signals)
            {
                sb.AppendLine(s.SignalName);
            }
            var win = new MessageTextWindow();
            win.Tb.Text = sb.ToString();
            win.ShowDialog();
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
