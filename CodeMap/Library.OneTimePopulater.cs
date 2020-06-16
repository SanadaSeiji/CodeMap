using CodeMap.Models;
using GitLabApiClient;
using System;
using System.Threading.Tasks;

namespace CodeMap
{
    public partial class Library
    {
        public async Task PopulateDynamoDBAsync()
        {
            var map = new Graph();
            var client = new GitLabClient(serviceBase, token); // haven't implemented IDisposable

            foreach (string group in gitlabGroups)
            {
                map = await GetOneGroupMapAsync(group, map, client);
            }

            await MapToDynamoTableAsync(map);
        }

        public async Task<Graph> GetOneGroupMapAsync(string groupName, Graph map, GitLabClient client)
        {          
            var group = await client.Groups.GetAsync(groupName);
            foreach (var project in group.Projects)
            {              
                if (project.Name == "HelloAPI")
                    continue; //template project shouldn't be counted
                
                try
                {                  
                    var packages = await ProcessXmlAsync(client, project, Helper.NeedNoPublicPackages);
                    map = Helper.AddPackagesInMap(project.Name, packages, map);
                    Console.WriteLine($" Archived: {project.Name}");
                }
                catch (Exception e)
                {
                    // log projects cannot be read dependency this way
                    Console.WriteLine($"Failed: {project.Name}, {e.Message}");
                }            
            }

            return map;
        }

        public async Task MapToDynamoTableAsync(Graph map)
        {
            foreach (Link link in map.links)
            {
                await db.PutRecordAsync(link.source, link.target);
                // if one fails, throw exception, delete all in table, start over
            }
        }
    }
}
