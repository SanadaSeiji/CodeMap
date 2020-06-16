using System;
using CodeMap.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeMap.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForceMapController : ControllerBase
    {
        private readonly Aws s3;

        public ForceMapController(Config config)
        {
            s3 = new Aws(config);
        }
        [HttpGet("{projectName}")]
        public ActionResult Get(string projectName)
        {
            try
            {
                string datasourcepath = Helper.GetDatasetPath("projectsRelationships.json");
                s3.DownloadFile(datasourcepath, $"force/{projectName}.json");
                return File("~/force.html", "text/html");
            }
            catch(Exception e)
            {
                return new JsonResult(e.Message+e.StackTrace);
            }
        }
    }
}