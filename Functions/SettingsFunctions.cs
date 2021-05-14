using Newtonsoft.Json;
using Nti.XlsxReader.Types;
using NtiConverter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtiConverter.Functions
{
    internal static class SettingsFunctions
    {
        public static void SaveObjectToJson(string fileName, object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            using var sw = new StreamWriter(fileName) { AutoFlush = true };
            sw.Write(json);
        }

        public static TType LoadObjectFromJson<TType>(string fileName)
        {
            string fileContent;
            using (var sr = new StreamReader(fileName))
            {
                fileContent = sr.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<TType>(fileContent);
        }

        public static void ApplyHeaders(ReaderSettings settings)
        {
            Headers.DescriptionHeader = settings.DescriptionHeader;
            Headers.IndexHeader = settings.IndexHeader;
            Headers.UnitsHeader = settings.UnitsHeader;
            Headers.SetpointsTypeHeader = settings.SetpointsTypeHeader;
            Headers.SetpointValuesHeader = settings.SetpointValuesHeader;
            Headers.DelayTimeHeader = settings.DelayTimeHeader;
            Headers.InversionHeader = settings.InversionHeader;
            Headers.SystemIdHeader = settings.SystemIdHeader;
            Headers.SignalIdHeader = settings.SignalIdHeader;
            Headers.SignalTypeHeader = settings.SignalTypeHeader;
            Headers.PstsHeader = settings.PstsHeader;
            Headers.ShmemHeader = settings.ShmemHeader;
            Headers.UpsHeader = settings.UpsHeader;
            Headers.SignalTypeTextHeader = settings.SignalTypeTextHeader;
        }
    }
}
