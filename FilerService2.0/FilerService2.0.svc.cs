﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace FilerService2._0
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class FilerService2 : IFilerService2
    {
        public void AddFile(ResourceDataVerbose data)
        {
            throw new NotImplementedException();
        }

        public void Delete(DeleteData Nickname)
        {
            throw new NotImplementedException();
        }

        public ResourceDataVerbose[] DoSearch(string Class, string Unit, string Type)
        {
            throw new NotImplementedException();
        }

        public FileContents GetFullFile(string Name, string Class, string Cookie)
        {
            //Select Archive from Cookies Natural Join UserIDs Natural Join Classes Natural Join Files where Cookie equals @Cookie && Class equals @Class && Name equals @Name
            //Line above is not correct. See below for a start.
            //SELECT Files.DataID FROM Files WHERE DataID IN (SELECT Classes.DataID FROM Classes WHERE Classes.Class = 'Biology') AND Name = '1210 articles_s18.pdf';
            throw new NotImplementedException();
        }
    }
}
