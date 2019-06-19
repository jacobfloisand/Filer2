using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace FilerService2._0
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class FilerService2 : IFilerService2
    {
        private static string FilerDB2;
        private static SqlConnection FilerDB2Connection;

        static FilerService2()
        {
            FilerDB2 = ConfigurationManager.ConnectionStrings["FilerDB2"].ConnectionString;
            FilerDB2Connection = new SqlConnection(FilerDB2);
            FilerDB2Connection.Open();
        }

        public void AddResource(ResourceDataVerbose data)
        {
            if(data.Date == null || data.Date.Equals("") || data.Class == null || data.Class.Equals("") || data.IsLink == null ||
                data.IsLink.Equals("") || data.Override == null || data.Override.Equals("") || data.Cookie == null || data.Cookie.Equals(""))
            {
                SetStatus(HttpStatusCode.Forbidden);
                return;
            }
            //Check to see if we have the necesary fields for a link if it's a link or a file if it's a file.
            if((data.IsLink.Equals("true") && VerifyIsLink(data)) || data.IsLink.Equals("false") && VerifyIsFile(data))
            {
                //If execution reaches this section, all the required fields are present
            }
            //This else statement gets executed if necessary fields are missing(such as link name if it's a link, or file contents if it's a file).
            else
            {
                SetStatus(HttpStatusCode.Forbidden);
                return;
            }

            //Extract unit, type, and comments into variables(use information about the data stored in the variables to build the queries).
            //Use function calls for queries instead of if statements to make things more readable.
            //Dynamically build queries(should be fairly straight forward).
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
            if (Name == null || Class == null || Cookie == null)
            {
                SetStatus(HttpStatusCode.Forbidden);
                return null;
            }
            string GetFileContentsQuery = "SELECT Files.Archive FROM Files JOIN Classes ON Files.DataID = Classes.DataID " +
                                          "WHERE Files.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                          "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                          "WHERE Cookies.Cookie = @Cookie;";

            using (SqlCommand command = new SqlCommand(GetFileContentsQuery, FilerDB2Connection))
            {
                command.Parameters.AddWithValue("@Name", Name);
                command.Parameters.AddWithValue("@Class", Class);
                command.Parameters.AddWithValue("@Cookie", Cookie);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    //If there's something to read then it's the file contents.
                    if (reader.Read())
                    {
                        return new FileContents
                        {
                            File = reader.GetString(0)
                        };
                    }
                    //If there's nothing to read then no file matched the Name, class, cookie combo.
                    else
                    {
                        SetStatus(HttpStatusCode.Conflict);
                        return null;
                    }
                }
            }
        }

        private static void SetStatus(HttpStatusCode status)
        {
            WebOperationContext.Current.OutgoingResponse.StatusCode = status;
        }

        private static bool VerifyIsLink(ResourceDataVerbose data)
        {
            if(data.Link == null || data.Link.Equals("") || data.LinkName == null || data.LinkName.Equals(""))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static bool VerifyIsFile(ResourceDataVerbose data)
        {
            if(data.File == null || data.File.Equals("") || data.FileName == null || data.FileName.Equals(""))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
