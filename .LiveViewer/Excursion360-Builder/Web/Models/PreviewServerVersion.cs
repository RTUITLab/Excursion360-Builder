using System;

namespace Web.Models;

public class PreviewServerVersion
{
    public string Version { get; set; }
    public DateTimeOffset? ReleaseDate { get; set; }
}
