using Microsoft.Win32;
using NtiConverter.Functions;
using NtiConverter.Models;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Vab.WpfCommands.Commands;

namespace NtiConverter.ViewModels
{
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        public const string SettingsFileName = "settings.ntirs";

        private ReaderSettings _settings;
        public ReaderSettings Settings 
        { 
            get => _settings; 
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public SettingsViewModel()
        {
            if (!File.Exists(SettingsFileName))
            {
                Settings = new ReaderSettings();
                SettingsFunctions.SaveObjectToJson(SettingsFileName, Settings);
            }
            else
            {
                Settings = SettingsFunctions.LoadObjectFromJson<ReaderSettings>(SettingsFileName);
            }
            SettingsFunctions.ApplyHeaders(Settings);
            ApplySettingsCmd = new RelayCommand(() => ApplySettings());
            SaveSettingsCmd = new RelayCommand(() => SaveSettings());
            OpenSettingsCmd = new RelayCommand(() => OpenSettings());
        }

        public ICommand ApplySettingsCmd { get; }
        private void ApplySettings()
        {
            SettingsFunctions.SaveObjectToJson(SettingsFileName, Settings);
            SettingsFunctions.ApplyHeaders(Settings);
        }

        public ICommand SaveSettingsCmd { get; }
        private void SaveSettings()
        {
            var sfd = new SaveFileDialog()
            {
                DefaultExt = "ntirs",
                Filter = "*.ntirs | *.ntirs"
            };
            var res = sfd.ShowDialog();
            if (!res.HasValue || !res.Value) return;
            SettingsFunctions.SaveObjectToJson(sfd.FileName, Settings);
        }        

        public ICommand OpenSettingsCmd { get; }
        private void OpenSettings()
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "*.ntirs | *.ntirs"
            };
            var res = ofd.ShowDialog();
            if (!res.HasValue || !res.Value) return;
            Settings = SettingsFunctions.LoadObjectFromJson<ReaderSettings>(ofd.FileName);
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
