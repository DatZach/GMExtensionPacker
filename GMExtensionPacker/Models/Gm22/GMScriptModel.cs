using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GMExtensionPacker.Models.Gm22
{
    [DataContract]
    internal sealed class GMScriptModel : ModelBase
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public bool IsCompatibility { get; set; }

        [DataMember]
        public bool IsDnD { get; set; }

        public GMScriptModel()
            : base("GMScript", "1.0")
        {

        }
    }
}
