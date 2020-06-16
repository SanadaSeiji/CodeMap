using CodeMap.Models;
using GitLabApiClient;
using GitLabApiClient.Models.Projects.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CodeMap
{
    public static partial class Helper
    {
        public static async Task<string> GetXmlFromProject(GitLabClient client, Project project)
        {
            var response = await client.Files.GetAsync(project, filePath: $"{project.Name}/{project.Name}.csproj", reference: project.DefaultBranch);
            return GetCspFromClientAPI(response);
        }

        public static string GetCspFromClientAPI(GitLabApiClient.Models.Files.Responses.File response)
        {
            var cspXml = response.ContentDecoded;
            // original GitLab Api has no such problem
            // using nuget GitLabApi, returned .csproj file sometimes contains one unknown char before <
            // which makes them invalid xml
            if (cspXml[0] != '<')
                cspXml = cspXml.Remove(0, 1);
            return cspXml;
        }

        public static Func<PackageReference, bool> NeedNoPublicPackages = pak => IsGitPackage(pak);

        public static Func<PackageReference, bool> NeedAllPackages = pak => true; // to be the same type as NeedNoPublicPackages


        public static PackageReference GetPackageFromCoreElement(XElement ele, Func<PackageReference, bool> PackageFilterRule)
        {
            string include = "";
            string version = "";

            try
            {
                foreach (XAttribute attr in ele.Attributes())
                {
                    if (attr.Name == "Include") include = attr.Value;
                    if (attr.Name == "Version") version = attr.Value;
                }
                var pak = new PackageReference()
                {
                    Include = include,
                    Version = version
                };
                if (PackageFilterRule(pak))
                    return pak;
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"failed to get package from include={include}, {e.Message}, {e.StackTrace}");
                // if (ever) failed to set packageReference?
                // do not fail for entire project, simply move on to next element for next package  
                return null;
            }
        }

        public static PackageReference GetPackageFromFrameworkElement(XElement ele, Func<PackageReference, bool> PackageFilterRule)
        {
            string msg = "";
            foreach (XAttribute attr in ele.Attributes())
            {
                if (attr.Name == "Include") msg = attr.Value;
            }
            var pak = new PackageReference();
            try
            {
                SetPackageNameFromFrameworkMessage(pak, msg);
                if (PackageFilterRule(pak))
                    return pak;
                else return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"failed to get package from include={msg}, {e.Message}, {e.StackTrace}");
                // when failed to set package e.g.: <Reference include="System" />
                // do not fail for entire project, simply move on to next package
                return null;
            }
        }

        public static void AddInPackageListFromElement(List<PackageReference> packageList, XElement ele, 
            Func<XElement, Func<PackageReference, bool>, PackageReference> GetPackageFromElement,
            Func<PackageReference, bool>PackageFilterRule)
        {
            var newPak = GetPackageFromElement(ele, PackageFilterRule);
            if (!(newPak is null))
            {
                packageList.Add(newPak);
            }
        }
   
        public static List<PackageReference> GetPackagesFromXml(string cspXml, Func<PackageReference, bool> PackageFilterRule)
        {
            var xnodes = GetXNodes(cspXml);       
            return GetPackagesFromXNodes(xnodes, PackageFilterRule);
        }

        public static List<PackageReference> GetPackagesFromXNodes(IEnumerable<XNode> xnodes, Func<PackageReference, bool> PackageFilterRule)
        {
            var packageList = new List<PackageReference>();
            foreach (XNode node in xnodes)
            {
                if (node is XElement)
                {
                    XElement ele = node as XElement;
                    if (ele.Name == "PackageReference") // for .Net Core
                    {
                        AddInPackageListFromElement(packageList, ele, GetPackageFromCoreElement, PackageFilterRule);
                    }
                    if (ele.Name.LocalName == "Reference")
                    {
                        AddInPackageListFromElement(packageList, ele, GetPackageFromFrameworkElement, PackageFilterRule);
                    }
                }
            }
            return packageList;
        }

        public static IEnumerable<XNode> GetXNodes(string cspXml)
        {
            TextReader tr = new StringReader(cspXml);
            XDocument doc = XDocument.Load(tr);
            var xnodes= from node in doc.DescendantNodes()
                                      select node;
            return xnodes;
        }

        public static void SetPackageNameFromFrameworkMessage(PackageReference pak, string msg)
        {
            string[] msgs = msg.Split(',');
            pak.Include = msgs[0];
            int start;
            string versionMark = "Version=";
            if (msgs[1].Contains(versionMark))
            {
                start = msgs[1].IndexOf(versionMark)+ versionMark.Length;
                pak.Version = msgs[1].Substring(start, msgs[1].Length - start);
            }
        }

        public static bool IsGitPackage(PackageReference pak)
        {
            if (IsKnownPublicPackage(pak.Include))
                return false;       
            // for self generated package, their nuget's version number different from public one
            // public: 2.2.0, or 6.0.0.0
            // self generated: 1.0.0.1908201259
            string[] numbers = pak.Version.Split('.');
            if (numbers.Length < 4)
                return false;

            string last = numbers[3];
            if (last.Length >= 5) 
                return true; // ??? newly generated nuget always has 10 bit at last number; SendMail has 5...
            else 
                return false;        
        }

        public static bool IsKnownPublicPackage(string name)
        {
            // this should pass from outside config
            var publicPakSuffixs = new List<string>() { "Amazon", "AspNetCore", "Autofac", "AWS", "AWSSDK", "Azure",
                "BouncyCastle", "DocumentFormat", "DotNetZip", "EntityFramework", "Esent", "FluentValidation", "Functional", "GitLabApiClient",
                "Hammock", "HtmlAgilityPack", "JetBrains", "MailKit", "MediatR", "MicroElements", "Microsoft", "MimeKit","Moq", "MySql",
                 "Newtonsoft","NLog", "NuGet", "Optional", "Oracle", "Pomelo", "Postmark","prometheus-net","RabbitMQ", "restsharp", "Restsharp",
                 "RouteMagic","runtime","sentry", "Sentry", "ServiceStack", "Specifications", "SSH",  "Swashbuckle","System", "WebActivatorEx" };
            foreach (string suffix in publicPakSuffixs)
            {
                if (name.StartsWith(suffix))
                    return true;
            }
            return false;
        }
    }
}
