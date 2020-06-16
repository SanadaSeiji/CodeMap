using CodeMap.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeMap
{
    //For Chord Map
    public static partial class Helper
    {      
        public static void GenerateProjectsCsv(Graph map)
        {
            if (map is null) throw new ArgumentNullException("map is empty!");

            var output = new List<string>();
            output.Add("name,color");
            var colors = new List<string> {"#9ACD32", "#377DB8", "#F5DEB3", "#EE82EE", "#40E0D0",
                                                    "#FF6347","#D8BFD8","#D2B48C","#4682B4","#00FF7F",
                                                    "#FFFAFA","#708090","#708090","#6A5ACD","#87CEEB",
                                                    "#A0522D","#FFF5EE","#2E8B57","#F4A460","#FA8072" };
            int colorIndex = 0;
            foreach (Node node in map.nodes)
            {
                // first use hard-coded colors from sample
                if (colorIndex < colors.Count)
                {
                    output.Add(node.id + "," + colors[colorIndex]);
                    colorIndex++;
                }
                else
                    output.Add(node.id + "," + GenerateRandomColor());
            }
            string csvpath = GetDatasetPath("projects.csv");
            File.WriteAllLines(csvpath, output);
        }

        public static string GenerateRandomColor()
        {
            var r = new Random();
            var rgbColor = Color.FromArgb(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256));
            return RGBtoHex(rgbColor);
        }

        public static string RGBtoHex(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static void GenerateMatrixJson(Graph map)
        {
            if (map is null) throw new ArgumentNullException("map is empty!");

            string output = ForceMapToMatrixString(map);
            string jsonpath = GetDatasetPath("matrix.json");
            File.WriteAllText(jsonpath, output);
        }

        public static string MatrixArrayToString(int[,] matrix)
        {
            int size = matrix.GetLength(0);

            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < size; i++)
            {
                sb.Append("[");
                for (int j = 0; j < size; j++)
                {
                    sb.Append(matrix[i, j] + ",");
                }
                sb.Remove(sb.Length - 1, 1); // 0,1,0,  -> get rid of last ,
                sb.Append("],");
            }
            sb.Remove(sb.Length - 1, 1); // [], -> get rid of the last ,
            sb.Append("]");
            return sb.ToString();
        }

        public static string ForceMapToMatrixString(Graph map)
        {
            int size = map.nodes.Count();
            var matrix = new int[size, size];
            //update matrix
            var lookup = new Dictionary<string, int>();
            int index = 0;
            foreach (Node node in map.nodes)
            {
                lookup[node.id] = index;
                index++;
            }
            foreach (Link link in map.links)
            {
                matrix[lookup[link.source], lookup[link.target]] = 1;
            }

            return MatrixArrayToString(matrix);
        }
    }
}
