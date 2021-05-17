using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Vab.WpfCommands.Commands;

namespace NtiConverter.Types
{
    internal class FormEntity : INotifyPropertyChanged
    {
        public FormEntity()
        {
            FilesToCheck = new ObservableCollection<string>();
            AddFilesToCheckCmd = new RelayCommand(() => AddFilesToCheck());
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private List<string> _scriptParams;
        public List<string> ScriptParams
        {
            get => _scriptParams;
            set
            {
                _scriptParams = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _filesToCheck;
        public ObservableCollection<string> FilesToCheck
        {
            get => _filesToCheck;
            set
            {
                _filesToCheck = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddFilesToCheckCmd { get; }
        private void AddFilesToCheck()
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = true,
              //  Filter = "*.ui | *.ui",
            };
            var res = ofd.ShowDialog();
            if (!res.HasValue || !res.Value) return;
            foreach (var file in ofd.FileNames)
            {
                FilesToCheck.Add(Path.GetRelativePath(Directory.GetCurrentDirectory(), file));
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
