using CodeMap;
using CodeMap.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CodeMapTests
{
    [TestClass]
    public partial class HelperTests
    {
        private readonly static List<Node> Nodes = new List<Node>() { new Node() { id = "CProject" },
                                                                      new Node() { id = "AProject" } };
        private readonly static List<Link> Links = new List<Link>() { new Link() { source = "AProject", target = "CProject" } };
        private readonly Graph map = new Graph(){ nodes = Nodes,links = Links };

        [TestMethod]
        public void Core_CspToXml_GetPackages()
        {
            string cspOurPakInFrontOfPublicPak = "<Project Sdk=\"Microsoft.NET.Sdk.Web\">" +
                  "<PropertyGroup>" +
                    "<TargetFramework> netcoreapp2.2 </TargetFramework> " +
                     "</PropertyGroup>" +
                    "<ItemGroup>" +
                        "<PackageReference Include=\"AProject\" Version = \"1.0.0.1910151602\" />" +
                        "<PackageReference Include=\"Microsoft.Extension\" Version = \"1.0.0.8\" />" +
                    "</ItemGroup>" +
                "</Project>";
            var resultList = Helper.GetPackagesFromXml(cspOurPakInFrontOfPublicPak, Helper.NeedNoPublicPackages);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual("AProject", resultList[0].Include);

            var resultStringList = Helper.GetPackagesFromXml(cspOurPakInFrontOfPublicPak, Helper.NeedAllPackages).Select(pak => pak.Include).ToList();
            var expected = new List<string>() { "AProject", "Microsoft.Extension" };
            Assert.AreEqual(expected.Count, resultStringList.Count);
            Assert.IsTrue(resultStringList.All(expected.Contains));

            string cspOurPakBehindPublicPak = "<Project Sdk=\"Microsoft.NET.Sdk.Web\">" +
                  "<PropertyGroup>" +
                    "<TargetFramework> netcoreapp2.2 </TargetFramework> " +
                     "</PropertyGroup>" +
                    "<ItemGroup>" +
                        "<PackageReference Include=\"Microsoft.Extension\" Version = \"1.0.0.8\" />" +
                        "<PackageReference Include=\"AProject\" Version = \"1.0.0.1910151602\" />" +                
                    "</ItemGroup>" +
                "</Project>";
            resultList = Helper.GetPackagesFromXml(cspOurPakBehindPublicPak, Helper.NeedNoPublicPackages);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual("AProject", resultList[0].Include);

            resultStringList = Helper.GetPackagesFromXml(cspOurPakBehindPublicPak, Helper.NeedAllPackages).Select(pak => pak.Include).ToList();
            expected = new List<string>() { "AProject", "Microsoft.Extension" };
            Assert.AreEqual(expected.Count, resultStringList.Count);
            Assert.IsTrue(resultStringList.All(expected.Contains));
        }

        [TestMethod]
        public void Framework_CspToXml_GetPackages()
        {
            string csp = "<Project Sdk=\"Microsoft.NET.Sdk.Web\">" +
                    "<ItemGroup>" +
                        "<Reference Include = \"AProject, Version=2.3.50.1911061721, Culture=neutral, PublicKeyToken=1234567890, processorArchitecture=MSIL\">" +
                        "</Reference>"+
                    "</ItemGroup>"+
                "</Project>";
            var resultList = Helper.GetPackagesFromXml(csp, Helper.NeedNoPublicPackages);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual("AProject", resultList[0].Include);

            string cspAmongUnreadablePak = "<Project Sdk=\"Microsoft.NET.Sdk.Web\">" +
                   "<ItemGroup>" +
                   "<Reference Include = \"EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=1234567890, processorArchitecture=MSIL\" > " +
                       @"<HintPath> ..\packages\AWSSDK.2.3.50.1\lib\net35\AWSSDK.dll </HintPath>" +
                       "<Private> True </Private>" +
                       "</Reference>" +
                       "<Reference Include = \"SendMail, Version=1.0.6682.23301, Culture=neutral, processorArchitecture=MSIL\">" +
                       @"<HintPath> ..\packages\AWSSDK.2.3.50.1\lib\net35\AWSSDK.dll </HintPath>" +
                       "<Private> True </Private>" +
                       "</Reference>" +
                       "<Reference Include = \"System\"/>" +
                   "</ItemGroup>" +
               "</Project>";
            resultList = Helper.GetPackagesFromXml(cspAmongUnreadablePak, Helper.NeedNoPublicPackages);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual("SendMail", resultList[0].Include);
        }


        [TestMethod]
        public void GivenReferenceFromFramework_GetPackageInfo_UnitTest()
        {
            const string msg = "AProject, Version=2.3.50.125656, Culture=neutral, PublicKeyToken=1234567890, processorArchitecture=MSIL";
            var pak = new PackageReference();
            Helper.SetPackageNameFromFrameworkMessage(pak, msg);
            Assert.AreEqual("AProject", pak.Include);
            Assert.AreEqual("2.3.50.125656", pak.Version);
        }

        [TestMethod]
        public void Core_CspXml_GetXNode_AsXElement_ForPackage()
        {
            string csp = "<Project Sdk=\"Microsoft.NET.Sdk.Web\">" +
                  "<PropertyGroup>" +
                    "<TargetFramework> netcoreapp2.2 </TargetFramework> " +
                     "</PropertyGroup>" +
                    "<ItemGroup>" +
                        "<PackageReference Include=\"AProject\" Version = \"1.0.0.1910151602\" />" +
                        "<PackageReference Include=\"Microsoft.Extension\" Version = \"1.0.0.8\" />" +
                    "</ItemGroup>" +
                "</Project>";
            var xnodes = Helper.GetXNodes(csp).ToList();
            bool result = false;
            foreach (XNode node in xnodes)
            {
                if (node is XElement)
                {
                    XElement ele = node as XElement;
                    Assert.IsNotNull(ele.Name);
                    if(ele.Name == "PackageReference")
                    {
                        result = true;
                        break;
                    }
                }
            }
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Framework_CspXml_GetXNode_AsXElement_ForPackages()
        {
            string csp = "<Project Sdk=\"Microsoft.NET.Sdk.Web\">" +
                    "<ItemGroup>" +
                        "<Reference Include = \"AProject, Version=2.3.50.1911061721, Culture=neutral, PublicKeyToken=1234567890, processorArchitecture=MSIL\">" +
                        "</Reference>" +
                    "</ItemGroup>" +
                "</Project>";
            var xnodes = Helper.GetXNodes(csp).ToList();
            bool result = false;
            foreach (XNode node in xnodes)
            {
                if (node is XElement)
                {
                    XElement ele = node as XElement;
                    Assert.IsNotNull(ele.Name);
                    if (ele.Name.LocalName == "Reference")
                    {
                        result = true;
                        break;
                    }
                }
            }
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Core_GetPakNameFromXelement()
        {
            string csp = "<Project Sdk=\"Microsoft.NET.Sdk.Web\">" +
                 "<PropertyGroup>" +
                   "<TargetFramework> netcoreapp2.2 </TargetFramework> " +
                    "</PropertyGroup>" +
                   "<ItemGroup>" +
                       "<PackageReference Include=\"AProject\" Version = \"1.0.0.1910151602\" />" +
                       "<PackageReference Include=\"Microsoft.Extension\" Version = \"1.0.0.8\" />" +
                   "</ItemGroup>" +
               "</Project>";
            var resultOurPak = new PackageReference();

            var xnodes = Helper.GetXNodes(csp).ToList();
            foreach (XNode node in xnodes)
            {
                if (node is XElement)
                {
                    XElement ele = node as XElement;
                    if (ele.Name == "PackageReference")
                    {
                        resultOurPak = Helper.GetPackageFromCoreElement(ele, Helper.NeedNoPublicPackages);
                        break;
                    }
                }
            }
            Assert.IsTrue(resultOurPak.Include == "AProject");
            Assert.IsTrue(resultOurPak.Version == "1.0.0.1910151602");

            var resultAllPak = new List<PackageReference>();
            foreach (XNode node in xnodes)
            {
                if (node is XElement) 
                {
                    XElement ele = node as XElement;
                    if (ele.Name == "PackageReference")
                    {
                        resultAllPak.Add(Helper.GetPackageFromCoreElement(ele, Helper.NeedAllPackages));
                    }
                }
            }
            Assert.IsTrue(resultAllPak.Count == 2);
            Assert.IsTrue(resultAllPak.Where(pak => (pak.Include == "AProject")&&(pak.Version == "1.0.0.1910151602")).ToList().Count == 1);
            Assert.IsTrue(resultAllPak.Where(pak => (pak.Include == "Microsoft.Extension") && (pak.Version == "1.0.0.8")).ToList().Count == 1);
        }

        [TestMethod]
        public void Framework_GetPakNameFromXelement()
        {
            string csp = "<Project Sdk=\"Microsoft.NET.Sdk.Web\">" +
                     "<ItemGroup>" +
                         "<Reference Include = \"AProject, Version=2.3.50.1911061721, Culture=neutral, PublicKeyToken=1234567890, processorArchitecture=MSIL\">" +
                         "</Reference>" +
                         "<Reference Include = \"Microsoft.Extension, Version=1.0.0.8, Culture=neutral, PublicKeyToken=1234567890, processorArchitecture=MSIL\">" +
                         "</Reference>" +
                     "</ItemGroup>" +
                 "</Project>";
            var resultOurPak = new PackageReference();

            var xnodes = Helper.GetXNodes(csp).ToList();
            foreach (XNode node in xnodes)
            {
                if (node is XElement)
                {
                    XElement ele = node as XElement;
                    if (ele.Name.LocalName == "Reference")
                    {
                        resultOurPak = Helper.GetPackageFromFrameworkElement(ele, Helper.NeedNoPublicPackages);
                        break;
                    }
                }
            }
            Assert.IsTrue(resultOurPak.Include == "AProject");
            Assert.IsTrue(resultOurPak.Version == "2.3.50.1911061721");

            var resultAllPak = new List<PackageReference>();
            foreach (XNode node in xnodes)
            {
                if (node is XElement)
                {
                    XElement ele = node as XElement;
                    if (ele.Name.LocalName == "Reference")
                    {
                        resultAllPak.Add(Helper.GetPackageFromFrameworkElement(ele, Helper.NeedAllPackages));
                    }
                }
            }
            Assert.IsTrue(resultAllPak.Count == 2);
            Assert.IsTrue(resultAllPak.Where(pak => (pak.Include == "AProject") && (pak.Version == "2.3.50.1911061721")).ToList().Count == 1);
            Assert.IsTrue(resultAllPak.Where(pak => (pak.Include == "Microsoft.Extension") && (pak.Version == "1.0.0.8")).ToList().Count == 1);
        }

        [TestMethod]
        public void Core_AddInPackageListFromElement_tests()
        {
            string csp = "<Project Sdk=\"Microsoft.NET.Sdk.Web\">" +
                "<PropertyGroup>" +
                  "<TargetFramework> netcoreapp2.2 </TargetFramework> " +
                   "</PropertyGroup>" +
                  "<ItemGroup>" +
                      "<PackageReference Include=\"AProject\" Version = \"1.0.0.1910151602\" />" +
                      "<PackageReference Include=\"Microsoft.Extension\" Version = \"1.0.0.8\" />" +
                  "</ItemGroup>" +
              "</Project>";          
            var xnodes = Helper.GetXNodes(csp).ToList();

            var resultOurPaks = new List<PackageReference>();
            foreach (XNode node in xnodes)
            {
                if (node is XElement)
                {
                    XElement ele = node as XElement;
                    if (ele.Name == "PackageReference")
                        Helper.AddInPackageListFromElement(resultOurPaks,ele,Helper.GetPackageFromCoreElement, Helper.NeedNoPublicPackages);
                }
            }
            Assert.IsTrue(resultOurPaks.Count == 1);
            Assert.IsTrue(resultOurPaks[0].Include == "AProject");
            Assert.IsTrue(resultOurPaks[0].Version == "1.0.0.1910151602");

            var resultAllPaks = new List<PackageReference>();
            foreach (XNode node in xnodes)
            {
                if (node is XElement)
                {
                    XElement ele = node as XElement;
                    if (ele.Name == "PackageReference")
                        Helper.AddInPackageListFromElement(resultAllPaks, ele, Helper.GetPackageFromCoreElement, Helper.NeedAllPackages);
                }
            }
            Assert.IsTrue(resultAllPaks.Count == 2);
            Assert.IsTrue(resultAllPaks.Where(pak => (pak.Include == "AProject") && (pak.Version == "1.0.0.1910151602")).ToList().Count == 1);
            Assert.IsTrue(resultAllPaks.Where(pak => (pak.Include == "Microsoft.Extension") && (pak.Version == "1.0.0.8")).ToList().Count == 1);
        }

        [TestMethod]
        public void Framework_AddInPackageListFromElement_tests()
        {
            string csp = "<Project Sdk=\"Microsoft.NET.Sdk.Web\">" +
                     "<ItemGroup>" +
                         "<Reference Include = \"AProject, Version=2.3.50.1911061721, Culture=neutral, PublicKeyToken=1234567890, processorArchitecture=MSIL\">" +
                         "</Reference>" +
                         "<Reference Include = \"Microsoft.Extension, Version=1.0.0.8, Culture=neutral, PublicKeyToken=1234567890, processorArchitecture=MSIL\">" +
                         "</Reference>" +
                     "</ItemGroup>" +
                 "</Project>";
            var xnodes = Helper.GetXNodes(csp).ToList();

            var resultOurPaks = new List<PackageReference>();
            foreach (XNode node in xnodes)
            {
                if (node is XElement)
                {
                    XElement ele = node as XElement;
                    if (ele.Name.LocalName == "Reference")
                        Helper.AddInPackageListFromElement(resultOurPaks, ele, Helper.GetPackageFromFrameworkElement, Helper.NeedNoPublicPackages);
                }
            }
            Assert.IsTrue(resultOurPaks.Count == 1);
            Assert.IsTrue(resultOurPaks[0].Include == "AProject");
            Assert.IsTrue(resultOurPaks[0].Version == "2.3.50.1911061721");

            var resultAllPaks = new List<PackageReference>();
            foreach (XNode node in xnodes)
            {
                if (node is XElement)
                {
                    XElement ele = node as XElement;
                    if (ele.Name.LocalName == "Reference")
                        Helper.AddInPackageListFromElement(resultAllPaks, ele, Helper.GetPackageFromFrameworkElement, Helper.NeedAllPackages);
                }
            }
            Assert.IsTrue(resultAllPaks.Count == 2);
            Assert.IsTrue(resultAllPaks.Where(pak => (pak.Include == "AProject") && (pak.Version == "2.3.50.1911061721")).ToList().Count == 1);
            Assert.IsTrue(resultAllPaks.Where(pak => (pak.Include == "Microsoft.Extension") && (pak.Version == "1.0.0.8")).ToList().Count == 1);
        }

        [TestMethod]
        public void Core_GetPackagesFromXNodes_Tests()
        {
            string csp = "<Project Sdk=\"Microsoft.NET.Sdk.Web\">" +
               "<PropertyGroup>" +
                 "<TargetFramework> netcoreapp2.2 </TargetFramework> " +
                  "</PropertyGroup>" +
                 "<ItemGroup>" +
                     "<PackageReference Include=\"AProject\" Version = \"1.0.0.1910151602\" />" +
                     "<PackageReference Include=\"Microsoft.Extension\" Version = \"1.0.0.8\" />" +
                 "</ItemGroup>" +
             "</Project>";
            var xnodes = Helper.GetXNodes(csp).ToList();

            var resultOurPaks = Helper.GetPackagesFromXNodes(xnodes, Helper.NeedNoPublicPackages);
            Assert.IsTrue(resultOurPaks.Count == 1);
            Assert.IsTrue(resultOurPaks[0].Include == "AProject");
            Assert.IsTrue(resultOurPaks[0].Version == "1.0.0.1910151602");

            var resultAllPaks = Helper.GetPackagesFromXNodes(xnodes, Helper.NeedAllPackages);
            Assert.IsTrue(resultAllPaks.Count == 2);
            Assert.IsTrue(resultAllPaks.Where(pak => (pak.Include == "AProject") && (pak.Version == "1.0.0.1910151602")).ToList().Count == 1);
            Assert.IsTrue(resultAllPaks.Where(pak => (pak.Include == "Microsoft.Extension") && (pak.Version == "1.0.0.8")).ToList().Count == 1);
        }

        [TestMethod]
        public void Framework_GetPackagesFromXNodes_Tests()
        {
            string csp = "<Project Sdk=\"Microsoft.NET.Sdk.Web\">" +
                     "<ItemGroup>" +
                         "<Reference Include = \"AProject, Version=2.3.50.1911061721, Culture=neutral, PublicKeyToken=1234567890, processorArchitecture=MSIL\">" +
                         "</Reference>" +
                         "<Reference Include = \"Microsoft.Extension, Version=1.0.0.8, Culture=neutral, PublicKeyToken=1234567890, processorArchitecture=MSIL\">" +
                         "</Reference>" +
                     "</ItemGroup>" +
                 "</Project>";
            var xnodes = Helper.GetXNodes(csp).ToList();

            var resultOurPaks = Helper.GetPackagesFromXNodes(xnodes, Helper.NeedNoPublicPackages);
            Assert.IsTrue(resultOurPaks.Count == 1);
            Assert.IsTrue(resultOurPaks[0].Include == "AProject");
            Assert.IsTrue(resultOurPaks[0].Version == "2.3.50.1911061721");

            var resultAllPaks = Helper.GetPackagesFromXNodes(xnodes, Helper.NeedAllPackages);
            Assert.IsTrue(resultAllPaks.Count == 2);
            Assert.IsTrue(resultAllPaks.Where(pak => (pak.Include == "AProject") && (pak.Version == "2.3.50.1911061721")).ToList().Count == 1);
            Assert.IsTrue(resultAllPaks.Where(pak => (pak.Include == "Microsoft.Extension") && (pak.Version == "1.0.0.8")).ToList().Count == 1);
        }

        [TestMethod]
        public void IsKnownPublicPackage_Tests()
        {
            string ourPak = "AwsCaller";
            bool result = Helper.IsKnownPublicPackage(ourPak);
            Assert.IsFalse(result);

            string publicPak = "Microsoft.Anything";
            result = Helper.IsKnownPublicPackage(publicPak);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsGitPackage_Tests()
        {
            var pakWithGeneratedVersionNo = new PackageReference() { Include = "test", Version = "0.0.0.1908201259-beta" };
            var result = Helper.IsGitPackage(pakWithGeneratedVersionNo);
            Assert.IsTrue(result);

            var pakWith4BitsPublicVersionNo = new PackageReference() { Include = "test", Version = "0.0.0.0" };
            result = Helper.IsGitPackage(pakWith4BitsPublicVersionNo);
            Assert.IsFalse(result);

            var pakWith3BitsPublicVersionNo = new PackageReference() { Include = "System", Version = "1.0.0" };
            result = Helper.IsGitPackage(pakWith3BitsPublicVersionNo);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Node_NotExists_Tests()
        {
            bool result = Helper.NotExists("ADS", map);
            Assert.IsTrue(result);
            result = Helper.NotExists("AProject", map);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Link_NotExists_Tests()
        {
            bool result = Helper.NotExists("ADS", "CProject", map);
            Assert.IsTrue(result);
            result = Helper.NotExists("AProject", "CProject", map);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenPackages_UpdateMap_Tests()
        {
            var map = new Graph();

            Assert.IsTrue(map.nodes.Count == 0);
            Assert.IsTrue(map.links.Count == 0);


            var packages = new List<PackageReference>() { new PackageReference() {
                Include="source1", Version="0.0.0.0"}
            };
            map = Helper.AddPackagesInMap("target", packages, map);

            Assert.IsTrue(map.nodes.Count == 2);
            Assert.IsTrue(map.nodes[0].id == "target" || map.nodes[0].id == "source1");
            Assert.IsTrue(map.nodes[1].id == "target" || map.nodes[1].id == "source1");
            Assert.IsTrue(map.links.Count == 1);
            Assert.IsTrue((map.links[0].source == "source1") && (map.links[0].target == "target"));


            var repeatedPak = packages;
            map = Helper.AddPackagesInMap("target", repeatedPak, map);

            Assert.IsTrue(map.nodes.Count == 2);
            Assert.IsTrue(map.nodes.Where(node => node.id == "source1").ToList().Count == 1);
            Assert.IsTrue(map.nodes.Where(node => node.id == "target").ToList().Count == 1);
            Assert.IsTrue(map.links.Count == 1);
            Assert.IsTrue(map.links.Where(link => (link.source == "source1") && (link.target == "target")).ToList().Count == 1);


            var newPak = new List<PackageReference>() { new PackageReference() {
                Include="source2", Version="0.0.0.0"}
            };
            map = Helper.AddPackagesInMap("target", newPak, map);

            Assert.IsTrue(map.nodes.Count == 3);
            Assert.IsTrue(map.nodes.Where(node => node.id == "source1").ToList().Count == 1);
            Assert.IsTrue(map.nodes.Where(node => node.id == "source2").ToList().Count == 1);
            Assert.IsTrue(map.nodes.Where(node => node.id == "target").ToList().Count == 1);
            Assert.IsTrue(map.links.Count == 2);
            Assert.IsTrue(map.links.Where(link => (link.source == "source1") && (link.target == "target")).ToList().Count == 1);
            Assert.IsTrue(map.links.Where(link => (link.source == "source2") && (link.target == "target")).ToList().Count == 1);
        }

        [TestMethod]
        public void GetCorrectPath()
        {
            string path = Helper.GetDatasetPath("projectsRelationships.json");
            Console.WriteLine(path);
            string fixedPathEnding = Path.Combine("wwwroot","projectsRelationships.json");
            Assert.IsTrue(path.EndsWith(fixedPathEnding));
        }

        [TestMethod]
        public void GivenMatrix2DArray_GenerateString()
        {
            int[,] matrix = { { 0, 1 }, { 0, 0 } };
            string result = Helper.MatrixArrayToString(matrix);
            string expected = "[[0,1],[0,0]]";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GivenForceMap_GenerateMatrixString()
        {
            string result = Helper.ForceMapToMatrixString(map);
            string expected = "[[0,0],[1,0]]";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RandomColor_Generator_Test()
        {
            var result1 = Helper.GenerateRandomColor();
            var result2 = Helper.GenerateRandomColor();
            Assert.AreNotEqual(result1, result2);
        }

        [TestMethod]
        public void GivenColorRGB_ConvertToHex()
        {
            var color = Color.Red;
            var result = Helper.RGBtoHex(color);
            Assert.AreEqual("#FF0000", result);
        }
    }
}
