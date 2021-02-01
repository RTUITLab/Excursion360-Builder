using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Web.Models.Options;

namespace Web.Controllers
{
    [ApiController]
    public class SceneController : ControllerBase
    {
        /// <summary>
        /// Override default configuration
        /// </summary>
        /// <returns></returns>
        [Route("config.json")]
        public ActionResult ConfigJSON()
        {
            return Ok(new { sceneUrl = "" });
        }

        [Route("Assets/{*asset}")]
        public ActionResult GetTexture(
            [FromServices] IOptions<StartupOptions> options,
            string asset)
        {
            var filePath = Path.Combine(options.Value.AssetsPath, asset?.TrimStart());
            if (System.IO.File.Exists(filePath))
            {
                return new PhysicalFileResult(filePath, $"image/{Path.GetExtension(filePath).TrimStart('.')}");
            }
            return NotFound($"Can't locate file {asset}");
        }
    }
}
