using NtiConverter.Types;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NtiConverter.ViewModels
{
    internal class FormViewModel : INotifyPropertyChanged
    {
        private FormEntity _entity;
        public FormEntity Entity
        {
            get => _entity;
            set
            {
                if (_entity != null)
                {
                    _entity.FilesToCheck.CollectionChanged -= FilesToCheckCollectionChanged;
                    _entity.PropertyChanged -= EntityPropertyChanged;
                }                    
                _entity = value;
                value.PropertyChanged += EntityPropertyChanged;
                value.FilesToCheck.CollectionChanged += FilesToCheckCollectionChanged;
                OnPropertyChanged();
            }
        }

        private void EntityPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FilesToCheck")
            {
                Entity.FilesToCheck.CollectionChanged += FilesToCheckCollectionChanged;
                var str = string.Empty;
                foreach (var file in Entity.FilesToCheck)
                    str += $"{file};";
                CheckFilesString = str;
            }
        }

        private void FilesToCheckCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var str = string.Empty;
            foreach (var file in Entity.FilesToCheck)
                str += $"{file};";
            CheckFilesString = str;
        }

        private string _checkFilesString;
        public string CheckFilesString
        {
            get => _checkFilesString;
            set
            {
                _checkFilesString = value;
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
