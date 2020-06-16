using CodeMap.Models;
using GitLabApiClient;
using GitLabApiClient.Models.Projects.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeMap
{
    //Purpose: called in MapUpdate, which should be called from ci whenever a project been pushed
    public partial class Library
    {
        private readonly string token; // user's token, generated in Gitlab
        private readonly string serviceBase;

        private readonly Aws db;

        private readonly List<string> gitlabGroups;

        public Library(Config config)
        {
            token = config.Token;
            serviceBase = config.ServiceBase;
            db = new Aws(config);
            gitlabGroups = config.GitlabGroups;
        }

        public async Task UpdateCmdAsync(string groupName, string projectName)
        {
            // After pushed in Github, MapaUpadte called by ci 
            // as last step, to get project from GitLab
            var newlyPushed = await GetDependencyFromCsprojOnGitlab(groupName, projectName);
            await UpdateDBAsync(projectName, newlyPushed);
        }

        public async Task<List<string>> GetDependencyFromCsprojOnGitlab(string groupName, string projectName)
        {         
            var client = new GitLabClient(serviceBase, token); // No IDisposable implemented
            var group = await client.Groups.GetAsync(groupName);
            var packages = new List<PackageReference>();
            foreach (var project in group.Projects)
            {
                if (project.Name == projectName)
                {
                    packages = await ProcessXmlAsync(client, project, Helper.NeedNoPublicPackages);
                    return packages.Select(pak => pak.Include).ToList();
                }
            }
            return new List<string>(); // when found no dependency, return empty list
        }

        public async Task<List<PackageReference>> ProcessXmlAsync(GitLabClient client, Project project,
            Func<PackageReference, bool> packageFilterRule)
        {
            if (client is null) throw new ArgumentNullException("GitLabClient is null");
            if (project is null) throw new ArgumentNullException("Project is null");

            var cspXml = await Helper.GetXmlFromProject(client, project); //from .csproj
            return Helper.GetPackagesFromXml(cspXml, packageFilterRule);
        }

        // when a new project pushed
        // update its dependency
        // means using it as target, update source -> target
        public async Task UpdateDBAsync(string target, List<string> newlyPushed)
        {
            List<DBRecord> existedObj = await db.GetRecordsAsync("TargetProject", target);
            // if one record updating fails, entire action fails and throw exception
            await AddNewRecordsAsync(existedObj, target, newlyPushed);
            await DeleteBrokenRecordsAsync(existedObj, newlyPushed);
        }

        public async Task AddNewRecordsAsync(List<DBRecord> existedObj, string target, List<string> newlyPushed)
        {
            List<string> toAdd = GetRecordsNeedToAdd(existedObj, newlyPushed);
            foreach (string source in toAdd)
            {
                await db.PutRecordAsync(source, target);
                Console.WriteLine($"Added: {source} -> {target}");
            }
        }

        public async Task DeleteBrokenRecordsAsync(List<DBRecord> existedObj, List<string> newlyPushed)
        {
            List<DBRecord> toDelete = GetRecordsNeedToDelete(existedObj, newlyPushed);
            foreach (DBRecord obj in toDelete)
            {
                await db.DeleteRecordAsync(obj.Id);
                Console.WriteLine($"deleted: {obj.Source} -> {obj.Target}");
            }
        }

        public List<string> GetRecordsNeedToAdd(List<DBRecord> existedObj, List<string> newlyPushed)
        {
            var existed = existedObj.Select(obj => obj.Source);
            var result = new List<string>();
            foreach (string source in newlyPushed)
            {
                if (!existed.Contains(source))
                    result.Add(source);
            }
            return result;
        }

        public List<DBRecord> GetRecordsNeedToDelete(List<DBRecord> existedObj, List<string> newlyPushed)
        {
            var result = new List<DBRecord>();
            foreach (DBRecord obj in existedObj)
            {
                if (!newlyPushed.Contains(obj.Source))
                    result.Add(obj);
            }
            return result;
        }
    }
}
