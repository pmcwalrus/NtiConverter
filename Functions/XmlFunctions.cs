using Nti.XlsxReader.Entities;
using Nti.XlsxReader.Types;
using NtiConverter.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;

namespace NtiConverter.Functions
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
            foreach (var device in data.Ip.Where(x => x.DeviceType == DeviceType.Worstation))
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
            sb.AppendLine(GetVk(data));
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
            sb.AppendLine("\t</shmem>");
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

        private const int AlarmOffset = 23;

        public static string GetUps(NtiBase data, UpsEntity ups)
        {
            var parmsString = string.Empty;
            var parms = data.Signals.Where(x => x.Ups == ups.Id);
            var parmsCritical = data.Signals.Where(x => !string.IsNullOrWhiteSpace(x.Ups) 
            && ((int.Parse(x.Ups) + AlarmOffset).ToString() == ups.Id) 
            && x.SetpointTypes != null
            && (x.SetpointTypes.Contains(SetpointTypes.HH) || x.SetpointTypes.Contains(SetpointTypes.LL)));
            foreach (var parm in parms)
            {
                if (parm.Type == SignalTypes.Alarm || parm.Type == SignalTypes.CritcalAlarm)
                    parmsString += string.IsNullOrEmpty(parmsString)
                        ? $"{parm.Name} "
                        : $"| {parm.Name} ";
                if (parm.Is420mA)
                    parmsString += string.IsNullOrEmpty(parmsString)
                        ? $"{parm.Name}_f0 "
                        : $"| {parm.Name}_f0 ";
                if (parm.SetpointTypes != null)
                {
                    foreach (var sp in parm.SetpointTypes)
                    {
                        string suffix;
                        switch (sp)
                        {
                            case SetpointTypes.LL:
                                continue;
                            case SetpointTypes.L:
                                suffix = "L";
                                break;
                            case SetpointTypes.H:
                                suffix = "H";
                                break;
                            case SetpointTypes.HH:
                                continue;
                            default:
                                suffix = "???";
                                break;
                        }
                        parmsString += string.IsNullOrEmpty(parmsString)
                            ? $"{parm.Name}_{suffix} "
                            : $"| {parm.Name}_{suffix} ";
                    }
                }
            }
            foreach (var parm in parmsCritical)
            {
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
                                continue;
                            case SetpointTypes.H:
                                continue;
                            case SetpointTypes.HH:
                                suffix = "HH";
                                break;
                            default:
                                continue;
                        }
                        parmsString += string.IsNullOrEmpty(parmsString)
                            ? $"{parm.Name}_{suffix} "
                            : $"| {parm.Name}_{suffix} ";
                    }
                }
            }
            var script = string.IsNullOrEmpty(parmsString)
                ? string.Empty
                : $"script=\"{parmsString}\"";
            return $"\t\t<parm name=\"UPS_{ups.Id}\" type=\"int\" {script} description=\"{ups.Group}\"/>";
        }

        public static string GetVk(NtiBase data)
        {
            var sb = new StringBuilder();
            foreach (var vk in data.Vk)
            {
                if (string.IsNullOrWhiteSpace(vk.Name)) continue;
                var parms = data.Signals.Where(x => x.Vk == vk.Number);
                var parmsString = string.Empty;
                foreach (var parm in parms)
                {
                    if (parm.Type == SignalTypes.Alarm || parm.Type == SignalTypes.CritcalAlarm)
                        parmsString += string.IsNullOrEmpty(parmsString)
                            ? $"{parm.Name} "
                            : $"| {parm.Name} ";
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
                                ? $"{parm.Name}_{suffix} "
                                : $"| {parm.Name}_{suffix} ";
                        }
                    }
                    if (parm.Is420mA)
                    {
                        parmsString += string.IsNullOrEmpty(parmsString)
                                ? $"{parm.Name}_f0 "
                                : $"| {parm.Name}_f0 ";
                    }
                }
                var script = string.IsNullOrEmpty(parmsString) ? string.Empty : $" script=\"{parmsString}\"";
                sb.AppendLine($"\t\t<parm name=\"{vk.Name}\" type=\"int\" description=\"{vk.Description}\"{script}/>");
            }
            return sb.ToString();
        }

        public static string GetSk(NtiBase data, string sk)
        {
            var window = data.Ups.FirstOrDefault(x => x.AlarmGroup == sk).Window;
            string upsString = string.Empty;
            foreach (var ups in data.Ups.Where(x => x.AlarmGroup == sk))
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
            var devices = data.Ip.Where(x => x.DeviceType == DeviceType.Device
            || x.DeviceType == DeviceType.ExternalSystem).ToList();
            foreach (var device in devices)
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
            var ascendingSignals = signals.OrderBy(x => Convert.ToInt32(x.NumberStr));
            foreach (var signal in ascendingSignals)
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
                    $"type=\"{modbusType}\" parm=\"{param.Name}\"/>");
            }
            return sb.ToString();
        }

        public static string GetShemms(NtiBase data)
        {
            var sb = new StringBuilder();
            var shmemList = GetShmemList(data);
            foreach (var shmemName in shmemList)
            {
                var shmem = data.Shmems.FirstOrDefault(x => x.Name == shmemName);
                if (shmem == null)
                {
                    MessageBox.Show($"Shmem {shmemName} не будет создан, т.к. отсутствует на листе shmems", "WARNING",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }
                sb.AppendLine(GetSingleShemem(data, shmem));
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        public static string GetSingleShemem(NtiBase data, ShmemEntity shmem)
        {
            var sb = new StringBuilder();
            var type = string.IsNullOrWhiteSpace(shmem.Type)
                ? string.Empty
                : $"type=\"{shmem.Type}\" ";
            var staleTimeout = string.IsNullOrWhiteSpace(shmem.StaleTimeout)
                ? string.Empty
                : $"stale_timeout=\"{shmem.StaleTimeout}\" ";
            var keepalive = string.IsNullOrWhiteSpace(shmem.KeepAlive)
                ? string.Empty
                : $"keepalive=\"{shmem.KeepAlive}\" ";
            var logging = string.IsNullOrWhiteSpace(shmem.Logging)
                ? string.Empty
                : $"logging=\"{shmem.Logging}\" ";
            var keepTime = string.IsNullOrWhiteSpace(shmem.KeepTime)
                ? string.Empty
                : $"keep_time=\"{shmem.KeepTime}\" ";
            var keepMb = string.IsNullOrWhiteSpace(shmem.KeepMb)
                ? string.Empty
                : $"keep_mb=\"{shmem.KeepMb}\" ";
            sb.AppendLine($"\t<shmem name=\"{shmem.Name}\" {type}{staleTimeout}{keepalive}{logging}{keepTime}{keepMb}>");
            sb.AppendLine(GetShmemParams(data, shmem.Name));
            sb.AppendLine($"\t</shmem>");
            return sb.ToString();
        }

        public static List<string> GetShmemList(NtiBase data)
        {
            var shmems = new List<string>();
            //foreach (var signal in data.Layout)
            //{
            //    var type = "???";
            //    if (signal.Type == "DI" || signal.Type == "DO") type = "DI-DO";
            //    else if (signal.Type.Contains("AI")) type = "AI";
            //    var shmem = $"nsc-{signal.DeviceIndex}-{type}";
            //    if (!shmems.Contains(shmem))
            //        shmems.Add(shmem);
            //}
            foreach (var signal in data.Signals.Where(x => !string.IsNullOrWhiteSpace(x.Shmem)))
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
            var updateTreshold = !string.IsNullOrWhiteSpace(param.UpdateTreshold)
                && !param.UpdateTreshold.ToLower().Contains("no")
                && !param.Is420mA
                ? $"update_threshold=\"{param.UpdateTreshold}\" "
                : string.Empty;
            var memchange = param.Is420mA 
                || param.UpdateTreshold.ToLower().Contains("no") 
                ? "mem_change=\"false\" " 
                : string.Empty;
            var script = !string.IsNullOrWhiteSpace(param.Script)
                ? $"script=\"{param.Script}\" "
                : string.Empty;

            var alarmString = param.Type == SignalTypes.Alarm 
                || param.Type == SignalTypes.CritcalAlarm
                ? alarmGroup
                : string.Empty;

            sb.AppendLine($"\t\t<parm name=\"{param.Name}\" " +
                $"type=\"{param.TypeString}\" {updateTreshold}{inverted}{memchange}{alarmString}{script}" +
                $"description=\"{param.Description}\"/>");


            if (param.Is420mA)
            {
                var f0String = $"\t\t<parm name=\"{param.Name}_f0\" " +
                    $"type=\"alarm\" script=\"({param.Name} &lt; -1250) || ({param.Name} &gt; 11250)\"  " +
                    $"{alarmGroup}description=\"{param.Description} Отказ датчика\"/>";
                sb.AppendLine(f0String);
            }
            if (!string.IsNullOrWhiteSpace(param.Units) && param.Is420mA) // В последних правках _u нужно создавать только для 4-20 сигналов
            {
                var updateTreshold420 = string.Empty;
                if (param.Is420mA)
                {
                    updateTreshold420 = !string.IsNullOrWhiteSpace(param.UpdateTreshold)
                        ? $"update_threshold=\"{param.UpdateTreshold}\" "
                        : $"update_threshold=\"0.1\" ";
                }
                var unitsString = $"\t\t<parm name=\"{param.Name}_u\" type=\"{param.TypeString}\" " +
                    $"script=\"alg.interpolation({param.Name}, x1_{param.Name}, y1_{param.Name})\" " +
                    $"{updateTreshold420}description=\"{param.Description} в {param.Units}\"/>";
                sb.AppendLine(unitsString);
            }
            if (!string.IsNullOrWhiteSpace(param.SetpointTypesString))
            {
                UpsEntity critical_ups = null; ;
                if (!string.IsNullOrWhiteSpace(param.Ups))
                {
                    critical_ups = data.Ups.FirstOrDefault(x => x.Id == (int.Parse(param.Ups) + AlarmOffset).ToString());
                    if (critical_ups == null)
                    {
                        throw new Exception($"Для уставок {param.Description} ID УПС для critical alarm не определен.");
                    }
                }               
                var criticalAlarmGroup = !string.IsNullOrWhiteSpace(param.Ups)
                    ? $"alarm_group=\"{critical_ups.AlarmGroup}\" "
                    : string.Empty;
                var suffix = string.Empty;
                var type = string.Empty;
                var atr = string.Empty;
                var descriptionLevel = string.Empty;
                for (var i = 0; i < param.SetpointTypes.Count; i++)
                {
                    var alarm = string.Empty;
                    switch (param.SetpointTypes[i])
                    {
                        case SetpointTypes.LL:
                            suffix = "LL";
                            type = "critical_alarm";
                            descriptionLevel = "АНУ";
                            alarm = criticalAlarmGroup;
                            atr = "&lt;";
                            break;
                        case SetpointTypes.L:
                            suffix = "L";
                            type = "alarm";
                            descriptionLevel = "НУ";
                            alarm = alarmGroup;
                            atr = "&lt;";
                            break;
                        case SetpointTypes.H:
                            suffix = "H";
                            type = "alarm";
                            descriptionLevel = "ВУ";
                            alarm = alarmGroup;
                            atr = "&gt;";
                            break;
                        case SetpointTypes.HH:
                            suffix = "HH";
                            type = "critical_alarm";
                            descriptionLevel = "АВУ";
                            alarm = criticalAlarmGroup;
                            atr = "&gt;";
                            break;
                        case SetpointTypes.Unkown:
                            suffix = "???";
                            type = "???";
                            descriptionLevel = "???";
                            alarm = "???";
                            atr = "???";
                            break;
                    }
                    var delayOn = string.Empty;
                    if (!string.IsNullOrEmpty(param.DelayTimeString))
                    {
                        var dTimeParseRes = Double.TryParse(param.DelayTimeString, out var delayTimeDouble);
                        if (!dTimeParseRes)
                            throw new AggregateException($"Can't parse delay time (Value: {param.DelayTimeString}) for parameter {param.Description}");
                        var dTimeMs = (int)(delayTimeDouble * 1000);
                        delayOn = $"delay_on=\"{dTimeMs}\" ";
                    }
                    var setpointString = $"\t\t<parm name=\"{param.Name}_{suffix}\" " +
                        $"type=\"{type}\" {delayOn}" +
                        $"script=\"{param.Name}_u {atr}={param.SetpointValues[i]}\" {alarm}" +
                        $"description=\"{param.Description} {descriptionLevel}\"/>";
                    sb.AppendLine(setpointString);
                }
            }
            return sb.ToString();
        }

        #region FormAnalyze

        public static List<FormEntity> GetFormListFromXml(string fileName)
        {
            if (fileName == null) return null;
            if (!File.Exists(fileName))
            {
                MessageBox.Show($"Файл {fileName} не найден!");
                return null;
            }
            using var sr = new StreamReader(fileName);
            var fileData = sr.ReadToEnd();
            var xmlData = XDocument.Parse(fileData);
            var parms = xmlData.Descendants("parm").ToList();
            var forms = parms.Where(x => ((string)x.Attribute("name"))
                .StartsWith("form_", StringComparison.OrdinalIgnoreCase)
                && ((string)x.Attribute("name")).Count(c => c == '_') == 1).ToList();
            var result = new List<FormEntity>();
            foreach (var form in forms)
            {
                var entity = new FormEntity
                {
                    Name = (string)form.Attribute("name"),
                    ScriptParams = GetScriptList(form),
                };
                result.Add(entity);
            }
            return result;
        }

        #endregion

        #region XML Analyze

        public static string FindDoubleParams(string fileName)
        {
            using var sr = new StreamReader(fileName);
            var fileData = sr.ReadToEnd();
            var xmlData = XDocument.Parse(fileData);
            var sb = new StringBuilder();
            var parms = xmlData.Descendants("parm").ToList();
            var doubleParams = new List<string>();
            foreach (var parm in parms)
            {
                var name = (string)parm.Attribute("name");
                if (doubleParams.Contains(name)) continue;
                var count = parms.Count(x => (string)x.Attribute("name") == name);
                if (count <= 1) continue;
                doubleParams.Add(name);
                sb.AppendLine($"{name}: {count}");
            }
            return sb.ToString();
        }

        public static string AnalyzeXml(string fileName)
        {
            using var sr = new StreamReader(fileName);
            var fileData = sr.ReadToEnd();
            var xmlData = XDocument.Parse(fileData);
            var sb = new StringBuilder();
            var parms = xmlData.Descendants("parm").ToList();
            var alarms = parms.Where(
                x => (string)x.Attribute("type") == "alarm"
                || (string)x.Attribute("type") == "critical_alarm").ToList();
            var ups = parms.Where(x => ((string)x.Attribute("name")).StartsWith("UPS_", StringComparison.OrdinalIgnoreCase)
                && ((string)x.Attribute("name")).Count(c => c == '_') == 1).ToList();
            var upsWithScripts = ups.Where(x => !string.IsNullOrWhiteSpace((string)x.Attribute("script"))).ToList();
            var forms = parms.Where(x => ((string)x.Attribute("name")).StartsWith("form_", StringComparison.OrdinalIgnoreCase)
                && ((string)x.Attribute("name")).Count(c => c == '_') == 1).ToList();
            var formsWithScripts = forms.Where(x => !string.IsNullOrWhiteSpace((string)x.Attribute("script"))).ToList();

            var alarmsWithWrongGourp = GetAlarmsWithWrongGroup(alarms);
            if (alarmsWithWrongGourp.Count > 0)
            {
                foreach (var a in alarmsWithWrongGourp)
                    sb.AppendLine($"Alarm {(string)a.Element.Attribute("name")} входит в неверную группу ({a.Group})!");
                sb.AppendLine();
            }

            var cAlarmsWithWrongGourp = GetCriticalAlarmsWithWrongGroup(alarms);
            if (cAlarmsWithWrongGourp.Count > 0)
            {
                foreach (var a in cAlarmsWithWrongGourp)
                    sb.AppendLine($"Critical alarm {(string)a.Element.Attribute("name")} входит в неверную группу ({a.Group})!");
                sb.AppendLine();
            }

            var alarmsWithoutUps = GetAlarmsWithoutScripts(alarms, upsWithScripts);
            if (alarmsWithoutUps.Count > 0)
            {
                foreach (var a in alarmsWithoutUps)
                    sb.AppendLine($"Alarm {(string)a.Attribute("name")} не входит ни в один UPS");
                sb.AppendLine();
            }

            var alarmsWithoutForms = GetAlarmsWithoutScripts(alarms, formsWithScripts);
            if (alarmsWithoutForms.Count > 0)
            {
                foreach (var a in alarmsWithoutForms)
                    sb.AppendLine($"Alarm {(string)a.Attribute("name")} не входит ни в один Form!");
                sb.AppendLine();
            }

            foreach (var u in upsWithScripts)
                sb.Append(FindNotAlarmScripts(parms, u));

            foreach (var u in formsWithScripts)
                sb.Append(FindNotAlarmScripts(parms, u));

            return sb.ToString();
        }

        public static List<(XElement Element, string Group)> GetAlarmsWithWrongGroup(List<XElement> allAlarms)
        {
            var alarms = allAlarms.Where(x => (string)x.Attribute("type") == "alarm");
            var result = new List<(XElement, string)>();
            foreach (var alarm in alarms)
            {
                var group = (string)alarm.Attribute("alarm_group");
                if (group == "4" || group == "5" || group == "7") continue;
                result.Add((alarm, group));
            }
            return result;
        }

        public static List<(XElement Element, string Group)> GetCriticalAlarmsWithWrongGroup(List<XElement> allAlarms)
        {
            var alarms = allAlarms.Where(x => (string)x.Attribute("type") == "critical_alarm");
            var result = new List<(XElement, string)>();
            foreach (var alarm in alarms)
            {
                var group = (string)alarm.Attribute("alarm_group");
                if (group == "1" || group == "2" || group == "3") continue;
                result.Add((alarm, group));
            }
            return result;
        }

        public static List<XElement> GetAlarmsWithoutScripts(List<XElement> alarms, List<XElement> parmsWithScripts)
        {
            var alarmsInScripts = new List<string>();
            foreach (var parm in parmsWithScripts)
                alarmsInScripts.AddRange(GetScriptList(parm));
            return alarms.Where(x => !alarmsInScripts.Contains((string)x.Attribute("name"))).ToList();
        }

        public static string FindNotAlarmScripts(List<XElement> allParms, XElement element)
        {
            var sb = new StringBuilder();
            var alarmsInScripts = GetScriptList(element);
            foreach (var a in alarmsInScripts)
            {
                var parm = allParms.FirstOrDefault(x => (string)x.Attribute("name") == a);
                if (parm == null)
                {
                    sb.AppendLine($"В скрипте {(string)element.Attribute("name")} используется сигнал {a}, который не объявлен!");
                    continue;
                }
                var parmType = (string)parm.Attribute("type");
                if (parmType != "alarm" && parmType != "critical_alarm")
                    sb.AppendLine($"В скрипте {(string)element.Attribute("name")} используется сигнал {a} типа {parmType}");
            }
            return sb.ToString();
        }

        public static List<string> GetScriptList(XElement element)
        {
            var alarmsInScripts = new List<string>();
            var script = (string)element.Attribute("script");
            if (string.IsNullOrWhiteSpace(script)) return null;
            alarmsInScripts.AddRange(script.Replace(" ", string.Empty)
                .Split('|', StringSplitOptions.RemoveEmptyEntries));
            return alarmsInScripts;
        }

        #endregion
    }
}
