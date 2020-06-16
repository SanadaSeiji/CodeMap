using CodeMap;
using GitLabApiClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CodeMapTests
{
    public partial class HelperTests
    {
        [TestMethod]
        public void Csv_FileGenerator_Test()
        {
            Helper.GenerateProjectsCsv(map);
        }

        [TestMethod]
        public void Matrix_FileGenerator_Test()
        {
            Helper.GenerateMatrixJson(map);
        }
    }
}
