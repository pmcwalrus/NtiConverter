using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NtiConverter.Models
{
    internal class ReaderSettings : INotifyPropertyChanged
    {

        #region Headers

        #region Main Signal List Headers
        private string _descriptionHeader = "Наименование параметра";
        public string DescriptionHeader 
        { 
            get => _descriptionHeader;
            set
            {
                _descriptionHeader = value;
                OnPropertyChanged();
            }
        }
        private string _indexHeader = "Индекс параметра";
        public string IndexHeader 
        { 
            get => _indexHeader;
            set
            {
                _indexHeader = value;
                OnPropertyChanged();
            } 
        }
        private string _unitsHeader =  "Единицы измерения";
        public string UnitsHeader
        {
            get => _unitsHeader;
            set
            {
                _unitsHeader = value;
                OnPropertyChanged();
            }
        }
        private string _setpointsTypeHeader = "Тип уставки";
        public string SetpointsTypeHeader
        {
            get => _setpointsTypeHeader;
            set
            {
                _setpointsTypeHeader = value;
                OnPropertyChanged();
            }
        }
        private string _setpointValuesHeader = "Значение";
        public string SetpointValuesHeader
        {
            get => _setpointValuesHeader;
            set
            {
                _setpointValuesHeader = value;
                OnPropertyChanged();
            }
        }
        private string _delayTimeHeader = "Время задержки,с";
        public string DelayTimeHeader
        {
            get => _delayTimeHeader;
            set
            {
                _delayTimeHeader = value;
                OnPropertyChanged();
            }
        }
        private string _inversionHeader = "Инверсия";
        public string InversionHeader
        {
            get => _inversionHeader;
            set
            {
                _inversionHeader = value;
                OnPropertyChanged();
            }
        }
        private string _systemIdHeader = "System ID";
        public string SystemIdHeader
        {
            get => _systemIdHeader;
            set
            {
                _systemIdHeader = value;
                OnPropertyChanged();
            }
        }
        private string _signalIdHeader = "Signal ID (new)";
        public string SignalIdHeader
        {
            get => _signalIdHeader;
            set
            {
                _signalIdHeader = value;
                OnPropertyChanged();
            }
        }
        private string _signalTypeHeader = "Type";
        public string SignalTypeHeader
        {
            get => _signalTypeHeader;
            set
            {
                _signalTypeHeader = value;
                OnPropertyChanged();
            }
        }
        private string _pstsHeader = "ПСТС";
        public string PstsHeader
        {
            get => _pstsHeader;
            set
            {
                _pstsHeader = value;
                OnPropertyChanged();
            }
        }
        private string _shmemHeader = "shmem";
        public string ShmemHeader
        {
            get => _shmemHeader;
            set
            {
                _shmemHeader = value;
                OnPropertyChanged();
            }
        }
        private string _upsHeader = "УПС";
        public string UpsHeader
        {
            get => _upsHeader;
            set
            {
                _upsHeader = value;
                OnPropertyChanged();
            }
        }
        private string _signalTypeTextHeader = "Тип сигнала";
        public string SignalTypeTextHeader
        {
            get => _signalTypeTextHeader;
            set
            {
                _signalTypeTextHeader = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region IP Headers
        private string _ipDeviceHeader = "Устройство";
        public string IpDeviceHeader
        {
            get => _ipDeviceHeader;
            set
            {
                _ipDeviceHeader = value;
                OnPropertyChanged();
            }
        }
        private string _ipNetwork1Header = "Сеть 1";
        public string IpNetwork1Header
        {
            get => _ipNetwork1Header;
            set
            {
                _ipNetwork1Header = value;
                OnPropertyChanged();
            }
        }
        private string _ipNetwork2Header = "Сеть 2";
        public string IpNetwork2Header
        {
            get => _ipNetwork2Header;
            set
            {
                _ipNetwork2Header = value;
                OnPropertyChanged();
            }
        }
        private string _ipDeviceNameHeader = "device_name";
        public string IpDeviceNameHeader
        {
            get => _ipDeviceNameHeader;
            set
            {
                _ipDeviceNameHeader = value;
                OnPropertyChanged();
            }
        }
        private string _ipIFace1Header = "iface1";
        public string IpIFace1Header
        {
            get => _ipIFace1Header;
            set
            {
                _ipIFace1Header = value;
                OnPropertyChanged();
            }
        }
        private string _ipIFace2Header = "iface2";
        public string IpIFace2Header
        {
            get => _ipIFace2Header;
            set
            {
                _ipIFace2Header = value;
                OnPropertyChanged();
            }
        }
        private string _ipPriorityHeader = "Приоритет";
        public string IpPriorityHeader
        {
            get => _ipPriorityHeader;
            set
            {
                _ipPriorityHeader = value;
                OnPropertyChanged();
            }
        }
        private string _ipVideoGroupHeader = "Группа видеокадров";
        public string IpVideoGroupHeader
        {
            get => _ipVideoGroupHeader;
            set
            {
                _ipVideoGroupHeader = value;
                OnPropertyChanged();
            }
        }
        private string _ipControlHeader = "Управление при";
        public string IpControlHeader
        {
            get => _ipControlHeader;
            set
            {
                _ipControlHeader = value;
                OnPropertyChanged();
            }
        }
        private string _ipRegistartorHeader = "registrator";
        public string IpRegistartorHeader
        {
            get => _ipRegistartorHeader;
            set
            {
                _ipRegistartorHeader = value;
                OnPropertyChanged();
            }
        }
        private string _ipRegistartorTimeoutHeader = "registrator_timeout";
        public string IpRegistartorTimeoutHeader
        {
            get => _ipRegistartorTimeoutHeader;
            set
            {
                _ipRegistartorTimeoutHeader = value;
                OnPropertyChanged();
            }
        }
        private string _ipTypeWorkstationString = "Рабочие станции";
        public string IpTypeWorkstationString
        {
            get => _ipTypeWorkstationString;
            set
            {
                _ipTypeWorkstationString = value;
                OnPropertyChanged();
            }
        }
        private string _ipTypeDeviceString = "Приборы сопряжения";
        public string IpTypeDeviceString
        {
            get => _ipTypeDeviceString;
            set
            {
                _ipTypeDeviceString = value;
                OnPropertyChanged();
            }
        }
        private string _ipTypeExternalSystemString = "Чужие системы ";
        public string IpTypeExternalSystemString
        {
            get => _ipTypeExternalSystemString;
            set
            {
                _ipTypeExternalSystemString = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Ups Headers
        private string _upsIdHeader = "id";
        public string UpsIdHeader
        {
            get => _upsIdHeader;
            set
            {
                _upsIdHeader = value;
                OnPropertyChanged();
            }
        }
        private string _upsGroupHeader = "Группа УПС";
        public string UpsGroupHeader
        {
            get => _upsGroupHeader;
            set
            {
                _upsGroupHeader = value;
                OnPropertyChanged();
            }
        }
        private string _upsAlarmGroupHeader = "alarm_group";
        public string UpsAlarmGroupHeader
        {
            get => _upsAlarmGroupHeader;
            set
            {
                _upsAlarmGroupHeader = value;
                OnPropertyChanged();
            }
        }
        private string _upsWindowHeader = "Окно СК";
        public string UpsWindowHeader
        {
            get => _upsWindowHeader;
            set
            {
                _upsWindowHeader = value;
                OnPropertyChanged();
            }
        }

        #endregion
        private string _deviceAdditionSheetPreamble = "dev_";
        public string DeviceAdditionSheetPreamble
        {
            get => _deviceAdditionSheetPreamble;
            set
            {
                _deviceAdditionSheetPreamble = value;
                OnPropertyChanged();
            }
        }

        #region Layout Header
        private string _layoutStartSeparator = "Start_address";
        public string LayoutStartSeparator
        {
            get => _layoutStartSeparator;
            set
            {
                _layoutStartSeparator = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #endregion

        #region PropertyChanged Impllementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
