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
using System.Collections;
using System.Data.SqlTypes;

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
            SetStatus(HttpStatusCode.Accepted);
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

        public ResourceDataVerbose[] DoSearch(string Class, string Unit, string Type, string Cookie)
        {
            if(IsNullOrEmpty(Class) || IsNullOrEmpty(Cookie))
            {
                SetStatus(HttpStatusCode.Forbidden);
                return null;
            }
            string Query = GetSearchQuery(Class, Unit, Type, Cookie);
            using(SqlCommand com = new SqlCommand(Query, FilerDB2Connection))
            {
                com.Parameters.AddWithValue("@Cookie", Cookie);
                com.Parameters.AddWithValue("@Class", Class);
                if (!IsNullOrEmpty(Unit))
                {
                    com.Parameters.AddWithValue("@Unit", Unit);
                }
                else if (!IsNullOrEmpty(Type))
                {
                    com.Parameters.AddWithValue("@Type", Type);
                }
                using(SqlDataReader reader = com.ExecuteReader())
                {
                    List<ResourceDataVerbose> dataList = new List<ResourceDataVerbose>();
                    if (reader.Read())
                    {
                        string webAddress = GetStringOrNull(reader, 5);
                        string dataUnit = GetStringOrNull(reader, 2);
                        string dataType = GetStringOrNull(reader, 3);
                        string dataComments = GetStringOrNull(reader, 4);
                        dataList.Add(new ResourceDataVerbose()
                        {
                            Name = reader.GetString(0),
                            Date = reader.GetDateTime(1).ToString(),
                            Unit = dataUnit,
                            Type = dataType,
                            Comments = dataComments,
                            Link = webAddress
                        });
                        while (reader.Read())
                        {
                            webAddress = GetStringOrNull(reader, 5);
                            dataUnit = GetStringOrNull(reader, 2);
                            dataType = GetStringOrNull(reader, 3);
                            dataComments = GetStringOrNull(reader, 4);
                            dataList.Add(new ResourceDataVerbose()
                            {
                                Name = reader.GetString(0),
                                Date = reader.GetDateTime(1).ToString(),
                                Unit = dataUnit,
                                Type = dataType,
                                Comments = dataComments,
                                Link = webAddress
                            });
                        }
                        return dataList.ToArray();
                    }
                    else
                    {
                        SetStatus(HttpStatusCode.NoContent);
                        return null;
                    }
                }
            }
        }

        private static string GetStringOrNull(SqlDataReader reader, int ordinal)
        {
            try
            {
                return reader.GetString(ordinal);
            }
            catch (SqlNullValueException e)
            {
                return null;
            }
        }

        private static string GetSearchQuery(string Class, string Unit, string Type, string Cookie)
        {
            string FileQuery = "SELECT Files.Name, Files.Date, Units.Unit, Types.Type, Comments.Comment, Links.Link FROM Files " +
                            "LEFT JOIN Units ON Files.DataID = Units.DataID " +
                            "JOIN Classes ON Classes.DataID = Files.DataID " +
                            "LEFT JOIN Types ON Types.DataID = Files.DataID " +
                            "LEFT JOIN Comments ON Comments.DataID = Files.DataID " +
                            "JOIN UserIDs ON UserIDs.DataID = Files.DataID " +
                            "JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                            "LEFT JOIN Links ON Files.DataID = Links.DataID " +
                            "WHERE Cookies.Cookie = @Cookie AND Classes.Class = @Class ";
            if (!IsNullOrEmpty(Unit))
            {
                FileQuery = FileQuery + "AND Units.Unit = @Unit ";
            }
            else if (!IsNullOrEmpty(Type))
            {
                FileQuery = FileQuery + "AND Types.Type = @Type ";
            }
            FileQuery = FileQuery + "Union ";

            string LinkQuery = "SELECT Links.Name, Links.Date, Units.Unit, Types.Type, Comments.Comment, Links.Link FROM Links " +
                                "LEFT JOIN Units ON Links.DataID = Units.DataID " +
                                "JOIN Classes ON Classes.DataID = Links.DataID " +
                                "LEFT JOIN Types ON Types.DataID = Links.DataID " +
                                "LEFT JOIN Comments ON Comments.DataID = Links.DataID " +
                                "JOIN UserIDs ON UserIDs.DataID = Links.DataID " +
                                "JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                "WHERE Cookies.Cookie = @Cookie AND Classes.Class = @Class ";
            if (!IsNullOrEmpty(Unit))
            {
                LinkQuery = LinkQuery + "AND Units.Unit = @Unit ";
            }
            else if (!IsNullOrEmpty(Type))
            {
                LinkQuery = LinkQuery + "AND Types.Type = @Type ";
            }
            return FileQuery + LinkQuery;
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

        public ClassName[] DoGetClasses(string Cookie)
        {
            if (IsNullOrEmpty(Cookie))
            {
                SetStatus(HttpStatusCode.Forbidden);
                return null;
            }
            string Query = "SELECT Classes.Class FROM Classes JOIN userIDs ON Classes.DataID = userIDs.DataID " +
                            "JOIN Cookies ON userIDs.UserID = Cookies.UserID " +
                            "WHERE Cookies.Cookie = @Cookie GROUP BY Class ";
            using(SqlCommand com = new SqlCommand(Query, FilerDB2Connection))
            {
                com.Parameters.AddWithValue("@Cookie", Cookie);
                using(SqlDataReader reader = com.ExecuteReader())
                {
                    List<ClassName> dataList = new List<ClassName>();
                    if (reader.Read())
                    {
                        dataList.Add(new ClassName()
                        {
                            Class = reader.GetString(0)
                        });
                        while (reader.Read())
                        {
                            dataList.Add(new ClassName()
                            {
                                Class = reader.GetString(0)
                            });
                        }
                        SetStatus(HttpStatusCode.OK);
                        return dataList.ToArray();
                    }
                    else
                    {
                        SetStatus(HttpStatusCode.NoContent);
                        return null;
                    }
                }
            }
        }

        public void DoUpdate(ResourceDataVerbose[] Data)
        {
            ResourceDataVerbose Current = Data[0];
            ResourceDataVerbose Updated = Data[1];

            if (IsNullOrEmpty(Current.Name) || IsNullOrEmpty(Current.Class) || IsNullOrEmpty(Current.Cookie) || IsNullOrEmpty(Current.IsLink))
            {
                SetStatus(HttpStatusCode.Forbidden);
                return;
            }

           
            if (!IsNullOrEmpty(Updated.Contents))
            {
                //do query to update the Contens(this will always be an updated query).
                string Query = GetUpdateQueryForStrongType("Contents", Current);
                using (SqlCommand com = new SqlCommand(Query, FilerDB2Connection))
                {
                    com.Parameters.AddWithValue("@Name", Current.Name);
                    com.Parameters.AddWithValue("@Class", Current.Class);
                    com.Parameters.AddWithValue("@Cookie", Current.Cookie);
                    com.Parameters.AddWithValue("@UpdatedContents", Updated.Contents);
                    com.ExecuteNonQuery();
                }
            }
            //Updating the unit information.
            if (IsNullOrEmpty(Updated.Unit))
            {
                //Perform a delete query for the unit(We don't need to worry about if it doesn't exits. If it does it will be deleted.
                string Query = GetUpdateQueryForDeletingSoftAttribute("Unit", Current);
                RunUpdateSQLForDeleteSoftAttribute(Query, Current);
            }
            else
            {
                //Using sql, do an if statement where if the row exists in the unit table, update it. Otherwise, create a row.
                string Query = GetUpdateQueryForInsertingSoftAtt("Unit", Current);
                using (SqlCommand com = new SqlCommand(Query, FilerDB2Connection))
                {
                    com.Parameters.AddWithValue("@Name", Current.Name);
                    com.Parameters.AddWithValue("@Class", Current.Class);
                    com.Parameters.AddWithValue("@Cookie", Current.Cookie);
                    com.Parameters.AddWithValue("@UpdatedUnit", Updated.Unit);
                    com.ExecuteNonQuery();
                }
                
            }
            //Updating the Type information.
            if (IsNullOrEmpty(Updated.Type))
            {
                //Perform a delete query for the type(We don't need to worry about if it doesn't exits. If it does it will be deleted.
                string Query = GetUpdateQueryForDeletingSoftAttribute("Type", Current);
                RunUpdateSQLForDeleteSoftAttribute(Query, Current);
            }
            else
            {
                //Using sql, do an if statement where if the row exits in the type table, update it. Otherwise, create a row.
                string Query = GetUpdateQueryForInsertingSoftAtt("Type", Current);
                using (SqlCommand com = new SqlCommand(Query, FilerDB2Connection))
                {
                    com.Parameters.AddWithValue("@Name", Current.Name);
                    com.Parameters.AddWithValue("@Class", Current.Class);
                    com.Parameters.AddWithValue("@Cookie", Current.Cookie);
                    com.Parameters.AddWithValue("@UpdatedType", Updated.Type);
                    com.ExecuteNonQuery();
                }
            }
            //Updating the Comments information.
            if (IsNullOrEmpty(Updated.Comments))
            {
                //Perform a delete query for the Comments(We don't need to worry about if it doesn't exits. If it does it will be deleted.
                string Query = GetUpdateQueryForDeletingSoftAttribute("Comments", Current);
                RunUpdateSQLForDeleteSoftAttribute(Query, Current);
            }
            else
            {
                //Using sql, do an if statement where if the row exits in the Comments table, update it. Otherwise, create a row.
                string Query = GetUpdateQueryForInsertingSoftAtt("Comments", Current);
                using (SqlCommand com = new SqlCommand(Query, FilerDB2Connection))
                {
                    com.Parameters.AddWithValue("@Name", Current.Name);
                    com.Parameters.AddWithValue("@Class", Current.Class);
                    com.Parameters.AddWithValue("@Cookie", Current.Cookie);
                    com.Parameters.AddWithValue("@UpdatedComments", Updated.Comments);
                    com.ExecuteNonQuery();
                }
            }
            if (!IsNullOrEmpty(Updated.Name))
            {
                if (NameIsTaken(Updated.Name, Current.Class, Current.Cookie, Current.IsLink))
                {
                    SetStatus(HttpStatusCode.Conflict);
                    return;
                }
                //Do query to update the name(this will always be an update query).
                string Query = GetUpdateQueryForStrongType("Name", Current);
                using (SqlCommand com = new SqlCommand(Query, FilerDB2Connection))
                {
                    com.Parameters.AddWithValue("@Name", Current.Name);
                    com.Parameters.AddWithValue("@Class", Current.Class);
                    com.Parameters.AddWithValue("@Cookie", Current.Cookie);
                    com.Parameters.AddWithValue("@UpdatedName", Updated.Name);
                    com.ExecuteNonQuery();
                }
            }
            if (!IsNullOrEmpty(Updated.Class))
            {
                if (NameIsTaken(Current.Name, Updated.Class, Current.Cookie, Current.IsLink))
                {
                    SetStatus(HttpStatusCode.Conflict);
                    return;
                }
                //Do query to update the class(this will always be an update query).
                string Query = GetUpdateQueryForStrongType("Class", Current);
                using (SqlCommand com = new SqlCommand(Query, FilerDB2Connection))
                {
                    com.Parameters.AddWithValue("@Name", Current.Name);
                    com.Parameters.AddWithValue("@Class", Current.Class);
                    com.Parameters.AddWithValue("@Cookie", Current.Cookie);
                    com.Parameters.AddWithValue("@UpdatedClass", Updated.Class);
                    com.ExecuteNonQuery();
                }
            }
            SetStatus(HttpStatusCode.OK);
            return;
        }
        /// <summary>
        /// Generates an update query for Name, Contents, and Class. Thus ColumnName must be either Name, Contents, or Class.
        /// </summary>
        /// <param name="ColumnName"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        private string GetUpdateQueryForStrongType(string ColumnName, ResourceDataVerbose Data)
        {
            if (ColumnName.Equals("Name"))
            {
                string Query = "";
                if (Data.IsLink.Equals("true"))
                {
                    Query = "Declare @DID INT; " +
                            "SET @DID = (SELECT Links.DataID FROM Links JOIN Classes ON Links.DataID = Classes.DataID " +
                            "WHERE Links.Name = @Name AND Classes.Class = @Class INTERSECT " +
                            "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                            "WHERE Cookies.Cookie = @Cookie); " +
                            "UPDATE Links " +
                            "SET Name = @UpdatedName " +
                            "WHERE DataID = @DID ";
                }
                else
                {
                    Query = "Declare @DID INT; " +
                            "SET @DID = (SELECT Files.DataID FROM Files JOIN Classes ON Files.DataID = Classes.DataID " +
                            "WHERE Files.Name = @Name AND Classes.Class = @Class INTERSECT " +
                            "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                            "WHERE Cookies.Cookie = @Cookie); " +
                            "UPDATE Files " +
                            "SET Name = @UpdatedName " +
                            "WHERE DataID = @DID ";
                }
                return Query;
            }
            if (ColumnName.Equals("Contents"))
            {
                string Query = "";
                if (Data.IsLink.Equals("true"))
                {
                    Query = "Declare @DID INT; " +
                            "SET @DID = (SELECT Links.DataID FROM Links JOIN Classes ON Links.DataID = Classes.DataID " +
                            "WHERE Links.Name = @Name AND Classes.Class = @Class INTERSECT " +
                            "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                            "WHERE Cookies.Cookie = @Cookie); " +
                            "UPDATE Links " +
                            "SET Link = @UpdatedContents " +
                            "WHERE DataID = @DID ";
                }
                else
                {
                    Query = "Declare @DID INT; " +
                            "SET @DID = (SELECT Files.DataID FROM Files JOIN Classes ON Files.DataID = Classes.DataID " +
                            "WHERE Files.Name = @Name AND Classes.Class = @Class INTERSECT " +
                            "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                            "WHERE Cookies.Cookie = @Cookie); " +
                            "UPDATE Files " +
                            "SET Archive = @UpdatedContents " +
                            "WHERE DataID = @DID ";
                }
                return Query;
            }
            if (ColumnName.Equals("Class"))
            {
                string Query = "";
                if (Data.IsLink.Equals("true"))
                {
                    Query = "Declare @DID INT; " +
                            "SET @DID = (SELECT Links.DataID FROM Links JOIN Classes ON Links.DataID = Classes.DataID " +
                            "WHERE Links.Name = @Name AND Classes.Class = @Class INTERSECT " +
                            "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                            "WHERE Cookies.Cookie = @Cookie); " +
                            "UPDATE Classes " +
                            "SET Class = @UpdatedClass " +
                            "WHERE DataID = @DID ";
                }
                else
                {
                    Query = "Declare @DID INT; " +
                            "SET @DID = (SELECT Files.DataID FROM Files JOIN Classes ON Files.DataID = Classes.DataID " +
                            "WHERE Files.Name = @Name AND Classes.Class = @Class INTERSECT " +
                            "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                            "WHERE Cookies.Cookie = @Cookie); " +
                            "UPDATE Classes " +
                            "SET Class = @UpdatedClass " +
                            "WHERE DataID = @DID ";
                }
                return Query;
            }
            return null;
        }

        private string GetUpdateQueryForDeletingSoftAttribute(string ColumnName, ResourceDataVerbose Data)
        {
            if (ColumnName.Equals("Unit"))
            {
                string Query = "";
                if (Data.IsLink.Equals("true"))
                {
                    Query = "Declare @DID INT; " +
                "SET @DID = (SELECT Links.DataID FROM Links JOIN Classes ON Links.DataID = Classes.DataID " +
                                                "WHERE Links.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                                "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                                "WHERE Cookies.Cookie = @Cookie); " +
                "DELETE FROM Units WHERE Units.DataID = @DID; ";
                }
                else
                {
                    Query = "Declare @DID INT; " +
                "SET @DID = (SELECT Files.DataID FROM Files JOIN Classes ON Files.DataID = Classes.DataID " +
                                                "WHERE Files.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                                "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                                "WHERE Cookies.Cookie = @Cookie); " +
                "DELETE FROM Units WHERE Units.DataID = @DID; ";
                }
                return Query;
            }
            if (ColumnName.Equals("Type"))
            {
                string Query = "";
                if (Data.IsLink.Equals("true"))
                {
                    Query = "Declare @DID INT; " +
                "SET @DID = (SELECT Links.DataID FROM Links JOIN Classes ON Links.DataID = Classes.DataID " +
                                                "WHERE Links.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                                "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                                "WHERE Cookies.Cookie = @Cookie); " +
                "DELETE FROM Types WHERE Types.DataID = @DID; ";
                }
                else
                {
                    Query = "Declare @DID INT; " +
                "SET @DID = (SELECT Files.DataID FROM Files JOIN Classes ON Files.DataID = Classes.DataID " +
                                                "WHERE Files.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                                "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                                "WHERE Cookies.Cookie = @Cookie); " +
                "DELETE FROM Types WHERE Types.DataID = @DID; ";
                }
                return Query;
            }
            if (ColumnName.Equals("Comments"))
            {
                string Query = "";
                if (Data.IsLink.Equals("true"))
                {
                    Query = "Declare @DID INT; " +
                "SET @DID = (SELECT Links.DataID FROM Links JOIN Classes ON Links.DataID = Classes.DataID " +
                                                "WHERE Links.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                                "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                                "WHERE Cookies.Cookie = @Cookie); " +
                "DELETE FROM Comments WHERE Comments.DataID = @DID; ";
                }
                else
                {
                    Query = "Declare @DID INT; " +
                "SET @DID = (SELECT Files.DataID FROM Files JOIN Classes ON Files.DataID = Classes.DataID " +
                                                "WHERE Files.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                                "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                                "WHERE Cookies.Cookie = @Cookie); " +
                "DELETE FROM Comments WHERE Comments.DataID = @DID; ";
                }
                return Query;
            }
            return null;
        }

        private void RunUpdateSQLForDeleteSoftAttribute(string Query, ResourceDataVerbose Data)
        {
            using (SqlCommand com = new SqlCommand(Query, FilerDB2Connection))
            {
                com.Parameters.AddWithValue("@Name", Data.Name);
                com.Parameters.AddWithValue("@Class", Data.Class);
                com.Parameters.AddWithValue("@Cookie", Data.Cookie);
                com.ExecuteNonQuery();
            }
        }

        private string GetUpdateQueryForInsertingSoftAtt(string ColumnName, ResourceDataVerbose Data)
        {
            if (Data.IsLink.Equals("false"))
            {
                if (ColumnName.Equals("Unit"))
                {

                    string Query = "Declare @DID INT; " +
                                    "Declare @C INT " +
                                    "SET @DID = (SELECT Files.DataID FROM Files JOIN Classes ON Files.DataID = Classes.DataID " +
                                    "WHERE Files.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                    "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                    "WHERE Cookies.Cookie = @Cookie); " +
                                    " " +
                                    "SET @C = (SELECT COUNT(*) FROM Units WHERE DataID = @DID); " +
                                    " " +
                                    "IF @C = 1 " +
                                    "BEGIN " +
                                    "    UPDATE Units " +
                                    "    SET Unit = @UpdatedUnit " +
                                    " " +
                                    "    WHERE DataID = @DID " +
                                    "END " +
                                    "ELSE " +
                                    "BEGIN " +
                                    "    INSERT INTO Units VALUES(@DID, @UpdatedUnit) " +
                                    "END ";
                    return Query;
                }
                if (ColumnName.Equals("Type"))
                {
                    string Query = "Declare @DID INT; " +
                                    "Declare @C INT " +
                                    "SET @DID = (SELECT Files.DataID FROM Files JOIN Classes ON Files.DataID = Classes.DataID " +
                                    "WHERE Files.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                    "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                    "WHERE Cookies.Cookie = @Cookie); " +
                                    " " +
                                    "SET @C = (SELECT COUNT(*) FROM Types WHERE DataID = @DID); " +
                                    " " +
                                    "IF @C = 1 " +
                                    "BEGIN " +
                                    "    UPDATE Types " +
                                    "    SET Type = @UpdatedType " +
                                    " " +
                                    "    WHERE DataID = @DID " +
                                    "END " +
                                    "ELSE " +
                                    "BEGIN " +
                                    "    INSERT INTO Types VALUES(@DID, @UpdatedType) " +
                                    "END ";
                    return Query;
                }
                if (ColumnName.Equals("Comments"))
                {
                    string Query = "Declare @DID INT; " +
                                    "Declare @C INT " +
                                    "SET @DID = (SELECT Files.DataID FROM Files JOIN Classes ON Files.DataID = Classes.DataID " +
                                    "WHERE Files.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                    "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                    "WHERE Cookies.Cookie = @Cookie); " +
                                    " " +
                                    "SET @C = (SELECT COUNT(*) FROM Comments WHERE DataID = @DID); " +
                                    " " +
                                    "IF @C = 1 " +
                                    "BEGIN " +
                                    "    UPDATE Comments " +
                                    "    SET Comment = @UpdatedComments " +
                                    " " +
                                    "    WHERE DataID = @DID " +
                                    "END " +
                                    "ELSE " +
                                    "BEGIN " +
                                    "    INSERT INTO Comments VALUES(@DID, @UpdatedComments) " +
                                    "END ";
                    return Query;
                }
                return null;
            }
            else
            {
                if (ColumnName.Equals("Unit"))
                {

                    string Query = "Declare @DID INT; " +
                                    "Declare @C INT " +
                                    "SET @DID = (SELECT Links.DataID FROM Links JOIN Classes ON Links.DataID = Classes.DataID " +
                                    "WHERE Links.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                    "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                    "WHERE Cookies.Cookie = @Cookie); " +
                                    " " +
                                    "SET @C = (SELECT COUNT(*) FROM Units WHERE DataID = @DID); " +
                                    " " +
                                    "IF @C = 1 " +
                                    "BEGIN " +
                                    "    UPDATE Units " +
                                    "    SET Unit = @UpdatedUnit " +
                                    " " +
                                    "    WHERE DataID = @DID " +
                                    "END " +
                                    "ELSE " +
                                    "BEGIN " +
                                    "    INSERT INTO Units VALUES(@DID, @UpdatedUnit) " +
                                    "END ";
                    return Query;
                }
                if (ColumnName.Equals("Type"))
                {
                    string Query = "Declare @DID INT; " +
                                     "Declare @C INT " +
                                    "SET @DID = (SELECT Links.DataID FROM Links JOIN Classes ON Links.DataID = Classes.DataID " +
                                    "WHERE Links.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                    "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                    "WHERE Cookies.Cookie = @Cookie); " +
                                    " " +
                                    "SET @C = (SELECT COUNT(*) FROM Types WHERE DataID = @DID); " +
                                    " " +
                                    "IF @C = 1 " +
                                    "BEGIN " +
                                    "    UPDATE Types " +
                                    "    SET Type = @UpdatedType " +
                                    " " +
                                    "    WHERE DataID = @DID " +
                                    "END " +
                                    "ELSE " +
                                    "BEGIN " +
                                    "    INSERT INTO Types VALUES(@DID, @UpdatedType) " +
                                    "END ";
                    return Query;
                }
                if (ColumnName.Equals("Comments"))
                {
                    string Query = "Declare @DID INT; " +
                                     "Declare @C INT " +
                                    "SET @DID = (SELECT Links.DataID FROM Links JOIN Classes ON Links.DataID = Classes.DataID " +
                                    "WHERE Links.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                    "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                    "WHERE Cookies.Cookie = @Cookie); " +
                                    " " +
                                    "SET @C = (SELECT COUNT(*) FROM Comments WHERE DataID = @DID); " +
                                    " " +
                                    "IF @C = 1 " +
                                    "BEGIN " +
                                    "    UPDATE Comments " +
                                    "    SET Comment = @UpdatedComments " +
                                    " " +
                                    "    WHERE DataID = @DID " +
                                    "END " +
                                    "ELSE " +
                                    "BEGIN " +
                                    "    INSERT INTO Comments VALUES(@DID, @UpdatedComments) " +
                                    "END ";
                    return Query;
                }
                return null;
            }
        }

        private bool NameIsTaken(string Name, string Class, string Cookie, string IsLink)
        {
            string Query = "";
            if (IsLink.Equals("false"))
            {
                Query = "SELECT COUNT(*) FROM (SELECT Files.DataID FROM Files JOIN Classes ON Files.DataID = Classes.DataID " +
                                "WHERE Files.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                "WHERE Cookies.Cookie = @Cookie) AS D";                
            }
            else
            {
                Query = "SELECT COUNT(*) FROM (SELECT Links.DataID FROM Links JOIN Classes ON Links.DataID = Classes.DataID " +
                                "WHERE Links.Name = @Name AND Classes.Class = @Class INTERSECT " +
                                "SELECT UserIDs.DataID FROM UserIDs JOIN Cookies ON UserIDs.UserID = Cookies.UserID " +
                                "WHERE Cookies.Cookie = @Cookie) AS D ";
            }
            using (SqlCommand com = new SqlCommand(Query, FilerDB2Connection))
            {
                com.Parameters.AddWithValue("@Name", Name);
                com.Parameters.AddWithValue("@Class", Class);
                com.Parameters.AddWithValue("@Cookie", Cookie);
                using (SqlDataReader Reader = com.ExecuteReader())
                {
                    Reader.Read();
                    int Matches = Reader.GetInt32(0);
                    if(Matches == 0)
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
    }
}
