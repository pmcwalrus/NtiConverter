using NtiConverter.Functions;
using NtiConverter.Types;
using NtiConverter.ViewModels;
using NtiConverter.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace NtiConverter.Models
{
    internal class FormCheckSettings : INotifyPropertyChanged
    {
        public const string FormCheckSettingsFileName = "forms.ntifcs";

        public FormCheckSettings()
        {
        }

        private string _xmlFileName;
        public string XmlFileName
        {
            get => _xmlFileName;
            set
            {
                if (_xmlFileName == value) return;
                //if (!File.Exists(value))
                //{
                //    MessageBox.Show($"Файл {value} не найден!");
                //    _xmlFileName = null;
                //    SettingsFunctions.SaveObjectToJson(FormCheckSettingsFileName, this);
                //    OnPropertyChanged();
                //    return;
                //}
                _xmlFileName = value;
                SettingsFunctions.SaveObjectToJson(FormCheckSettingsFileName, this);
                OnPropertyChanged();
            }
        }

        private List<(string, List<string>)> _filesToCheckList = new();
        public List<(string ParamName, List<string> FilesToCheck)> FilesToCheckList
        {
            get => _filesToCheckList;
            set
            {
                _filesToCheckList = value;
                OnPropertyChanged();
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
