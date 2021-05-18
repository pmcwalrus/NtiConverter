using Microsoft.Win32;
using NtiConverter.Functions;
using NtiConverter.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Vab.WpfCommands.Commands;

namespace NtiConverter.Types
{
    internal class FormEntity : INotifyPropertyChanged
    {
        public FormCheckSettings Settings { get; set; }
        public FormEntity()
        {
            FilesToCheck = new ObservableCollection<string>();
            AddFilesToCheckCmd = new RelayCommand(() => AddFilesToCheck());
            ClearFilesCmd = new RelayCommand(() => ClearFiles());
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
                value.CollectionChanged += FilesToCheckCollectionChanged;
                SaveFilesToCheck();
                OnPropertyChanged();
            }
        }

        private void FilesToCheckCollectionChanged(object sender,NotifyCollectionChangedEventArgs e)
        {
            SaveFilesToCheck();
        }

        private void SaveFilesToCheck()
        {
            if (Settings == null) return;
            var savedParam = Settings.FilesToCheckList.FirstOrDefault(x => x.ParamName == Name);
            if (savedParam.ParamName == null)
            {
                Settings.FilesToCheckList.Add((Name, FilesToCheck.ToList()));
            }
            else
            {
                if (savedParam.FilesToCheck == null)
                    savedParam.FilesToCheck = new List<string>();
                foreach(var file in FilesToCheck)
                {
                    if (savedParam.FilesToCheck.Contains(file))
                        continue;
                    savedParam.FilesToCheck.Add(file);                    
                }
                var listToDelete = new List<string>();
                foreach (var file in savedParam.FilesToCheck)
                {
                    if (!FilesToCheck.Contains(file))
                        listToDelete.Add(file);
                        
                }
                foreach (var d in listToDelete)
                    savedParam.FilesToCheck.Remove(d);
            }
            SettingsFunctions.SaveObjectToJson(FormCheckSettings.FormCheckSettingsFileName, Settings);
        }

        public ICommand AddFilesToCheckCmd { get; }
        private void AddFilesToCheck()
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "*.ui | *.ui",
            };
            var res = ofd.ShowDialog();
            if (!res.HasValue || !res.Value) return;
            foreach (var file in ofd.FileNames)
            {
                FilesToCheck.Add(Path.GetRelativePath(Directory.GetCurrentDirectory(), file));
            }
        }

        public ICommand ClearFilesCmd { get; }
        private void ClearFiles()
        {
            FilesToCheck = new ObservableCollection<string>();
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
