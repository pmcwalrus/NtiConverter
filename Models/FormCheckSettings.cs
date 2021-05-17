using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NtiConverter.Models
{
    internal class FormCheckSettings : INotifyPropertyChanged
    {
        public const string FormCheckSettingsFileName = "forms.ntifcs";

        private string _xmlFileName;
        public string XmlFileName
        {
            get => _xmlFileName;
            set
            {
                _xmlFileName = value;
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
