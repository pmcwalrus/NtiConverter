using Nti.XlsxReader.Types;
using System.IO;
using System.Linq;
using System.Text;

namespace NtiConverter
{
    internal static class XmlFunctions
    {
        /// <summary>
        /// Generate and save XML-file
        /// </summary>
        /// <param name="data">NTI-database for convertion</param>
        /// <param name="fileName">File name to save</param>
        /// <returns></returns>
        public static void SaveXml(NtiBase data, string fileName)
        {
            var xmlData = GetFileData(data);
            using var sw = new StreamWriter(fileName) { AutoFlush = true };
            sw.Write(xmlData);
        }

        public static string GetFileData(NtiBase data)
        {
            var sb = new StringBuilder();
            sb.AppendLine(data.XmlTop);
            sb.Append(GetModbusDevices(data));
            sb.Append(GetWorkstations(data));
            sb.Append(data.XmlBot);
            return sb.ToString();
        }

        public static string GetWorkstations(NtiBase data)
        {
            var sb = new StringBuilder();
            var workstations = data.Ip.Where(x => x.DeviceType == DeviceType.Worstation).ToList();
            foreach (var ws in workstations)
            {
                sb.AppendLine($"\t<workstation name=\"{ws.DeviceName}\"  " +
                    $"priority=\"{ws.Priority}\" " +
                    $"registrator=\"{ws.Registrator}\" " +
                    $"registrator_timeout=\"{ws.RegistartorTimeout}\" " +
                    $"force_start=\"false\" " +
                    $"parm_poweroff=\"{ws.DeviceName}_poweroff\">");
                sb.AppendLine($"\t\t<connection host=\"{ws.Network1}\" port=\"50201\" " +
                    $"parm=\"{ws.DeviceName}_con1\" iface=\"{ws.IFace1}\"/>");
                sb.AppendLine($"\t\t<connection host=\"{ws.Network2}\" port=\"50201\" " +
                     $"parm=\"{ws.DeviceName}_con1\" iface=\"{ws.IFace2}\"/>");
                sb.AppendLine("\t</workstation>");
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        public static string GetModbusDevices(NtiBase data)
        {
            var sb = new StringBuilder();
            var devices = data.Ip.Where(x => x.DeviceType == DeviceType.Device).ToList();
            foreach(var device in devices)
            {
                sb.AppendLine($"\t<device name=\"{device.DeviceName}\" " +
                    $"protocol=\"MODBUS-TCP\" address_offset=\"0\" " +
                    $"check_timeout_min=\"1\">");
                sb.AppendLine($"\t\t<connection host=\"{device.Network1}\" " +
                    $"port=\"502\" " +
                    $"parm=\"{device.DeviceName}_con1\"/>");
                sb.AppendLine($"\t\t<connection host=\"{device.Network2}\" " +
                    $"port=\"502\" " +
                    $"parm=\"{device.DeviceName}_con2\"/>");
                var addition = data.DeviceAdds.FirstOrDefault(x => x.DeviceName == device.DeviceName);
                if (addition != null)
                    sb.AppendLine(addition.Addition);
                sb.AppendLine("\t</device>");
                sb.Append("\r\n");
            }
            return sb.ToString();
        }
    }
}
