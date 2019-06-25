using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FilerService2._0
{

    /// <summary>
    /// This Data model is used when sending search result data to the client OR when the client is uploading a new resource to the server.
    /// </summary>
    [DataContract]
    public class ResourceDataVerbose
    {
        [DataMember(EmitDefaultValue = false)]
        public string Contents { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Date { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Class { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Unit { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Type { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string IsLink { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Override { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Cookie { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Comments { get; set; }
    }

    /// <summary>
    /// This data model is used to receive information for a file to be deleted.
    /// </summary>
    [DataContract]
    public class DeleteData
    {
        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Class { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Cookie { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string IsLink { get; set; }
    }

    /// <summary>
    /// This data model is used when the server is sending file contents to the client, per the client's request.
    /// </summary>
    [DataContract]
    public class FileContents
    {
        [DataMember(EmitDefaultValue = false)]
        public string File { get; set; }
    }
}