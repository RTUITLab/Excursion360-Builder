using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.Models.Options;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/system")]
    public class SystemInfoController : ControllerBase
    {
        private readonly IOptions<StartupOptions> options;

        public SystemInfoController(IOptions<StartupOptions> options)
        {
            this.options = options;
        }
        public ActionResult<StartupOptions> Options()
        {
            return options.Value;
        }
    }
}
