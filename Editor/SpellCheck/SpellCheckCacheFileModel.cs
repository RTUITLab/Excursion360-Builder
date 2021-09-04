using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages.Excursion360_Builder.Editor.SpellCheck
{
    [Serializable]
    public class SpellCheckCacheFileModel
    {
        public ApiUsage[] apiUsageHistory;
        public string[] exceptions;
        public SpellCheckPair[] spellCheckPairs;
    }

    [Serializable]
    public class SpellCheckPair
    {
        public string word;
        public RowResponse[] result;
    }
    [Serializable]
    public class ApiUsage
    {
        public string date;
        public int api;
        public int cache;
    }
}
