using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.Models;
using Web.Models.Options;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/system")]
    public class SystemInfoController : ControllerBase
    {
        private readonly IOptions<StartupOptions> startupOptions;
        private readonly IOptions<PreviewServerVersion> versionOptions;

        public SystemInfoController(
            IOptions<StartupOptions> startupOptions,
            IOptions<PreviewServerVersion> versionOptions)
        {
            this.startupOptions = startupOptions;
            this.versionOptions = versionOptions;
        }
        public ActionResult Options()
        {
            return Ok(new
            {
                startup = startupOptions.Value,
                version  = versionOptions.Value
            });
        }
    }
}
