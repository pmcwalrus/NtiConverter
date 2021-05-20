using Nti.XlsxReader.Entities;
using Nti.XlsxReader.Types;
using System.Collections.Generic;
using System.Linq;

namespace NtiConverter.Functions
{
    internal static class XlsxFunctions
    {
        public static List<SignalEntity> CheckLayout(NtiBase data)
        {
            return data.Signals.Where(x => !string.IsNullOrWhiteSpace(x.Index) 
            && !data.Layout.Any(z => z.SignalName == x.Index)).ToList();
        }

        public static List<SignalOnDevice> CheckSignalList(NtiBase data)
        {
            return data.Layout.Where(x => !data.Signals.Any(z => z.Index == x.SignalName)).ToList();
        }
    }
}
