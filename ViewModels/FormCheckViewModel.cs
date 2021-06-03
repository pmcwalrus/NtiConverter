using Microsoft.Win32;
using NtiConverter.Functions;
using NtiConverter.Models;
using NtiConverter.Types;
using NtiConverter.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
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
            FormControls = new ObservableCollection<FormControl>();
            SelectXmlCmd = new RelayCommand(() => SelectXml());
            CheckFormsCmd = new RelayCommand(() => CheckForms());
            if (!File.Exists(FormCheckSettings.FormCheckSettingsFileName))
            {
                Settings = new FormCheckSettings();
                SettingsFunctions.SaveObjectToJson(FormCheckSettings.FormCheckSettingsFileName, Settings);
            }
            else
            {
                Settings = SettingsFunctions.LoadObjectFromJson<FormCheckSettings>(FormCheckSettings.FormCheckSettingsFileName);
                if (!string.IsNullOrWhiteSpace(Settings.XmlFileName))
                {
                    try
                    {
                        ReadFormsFromXml();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}",
                            "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }            
            Settings.PropertyChanged += SettingsPropertyChanged;
        }

        private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "XmlFileName") return;
            try
            {
                ReadFormsFromXml();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}",
                    "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReadFormsFromXml()
        {
            FormControls = new ObservableCollection<FormControl>();
            var formList = XmlFunctions.GetFormListFromXml(Settings.XmlFileName);
            if (formList == null) return;
            foreach (var form in formList)
            {
                form.Settings = Settings;
                var savedFiles = Settings.FilesToCheckList.FirstOrDefault(x => x.ParamName == form.Name);
                if (savedFiles.FilesToCheck != null)
                {
                    form.FilesToCheck = new ObservableCollection<string>(savedFiles.FilesToCheck);
                }
                var control = new FormControl()
                {
                    DataContext = new FormViewModel
                    {
                        Entity = form,
                    },
                };
                FormControls.Add(control);
            }
        }

        private ObservableCollection<FormControl> _formControls;
        public ObservableCollection<FormControl> FormControls
        {
            get => _formControls;
            set
            {
                _formControls = value;
                IsCheckEnabled = value != null && value.Count > 0;
                value.CollectionChanged += FormControlsCollectionChanged;
                OnPropertyChanged();
            }
        }

        private void FormControlsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsCheckEnabled = FormControls.Count > 0;
        }

        private bool _isCheckEnabled;
        public bool IsCheckEnabled
        {
            get => _isCheckEnabled;
            set
            {
                _isCheckEnabled = value;
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
            Settings.XmlFileName = Path.GetRelativePath(Directory.GetCurrentDirectory(), ofd.FileName);            
        }

        public ICommand CheckFormsCmd { get; }
        private void CheckForms()
        {
            try
            {
                var entities = new List<FormEntity>();
                foreach (var formControl in FormControls)
                {
                    if (formControl.DataContext is not FormViewModel vm) return;
                    entities.Add(vm.Entity);
                }
                var result = FormFunctions.CheckForms(entities);
                if (string.IsNullOrWhiteSpace(result))
                {
                    MessageBox.Show($"Все файлы прошли проверку.",
                        "Успешно!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var win = new MessageTextWindow();
                    win.Tb.Text = result;
                    win.ShowDialog();
                }
            }
            catch(Exception e)
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
