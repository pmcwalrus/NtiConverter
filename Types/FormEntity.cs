using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NtiConverter.Types
{
    internal class FormEntity : INotifyPropertyChanged
    {
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

        #region PropertyChanged Impllementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
