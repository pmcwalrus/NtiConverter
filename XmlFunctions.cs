using Nti.XlsxReader.Entities;
using Nti.XlsxReader.Types;
using System;
using System.Collections.Generic;
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
            sb.AppendLine(GetShemms(data));
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
            var devices = data.Ip.Where(x => (x.DeviceType == DeviceType.Device 
            || x.DeviceType == DeviceType.ExternalSystem)).ToList();
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

                var signals = GetDeviceSignals(data, device.DeviceName);
                sb.AppendLine(signals);

                var addition = data.DeviceAdds.FirstOrDefault(x => x.DeviceName == device.DeviceName);
                if (addition != null)
                    sb.AppendLine(addition.Addition);
                sb.AppendLine("\t</device>");
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        public static string GetDeviceSignals(NtiBase data, string deviceName)
        {
            var sb = new StringBuilder();
            var signals = data.Layout.Where(x => x.DeviceIndex == deviceName).ToList();
            foreach(var signal in signals)
            {
                var param = data.Signals.FirstOrDefault(x => x.Index == signal.SignalName);
                if (param == null) continue;
                string modbusType;
                if (signal.Type.Contains("DI")) modbusType = "0x02";
                else if (signal.Type.Contains("DO")) modbusType = "0x0F";
                else if (signal.Type.Contains("AI")) modbusType = "0x04";
                else if (signal.Type.Contains("AO")) modbusType = "0x03";
                else modbusType = "???";
                sb.AppendLine($"\t\t<address start_addr=\"{signal.NumberStr}\" " +
                    $"type=\"{modbusType}\" parm=\"{param.SystemId}_{param.SignalId}\"/>");
            }
            return sb.ToString();
        }

        public static string GetShemms(NtiBase data)
        {
            var sb = new StringBuilder();
            var shmemList = GetShmemList(data);
            foreach (var shmem in shmemList)
            {
                sb.AppendLine(GetSingleShemem(data, shmem));
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        public static string GetSingleShemem(NtiBase data, string shmem)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\t<shmem name=\"{shmem}\" stale_timeout=\"45000\">");
            sb.AppendLine(GetShmemParams(data, shmem));
            sb.AppendLine($"\t</shmem>");
            return sb.ToString();
        }

        public static List<string> GetShmemList(NtiBase data)
        {
            var shmems = new List<string>();
            foreach (var signal in data.Layout)
            {
                var type = "???";
                if (signal.Type == "DI" || signal.Type == "DO") type = "DI-DO";
                else if (signal.Type.Contains("AI")) type = "AI";
                var shmem = $"nsc-{signal.DeviceIndex}-{type}";
                if (!shmems.Contains(shmem)) 
                    shmems.Add(shmem);
            }
            foreach(var signal in data.Signals.Where(x => !string.IsNullOrWhiteSpace(x.Shmem)))
            {
                var shmem = $"{signal.Shmem}";
                if (!shmems.Contains(shmem))
                    shmems.Add(shmem);
            }
            return shmems;
        }

        public static string GetShmemParams(NtiBase data, string shmem)
        {
            var sb = new StringBuilder();
            var parms = data.Signals.Where(x => x.Shmem == shmem);
            foreach (var parm in parms)
            {
                sb.Append(GetSingleParam(data, parm));                
            }
            return sb.ToString();
        }

        public static string GetSingleParam(NtiBase data, SignalEntity param)
        {
            var sb = new StringBuilder();
            var inverted = param.Inversion
                ? "inverted=\"true\" "
                : string.Empty;
            var alarmGroup = !string.IsNullOrWhiteSpace(param.Ups)
                ? $"alarm_group=\"{data.Ups.FirstOrDefault(x => x.Id == param.Ups).AlarmGroup}\" "
                : string.Empty;
            var paramName = $"{param.SystemId}_{param.SignalId}";
            sb.AppendLine($"\t\t<parm name=\"{paramName}\" " +
                $"type=\"{param.TypeString}\" {inverted} {alarmGroup}" +
                $"description=\"{param.Description}\"/>");
            if (!string.IsNullOrWhiteSpace(param.Units))
            {
                var unitsString = $"\t\t<parm name=\"{paramName}_u\" type=\"{param.TypeString}\" " +
                    $"script=\"alg.interpolation({paramName}, x1_f8017c1, y1_{paramName})\" " +
                    $"update_threshold=\"0.1\" description=\"{param.Description} в {param.Units}\"/>";
                sb.AppendLine(unitsString);
            }
            if (!string.IsNullOrWhiteSpace(param.SetpointTypesString))
            {
                var suffix = string.Empty;
                var type = string.Empty;
                var descriptionLevel = string.Empty;
                for (var i = 0; i < param.SetpointTypes.Count; i++)
                {
                    switch (param.SetpointTypes[i])
                    {
                        case SetpointTypes.LL:
                            suffix = "LL";
                            type = "critical_alarm";
                            descriptionLevel = "АНУ";
                            break;
                        case SetpointTypes.L:
                            suffix = "L";
                            type = "alarm";
                            descriptionLevel = "НУ";
                            break;
                        case SetpointTypes.H:
                            suffix = "H";
                            type = "alarm";
                            descriptionLevel = "ВУ";
                            break;
                        case SetpointTypes.HH:
                            suffix = "HH";
                            type = "critical_alarm";
                            descriptionLevel = "АВУ";
                            break;
                        case SetpointTypes.Unkown:
                            suffix = "???";
                            type = "???";
                            descriptionLevel = "???";
                            break;
                    }
                    var setpointString = $"\t\t<parm name=\"{paramName}_{suffix}\" " +
                        $"type=\"{type}\" delay_on=\"{param.DelayTimeString}\" " +
                        $"script=\"{paramName}_u &lt;={param.SetpointValues[i]}\" {alarmGroup}" +
                        $"description=\"{param.Description} {descriptionLevel}\"/>";
                    sb.AppendLine(setpointString);
                }
            }
            return sb.ToString();
        }
    }
}
