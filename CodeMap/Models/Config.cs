
using System.Collections.Generic;

namespace CodeMap.Models
{
    public class Config
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public string Bucket { get; set; }
        public string DynamorTable { get; set; }
        public string Token { get; set; }
        public string ServiceBase { get; set; }
        public List<string> GitlabGroups { get; set; }
    }
}
