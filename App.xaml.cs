using NtiConverter.Functions;
using NtiConverter.Models;
using NtiConverter.ViewModels;
using System.IO;
using System.Windows;

namespace NtiConverter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ReaderSettings settings;
            if (!File.Exists(SettingsViewModel.SettingsFileName))
            {
                settings = new ReaderSettings();
                SettingsFunctions.SaveObjectToJson(SettingsViewModel.SettingsFileName, settings);
            }
            else
            {
                settings = SettingsFunctions.LoadObjectFromJson<ReaderSettings>(SettingsViewModel.SettingsFileName);
            }
            SettingsFunctions.ApplyHeaders(settings);
        }
    }
}
