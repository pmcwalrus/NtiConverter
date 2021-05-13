using Nti.XlsxReader.Entities;
using Nti.XlsxReader.Types;
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
            sb.AppendLine(data.AddShmems);
            sb.AppendLine(GetPusOaps(data));
            sb.AppendLine(GetConnections(data));
            sb.AppendLine(GetShemms(data));
            sb.Append(GetModbusDevices(data));
            sb.Append(GetWorkstations(data));
            sb.Append(data.XmlBot);
            return sb.ToString();
        }

        public static string GetConnections(NtiBase data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("\t<shmem name=\"nsc-dev-connection\" keepalive=\"true\">");
            foreach (var device in data.Ip)
            {
                sb.AppendLine($"\t\t<parm name=\"{device.DeviceName}_con1\" type=\"int\"/>");
                sb.AppendLine($"\t\t<parm name=\"{device.DeviceName}_con2\" type=\"int\"/>");
            }
            foreach (var device in data.Ip)
            {
                sb.AppendLine($"\t\t<parm name=\"{device.DeviceName}_nc1\" " +
                    $"type=\"alarm\" script=\"{device.DeviceName}_con1 &lt; 704\" delay_on=\"5000\" " +
                    $"description=\"Нет связи с {device.Device} по сети 1\"/>");
                sb.AppendLine($"\t\t<parm name=\"{device.DeviceName}_nc2\" " +
                    $"type=\"alarm\" script=\"{device.DeviceName}_con2 &lt; 704\" delay_on=\"5000\" " +
                    $"description=\"Нет связи с {device.Device} по сети 2\"/>");
            }
            foreach(var device in data.Ip.Where(x => x.DeviceType == DeviceType.Worstation))
            {
                sb.AppendLine($"\t\t<parm name=\"{device.DeviceName}_poweroff\" " +
                    $"type=\"bool\" description=\"Выключение станции {device.Device}\"/>");
            }
            sb.AppendLine("\t</shmem>");
            return sb.ToString();
        }

        public static string GetPusOaps(NtiBase data)
        {
            var sb = new StringBuilder();
            var skList = new List<string>();
            sb.AppendLine("\t<shmem name=\"nsc-pus-OAPS-write\" stale_timeout=\"45000\">");
            foreach (var ups in data.Ups)
            {
                sb.AppendLine(GetUps(data, ups));
                if (!skList.Contains(ups.AlarmGroup))
                    skList.Add(ups.AlarmGroup);
            }
            foreach (var sk in skList)
            {
                sb.AppendLine(GetSk(data, sk));
            }
            foreach (var sk in skList)
            {
                sb.AppendLine($"\t\t<parm name=\"SK_{sk}_alarm\" type=\"bool\" " +
                    $"script=\"set_PUS(get_alarms(SK_{sk}), SK_{sk}_alarm_btn)\"/>");
            }
            foreach (var sk in skList)
            {
                sb.AppendLine($"\t\t<parm name=\"SK_{sk}_ack\" type=\"bool\" " +
                    $"script=\"set_PUS(get_alarms_ack(SK_{sk}), SK_{sk}_ack_btn)\"/>");
            }
            foreach (var ups in data.Ups)
            {
                sb.AppendLine($"\t\t<parm name=\"UPS_{ups.Id}_alarm\" type=\"bool\" " +
                    $"script=\"set_PUS(get_alarms(UPS_{ups.Id}), UPS_{ups.Id}_alarm_btn)\"/>");
            }
            foreach (var ups in data.Ups)
            {
                sb.AppendLine($"\t\t<parm name=\"UPS_{ups.Id}_ack\" type=\"bool\" " +
                    $"script=\"set_PUS(get_alarms_ack(UPS_{ups.Id}), UPS_{ups.Id}_ack_btn)\"/>");
            }
            foreach (var ups in data.Ups)
            {
                sb.AppendLine($"\t\t<parm name=\"UPS_{ups.Id}_cmd\" type=\"int\" " +
                    $"script=\"set_OAPS(UPS_{ups.Id}_alarm, UPS_{ups.Id}_ack)\"/>");
            }
            sb.AppendLine(GetStaticDataPusOaps());
            var skScript = string.Empty;
            foreach (var sk in skList)
            {
                skScript += string.IsNullOrEmpty(skScript)
                    ? $"SK_{sk} "
                    : $"| SK_{sk} ";
            }
            sb.AppendLine("\t\t<parm name=\"sound_on\" type=\"bool\" description=\"Звук РСТ\" " +
                $"script=\"get_alarms_not_ack({skScript})\"/>");
            sb.AppendLine("\t<shmem>");
            return sb.ToString();
        }

        public static string GetStaticDataPusOaps()
        {
            return "\t\t<parm name=\"kdmp_auto_on\" type=\"int\" description=\"Автовключение КДМП по параметру АПС\"/>\r\n" +
            "\t\t<parm name=\"autokdmp_impulse\" type=\"bool\" script=\"get_alarms_not_ack(kdmp_auto_on) &amp;&amp; SYS_new_alarm\"/>\r\n" +
		    "\t\t<parm name=\"autokdmp_alarm\" type=\"alarm\" script=\"autokdmp_impulse\" description=\"Сработало автовключение КДМП по АПС механической установки\"/>\r\n" +
  		    "\t\t<parm name=\"btn_mute\" type=\"bool\"/>\r\n" +                                
            "\t\t<parm name=\"mute_sound\" type=\"bool\" script=\"alg.reset_after_timeout('mute', qs_mute_fn(SYS_new_alarm, btn_mute, mute_sound), 30000)\"/>";
        }

        public static string GetUps(NtiBase data, UpsEntity ups)
        {
            var parmsString = string.Empty;
            var parms = data.Signals.Where(x => x.Ups == ups.Id);
            foreach (var parm in parms)
            {
                if (parm.Type == SignalTypes.Alarm || parm.Type == SignalTypes.CritcalAlarm)
                    parmsString += string.IsNullOrEmpty(parmsString)
                        ? $"{parm.SystemId}_{parm.SignalId} "
                        : $"| {parm.SystemId}_{parm.SignalId} ";
                if (parm.Is420mA)
                    parmsString += string.IsNullOrEmpty(parmsString)
                        ? $"{parm.SystemId}_{parm.SignalId}_f0 "
                        : $"| {parm.SystemId}_{parm.SignalId}_f0 ";
                if (parm.SetpointTypes != null)
                {
                    foreach (var sp in parm.SetpointTypes)
                    {
                        string suffix;
                        switch (sp)
                        {
                            case SetpointTypes.LL:
                                suffix = "LL";
                                break;
                            case SetpointTypes.L:
                                suffix = "L";
                                break;
                            case SetpointTypes.H:
                                suffix = "H";
                                break;
                            case SetpointTypes.HH:
                                suffix = "HH";
                                break;
                            default:
                                suffix = "???";
                                break;
                        }
                        parmsString += string.IsNullOrEmpty(parmsString)
                            ? $"{parm.SystemId}_{parm.SignalId}_{suffix} "
                            : $"| {parm.SystemId}_{parm.SignalId}_{suffix} ";
                    }
                }
            }
            var script = string.IsNullOrEmpty(parmsString)
                ? string.Empty
                : $"script=\"{parmsString}\"";
            return $"\t\t<parm name=\"UPS_{ups.Id}\" type=\"int\" {script} description=\"{ups.Group}\"/>";
        }

        public static string GetSk(NtiBase data, string sk)
        {
            var window = data.Ups.FirstOrDefault(x => x.AlarmGroup == sk).Window;
            string upsString = string.Empty;
            foreach(var ups in data.Ups.Where(x => x.AlarmGroup == sk))
            {
                upsString += string.IsNullOrEmpty(upsString)
                        ? $"UPS_{ups.Id} "
                        : $"| UPS_{ups.Id} ";
            }
            return $"\t\t<parm name=\"SK_{sk}\" type=\"int\" script=\"{upsString}\" description=\"{window}\"/>";
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
            if (param.Is420mA)
            {
                var f0String = $"\t\t<parm name=\"{paramName}_f0\" " +
                    $"type=\"alarm\" delay_on=\"3\" script=\"({paramName} &lt; -1250) || ({paramName} &gt; 11250)\"  " +
                    $"{alarmGroup}description=\"Отказ датчика {param.Description}\"/>";
                sb.AppendLine(f0String);
            }
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
