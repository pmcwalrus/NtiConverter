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
            SaveSettingsCmd = new RelayCommand(() => SaveSettings());
        }

        public ICommand SaveSettingsCmd { get; }
        private void SaveSettings()
        {
            SettingsFunctions.SaveObjectToJson(SettingsFileName, Settings);
            SettingsFunctions.ApplyHeaders(Settings);
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
