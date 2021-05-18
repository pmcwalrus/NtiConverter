using NtiConverter.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace NtiConverter.Functions
{
    internal static class FormFunctions
    {
        public static string CheckForms(List<FormEntity> entities)
        {
            var sb = new StringBuilder();
            foreach (var entity in entities)
            {
                foreach (var file in entity.FilesToCheck)
                {
                    var notUsed = GetUnusedParmsInForm(file, entity.ScriptParams);
                    if (notUsed == null || notUsed.Count == 0) continue;
                    sb.AppendLine($"Не найдены параметры для {entity.Name} на форме {file.Split('\\').Last()}:");
                    foreach (var parm in notUsed)
                        sb.AppendLine(parm);
                }
            }
            return sb.ToString();
        }

        public static List<string> GetUnusedParmsInForm(string formName, List<string> scriptElements)
        {
            if (!File.Exists(formName))
            {
                MessageBox.Show($"Не найден файл формы:\r\n{formName}",
                    "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            if (scriptElements == null)
                return null;
            using var sr = new StreamReader(formName);
            var fileData = sr.ReadToEnd();
            var result = new List<string>();
            foreach (var script in scriptElements)
            {
                if (!fileData.Contains(script))
                    result.Add(script); ;
            }
            return result;
        }
    }
}
