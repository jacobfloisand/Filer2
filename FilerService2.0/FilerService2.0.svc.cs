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
            if(IsNullOrEmpty(data.Date) || IsNullOrEmpty(data.Class) || IsNullOrEmpty(data.IsLink) || IsNullOrEmpty(data.Override) ||
                IsNullOrEmpty(data.Cookie) || IsNullOrEmpty(data.Contents) || IsNullOrEmpty(data.Name))
            {
                SetStatus(HttpStatusCode.Forbidden);
                return;
            }

            //Check to make sure the Class,Name,Cookie combo is not already taken. If it is we either delete the current resource or return conflict to client.
            if(NameAndClassAndCookieAlreadyExists(data.Name, data.Class, data.Cookie))
            {
                if (data.Override.Equals("true"))
                {
                    //This item that matches this one in the db must be delete to make room for the new one.
                    Delete(new DeleteData
                    {
                        Name = data.Name,
                        Class = data.Class,
                        Cookie = data.Cookie,
                        IsLink = data.IsLink
                    });
                }
                else
                {
                    SetStatus(HttpStatusCode.Conflict);
                    return;
                }
            }

            int DataID = 0;
            //Add the file or link to db.
            if (data.IsLink.Equals("true"))
            {
                DataID = AddLink(data);
            }
            if (data.IsLink.Equals("false"))
            {
                DataID = AddFile(data);
            }

            //Add any necessary additional data to db.
            //The second query involves updating all of the info associated with this file(class, unit, type, comments, cookie).
            string Query = "INSERT INTO Classes VALUES(@DataID, @Class) " +
                            "INSERT INTO UserIDs VALUES(@DataID, (SELECT UserID FROM Cookies WHERE Cookie = @Cookie)) ";
            if (!IsNullOrEmpty(data.Unit))
            {
                Query += "INSERT INTO Units VALUES(@DataID, @Unit) ";
            }
            if (!IsNullOrEmpty(data.Type))
            {
                Query += "INSERT INTO Types VALUES (@DataID, @Type)";
            }
            if (!IsNullOrEmpty(data.Comments))
            {
                Query += "INSERT INTO Comments VALUES(@DataID, @Comments)";
            }
            using (SqlCommand com = new SqlCommand(Query, FilerDB2Connection))
            {
                com.Parameters.AddWithValue("@Class", data.Class);
                com.Parameters.AddWithValue("@Cookie", data.Cookie);
                com.Parameters.AddWithValue("@DataID", DataID);
                if (!IsNullOrEmpty(data.Unit))
                {
                    com.Parameters.AddWithValue("@Unit", data.Unit);
                }
                if (!IsNullOrEmpty(data.Type))
                {
                    com.Parameters.AddWithValue("@Type", data.Type);
                }
                if (!IsNullOrEmpty(data.Comments))
                {
                    com.Parameters.AddWithValue("@Comments", data.Comments);
                }
                com.ExecuteNonQuery(); //This query inserts the file or link information into the database.
            }
        }

        public int AddFile(ResourceDataVerbose data)
        {
            int dataID = 0; //This will hold the dataID for this specific file.
            string Query = "INSERT INTO Files (Archive, Name, Date) VALUES (@File, @FileName, @Date)";
            using(SqlCommand com = new SqlCommand(Query, FilerDB2Connection))
            {
                com.Parameters.AddWithValue("@File", data.Contents);
                com.Parameters.AddWithValue("@FileName", data.Name);
                com.Parameters.AddWithValue("@Date", data.Date);
                using(SqlDataReader reader = com.ExecuteReader())
                {
                    reader.Read();
                    dataID = reader.GetInt32(0);
                }
            }
            return dataID;

        }

        public int AddLink(ResourceDataVerbose data)
        {
            int dataID = 0; //This will hold the dataID for this specific file.
            string Query = "INSERT INTO Links (Link, Name, Date) VALUES (@Link, @LinkName, @Date)";
            using (SqlCommand com = new SqlCommand(Query, FilerDB2Connection))
            {
                com.Parameters.AddWithValue("@Link", data.Contents);
                com.Parameters.AddWithValue("@LinkName", data.Name);
                com.Parameters.AddWithValue("@Date", data.Date);
                using (SqlDataReader reader = com.ExecuteReader())
                {
                    reader.Read();
                    dataID = reader.GetInt32(0);
                }
            }
            return dataID;
        }

        public void Delete(DeleteData data)
        {
            if(IsNullOrEmpty(data.Name) || IsNullOrEmpty(data.Class) || IsNullOrEmpty(data.Cookie) || IsNullOrEmpty(data.IsLink))
            {
                SetStatus(HttpStatusCode.Forbidden);
                return;
            }
            string Query = GetDeleteQuery(data);
            using (SqlCommand com = new SqlCommand(Query, FilerDB2Connection))
            {
                com.Parameters.AddWithValue("@Name", data.Name);
                com.Parameters.AddWithValue("@Class", data.Class);
                com.Parameters.AddWithValue("@Cookie", data.Cookie);
                using (SqlDataReader reader = com.ExecuteReader())
                {
                    reader.Read();
                    if(reader.RecordsAffected > 0)
                    {
                        SetStatus(HttpStatusCode.OK);
                        return;
                    }
                    else
                    {
                        SetStatus(HttpStatusCode.Conflict);
                        return;
                    }
                }
            }
            
        }

        private static string GetDeleteQuery(DeleteData data)
        {
            string Query = "";
            if (data.IsLink.Equals("true"))
            {
                Query = "Declare @DID INT; " +
            "SET @DID = (SELECT Links.DataID FROM Links JOIN Classes ON Links.DataID = Classes.DataID " +
                                            "WHERE Links.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                            "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                            "WHERE Cookies.Cookie = @Cookie); " +
            "DELETE FROM Classes WHERE Classes.DataID = @DID; " +
            "DELETE FROM Units WHERE Units.DataID = @DID; " +
            "DELETE FROM Types WHERE Types.DataID = @DID; " +
            "DELETE FROM Comments WHERE Comments.dataID = @DID; " +
            "DELETE FROM UserIDs WHERE UserIDs.DataID = @DID; " +
            "DELETE FROM Links WHERE Links.DataID = @DID; ";
            }
            else
            {
                Query = "Declare @DID INT; " +
            "SET @DID = (SELECT Files.DataID FROM Files JOIN Classes ON Files.DataID = Classes.DataID " +
                                            "WHERE Files.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                            "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                            "WHERE Cookies.Cookie = @Cookie); " +
            "DELETE FROM Classes WHERE Classes.DataID = @DID; " +
            "DELETE FROM Units WHERE Units.DataID = @DID; " +
            "DELETE FROM Types WHERE Types.DataID = @DID; " +
            "DELETE FROM Comments WHERE Comments.dataID = @DID; " +
            "DELETE FROM UserIDs WHERE UserIDs.DataID = @DID; " +
            "DELETE FROM Files WHERE Files.DataID = @DID; ";
            }
            return Query;
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
            string GetFileContentsQuery = "SELECT Files.Archive FROM Files WHERE DataID = " +
                                            "(SELECT Files.DataID FROM Files JOIN Classes ON Files.DataID = Classes.DataID " +
                                            "WHERE Files.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                            "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                            "WHERE Cookies.Cookie = @Cookie)";

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

        private static bool IsNullOrEmpty(string data)
        {
            return data == null || data.Equals("");
        }

        private static bool NameAndClassAndCookieAlreadyExists(string Name, string Class, string Cookie)
        {
            string Query = "(SELECT Files.DataID FROM Files JOIN Classes ON Files.DataID = Classes.DataID WHERE Name = @Name AND Class = @Class INTERSECT " +
                            "SELECT DataID FROM Cookies JOIN UserIDs ON Cookies.UserID = UserIDs.UserID WHERE Cookie = @Cookie) UNION " +
                            "(SELECT Links.DataID FROM Links JOIN Classes ON Links.DataID = Classes.DataID WHERE Name = @Name AND Class = @Class INTERSECT " +
                            "SELECT DataID FROM Cookies JOIN UserIDs ON Cookies.UserID = UserIDs.UserID WHERE Cookie = @Cookie)";
            using(SqlCommand com = new SqlCommand(Query, FilerDB2Connection))
            {
                com.Parameters.AddWithValue("@Name", Name);
                com.Parameters.AddWithValue("@Class", Class);
                com.Parameters.AddWithValue("@Cookie", Cookie);
                using (SqlDataReader reader = com.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}
