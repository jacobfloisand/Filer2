using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace FilerService2._0
{
    [ServiceContract]
    public interface IFilerService2
    {

        [WebInvoke(Method = "POST", UriTemplate = "/save")]
        void AddResource(ResourceDataVerbose data);

        [WebInvoke(Method = "POST", UriTemplate = "/delete")]
        void Delete(DeleteData Nickname);

        [WebGet(UriTemplate = "/File?Name={Name}&Class={Class}&Cookie={Cookie}")]
        FileContents GetFullFile(String Name, String Class, String Cookie);

        [WebGet(UriTemplate = "/Search?Class={Class}&Unit={Unit}&Type={Type}")]
        ResourceDataVerbose[] DoSearch(String Class, String Unit, String Type);
    }
}
