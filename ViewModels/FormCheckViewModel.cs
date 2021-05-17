using Microsoft.Win32;
using NtiConverter.Functions;
using NtiConverter.Models;
using NtiConverter.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Vab.WpfCommands.Commands;

namespace NtiConverter.ViewModels
{
    internal class FormCheckViewModel : INotifyPropertyChanged
    {
        private FormCheckSettings _settings;
        public FormCheckSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public FormCheckViewModel()
        {
            if (!File.Exists(FormCheckSettings.FormCheckSettingsFileName))
            {
                Settings = new FormCheckSettings();
                SettingsFunctions.SaveObjectToJson(FormCheckSettings.FormCheckSettingsFileName, Settings);
            }
            else
            {
                Settings = SettingsFunctions.LoadObjectFromJson<FormCheckSettings>(FormCheckSettings.FormCheckSettingsFileName);
            }
            SelectXmlCmd = new RelayCommand(() => SelectXml());
        }

        private ObservableCollection<FormEntity> _xmlForms;
        public ObservableCollection<FormEntity> XmlForms
        {
            get => _xmlForms;
            set
            {
                _xmlForms = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectXmlCmd { get; }
        private void SelectXml()
        {
            var ofd = new OpenFileDialog
            {
                Filter = "XML | *.xml"
            };
            var dRes = ofd.ShowDialog();
            if (!dRes.HasValue || !dRes.Value) return;
            Settings.XmlFileName = ofd.FileName;
            XmlForms = new ObservableCollection<FormEntity>(
                XmlFunctions.GetFormListFromXml(ofd.FileName));
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
