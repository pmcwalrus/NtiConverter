using Nti.XlsxReader.Types;
using System.IO;
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
            using (var sw = new StreamWriter(fileName) { AutoFlush = true })
            {
                sw.Write(xmlData);
            }
        }

        private static string GetFileData(NtiBase data)
        {
            var sb = new StringBuilder();
            sb.Append(data.XmlTop);
            sb.Append(data.XmlBot);
            throw new NotImplementedException();
            return sb.ToString();
        }
    }
}
