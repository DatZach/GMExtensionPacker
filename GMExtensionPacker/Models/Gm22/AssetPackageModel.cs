using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GMExtensionPacker.Models.Gm22
{
    [DataContract]
    internal sealed class AssetPackageModel
    {
        [DataMember]
        public string description { get; set; }

        [DataMember]
        public string helpfile { get; set; }

        [DataMember]
        public string license { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string packageID { get; set; }

        [DataMember]
        public string packageType { get; set; }

        [DataMember]
        public string projectType { get; set; }

        [DataMember]
        public string publisherName { get; set; }

        [DataMember]
        public List<Resource> resources { get; set; }

        [DataMember]
        public string version { get; set; }

        public AssetPackageModel()
        {
            projectType = "";
            resources = new List<Resource>();
            version = "1.0.0";
        }

        [DataContract]
        public sealed class Resource
        {
            [DataMember]
            public Guid id { get; set; }

            [DataMember]
            public string resourcePath { get; set; }

            [DataMember]
            public string resourceType { get; set; }

            [DataMember]
            public string viewPath { get; set; }

            public Resource()
            {
                id = Guid.NewGuid();
            }

            public static Resource NewScript(string name, string view)
            {
                return new Resource
                {
                    resourcePath = $"scripts\\{name}\\{name}.yy",
                    resourceType = "GMScript",
                    viewPath = $"scripts\\{view}"
                };
            }
        }
    }
}
