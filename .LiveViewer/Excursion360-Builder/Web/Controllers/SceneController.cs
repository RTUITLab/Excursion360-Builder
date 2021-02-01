using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        /// <summary>
        /// Override default tour configuration
        /// </summary>
        /// <returns></returns>
        [Route("tour.json")]
        public ActionResult SceneJSON()
        {
            return Ok(new { config = "yes" });
        }
    }
}
