using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CodeMap.Models;
using GitLabApiClient;
using Microsoft.AspNetCore.Mvc;

namespace CodeMap.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChordMapController : ControllerBase
    {
        private readonly string token;// user's token, generated in Gitlab
        private readonly string serviceBase;
        private readonly Library lib;

        public ChordMapController(Config config)
        {
            token = config.Token;
            serviceBase = config.ServiceBase;
            lib = new Library(config);
        }

        // GET api/values/"groupname"
        [HttpGet("{groupName}")] 
        public async Task<IActionResult> GetAsync(string groupName)
        {
            try
            {
                var client = new GitLabClient(serviceBase, token); // haven't implemented IDisposable
                var group = await client.Groups.GetAsync(groupName);

                var map = new Graph() { nodes = new List<Node>(), links = new List<Link>() };
                
                foreach (var project in group.Projects)
                {
                    try
                    {
                        var packages = await lib.ProcessXmlAsync(client, project, Helper.NeedNoPublicPackages);
                        map = Helper.AddPackagesInMap(project.Name, packages, map);
                        Console.WriteLine($" Archived: {project.Name}");
                    }
                    catch (Exception e)
                    {
                        // log projects cannot be read dependency this way
                        Console.WriteLine($"Failed: {project.Name}, {e.Message}. {e.StackTrace}");
                    }
                }
                
                // Dependency in group displayed only as Chord Map
                // because force map is a mess to look at for a group of projects
                Helper.GenerateProjectsCsv(map);
                Helper.GenerateMatrixJson(map);
                
                return Ok("data loaded. see map at .../chord.html");
            }
            catch(Exception e)
            {
                return new JsonResult(e.Message + e.StackTrace);
            }
        }
    }
}
