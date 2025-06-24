using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Web.Hubs;
using Web.Models.Options;

namespace Web.Controllers;

[ApiController]
public class SceneController : ControllerBase
{
    private readonly ILogger<SceneController> logger;

    public SceneController(ILogger<SceneController> logger)
    {
        this.logger = logger;
    }
    /// <summary>
    /// Override default configuration
    /// </summary>
    /// <returns></returns>
    [Route("config.json")]
    public ActionResult ConfigJSON()
    {
        return Ok(new { sceneUrl = "", minFOV = 0.1, maxFOV = 3 });
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

    /// <summary>
    /// Add additional scripts to page
    /// </summary>
    /// <param name="environment"></param>
    /// <returns></returns>
    [Route("index.html")]
    public ActionResult IndexHtml(
        [FromServices] IHostEnvironment environment)
    {
        var indexFile = Path.Combine(environment.ContentRootPath, "wwwroot", "index.html");
        if (!System.IO.File.Exists(indexFile))
        {
            return NotFound("not found index.html");
        }
        var indexContent = System.IO.File.ReadAllText(indexFile);
        indexContent = indexContent.Replace("</head>",
            "\n<script src=\"js/signalr/dist/browser/signalr.js\"></script>" +
            "\n<script src=\"js/interop.js\"></script>" +
            "\n</head>");
        return File(Encoding.UTF8.GetBytes(indexContent), "text/html");
    }

    /// <summary>
    /// Open state
    /// </summary>
    /// <returns></returns>
    [HttpPost("api/interop/openTour")]
    public async Task<ActionResult> OpenTourAction(
        [FromServices] IHubContext<InteropHub> interopHub,
        [FromBody] JsonElement openTourData)
    {
        var tourJson = JsonSerializer.Serialize(openTourData);
        await interopHub.Clients.All.SendAsync("OpenTour", tourJson);
        return Ok(tourJson);
    }
}
