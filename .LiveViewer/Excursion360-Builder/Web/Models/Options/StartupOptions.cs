using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Models.Options
{
    public class StartupOptions
    {
        public string AssetsPath { get; set; }
        public int ParentProcessId { get; set; }
    }
}
