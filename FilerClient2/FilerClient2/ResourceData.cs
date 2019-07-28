using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilerClient2
{
    public class ResourceData
    {
        public string Link;
        public string Name;
        public string Date;
        public string Unit;
        public string Type;
        public string Comments;

        public ResourceData Clone()
        {
            ResourceData FreshResource = new ResourceData();
            FreshResource.Link = Link;
            FreshResource.Name = Name;
            FreshResource.Date = Date;
            FreshResource.Unit = Unit;
            FreshResource.Type = Type;
            FreshResource.Comments = Comments;
            return FreshResource;
        }
    }
}
