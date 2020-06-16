using CodeMap.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeMap
{
    public static partial class Helper
    {
        public static string GetDatasetPath(string filename)
        {   // local d3 data in hard-coded folderpath: eg.: ...wwwroot/dataset.json
            string workingDir = AppDomain.CurrentDomain.BaseDirectory; // app

            string cDir = Path.Combine(workingDir, "wwwroot"); // app/wwwroot
            return Path.Combine(cDir, filename);
        }

        public static Graph AddPackagesInMap(string target, List<PackageReference> packages, Graph map)
        {
            if (NotExists(target, map))
                map.nodes.Add(new Node() { id = target });
            foreach (PackageReference pak in packages)
            {
                if (NotExists(pak.Include, map))
                    map.nodes.Add(new Node() { id = pak.Include });
                // if record in package repeated, it happens
                if (NotExists(pak.Include, target, map))
                    map.links.Add(new Link() { source = pak.Include, target = target });
            }
            return map;
        }

        public static bool NotExists(string target, Graph map)
        {
            var match = map.nodes.FirstOrDefault(node => node.id == target);
            return match is null;
        }

        public static bool NotExists(string source, string target, Graph map)
        {
            var match = map.links.FirstOrDefault(link => (link.source == source) && (link.target == target));
            return match is null;
        }
    }
}
