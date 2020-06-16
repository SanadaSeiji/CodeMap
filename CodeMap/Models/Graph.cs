using System.Collections.Generic;

namespace CodeMap.Models
{
    public class Graph
    {
        public List<Node> nodes { get; set; }
        public List<Link> links { get; set; }

        public Graph()
        {
            nodes = new List<Node>();
            links = new List<Link>();
        }
    }
}
