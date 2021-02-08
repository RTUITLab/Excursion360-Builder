using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Models
{
    public class PreviewServerVersion
    {
        public string Version { get; set; }
        public DateTimeOffset? ReleaseDate { get; set; }
    }
}
