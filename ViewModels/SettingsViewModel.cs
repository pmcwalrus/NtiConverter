﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NtiConverter.ViewModels
{
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        #region PropertyChanged Impllementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
