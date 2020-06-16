using CodeMap.Models;
using GitLabApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CodeMapTests")]
namespace CodeMap
{
    public partial class Library
    {
        // Purpose: find public packages used in github projects (.net framework/.net core)
        // => to generate KnownPublicPackages
        public async Task PrintAllPackagesAsync()
        {
            var client = new GitLabClient(serviceBase, token);

            var searchResult = new List<string>();

            foreach (string groupName in gitlabGroups)
            {
                var group = await client.Groups.GetAsync(groupName);
                foreach (var project in group.Projects)
                {
                    try
                    {
                        var packages = await ProcessXmlAsync(client, project, Helper.NeedAllPackages);
                        AddPackages(searchResult, packages);
                        Console.WriteLine($" Checked: {project.Name}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to get csproj: {project.Name}, {e.Message}");
                    }
                }
            }

            Console.WriteLine("Final Result: ");
            foreach (string r in searchResult.OrderBy(r=>r))
                Console.WriteLine(r);
        }



        internal void AddPackages(List<string> searchResult, List<PackageReference> packages)
        {
            foreach(PackageReference package in packages)
            {
                if (IsNewPackage(searchResult, package))
                    searchResult.Add(package.Include);
            }
        }

        internal bool IsNewPackage(List<string> searchResult, PackageReference package)
        {
            var result = searchResult.Find(r => r == package.Include);
            return result is null;
        }      
    }
}
