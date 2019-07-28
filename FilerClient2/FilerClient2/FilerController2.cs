using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FilerClient2
{
    class FilerController2
    {
        FilerView2 GUI;
        string Cookie = "A Cookie";
        string CurrentClass = "";
        public FilerController2(FilerView2 _GUI)
        {
            GUI = _GUI;
            GetClassesAsync();
            GUI.ClassClick += ClassClicked;
            GUI.UploadEvent += UploadResource;
            GUI.DeleteEvent += DoDelete;
            GUI.GetContentsEvent += DoGetFile;
            GUI.UpdateResourceEvent += DoUpdateResource;
        }

        private async void GetClassesAsync()
        {
            using(HttpClient client = MakeClient())
            {
                HttpResponseMessage response = await client.GetAsync("Classes?Cookie=" + Cookie);
                string StringResponse = response.Content.ReadAsStringAsync().Result;
                dynamic AsObj = JsonConvert.DeserializeObject(StringResponse);
                List<string> classes = new List<string>();
                for(int i = 0; i < AsObj.Count; i++)
                {
                    string cl = (string)AsObj[i].Class;
                    classes.Add(cl);
                }
                GUI.UpdateClasses(classes);
            }
        }

        private HttpClient MakeClient()
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:61993/FilerService2.0.svc/")
            };
            return client;            
        }

        private async void ClassClicked(string ClassName)
        {
            CurrentClass = ClassName;
            using (HttpClient client = MakeClient())
            {
                HttpResponseMessage response = await client.GetAsync("Search?Class=" + ClassName + "&Cookie=" + Cookie);
                string StringResponse = response.Content.ReadAsStringAsync().Result;
                dynamic AsObj = JsonConvert.DeserializeObject(StringResponse);
                List<ResourceData> Resources = new List<ResourceData>();
                for (int i = 0; i < AsObj.Count; i++)
                {
                    DateTime t = DateTime.Parse((string)AsObj[i].Date);
                    ResourceData current = new ResourceData()
                    {
                        Link = (string)AsObj[i].Link,
                        Name = (string)AsObj[i].Name,
                        Date = t.Date.ToString("d"),
                        Unit = (string)AsObj[i].Unit,
                        Type = (string)AsObj[i].Type,
                        Comments = (string)AsObj[i].Comments
                    };
                    
                    Resources.Add(current);
                }
                GUI.UpdateResources(Resources);
            }
        }

        private async void UploadResource(string Contents, string Name, string Unit, string Type, string IsLink, string Override, string Comments)
        {
            Console.WriteLine("Uploading resource");
            dynamic Data = new ExpandoObject();
            Data.Contents = EncodeString(Contents);
            Data.Date = DateTime.Now.ToString("d");
            Data.Name = Name;
            Data.Class = CurrentClass;
            Data.Unit = NullIfEmpty(Unit);
            Data.Type = NullIfEmpty(Type);
            Data.IsLink = IsLink;
            Data.Override = Override;
            Data.Cookie = Cookie;
            Data.Comments = NullIfEmpty(Comments);

            using (HttpClient Client = MakeClient())
            {
                
                StringContent Serialized = new StringContent(JsonConvert.SerializeObject(Data), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await Client.PostAsync("save", Serialized);
                string SerializedResponse = response.Content.ReadAsStringAsync().Result;
                dynamic DesearializedResponse = JsonConvert.DeserializeObject(SerializedResponse);
                int StatusCode = (int)response.StatusCode;
                Console.WriteLine("status is: " + StatusCode);
                if (StatusCode == 403)
                {
                    throw new Exception("Some of the fields were missing from the Upload request. Failed.");
                }
                if(StatusCode == 409)
                {
                    throw new Exception("You already have afile saved under that name in that class. Choose a different name please.");
                }
                if(StatusCode == 202)
                {
                    Console.WriteLine("File successfully saved on the server!");
                    List<ResourceData> Uploaded = new List<ResourceData>();
                    Uploaded.Add(new ResourceData()
                    {
                        Link = IsLink.Equals("true") ? Contents : null,
                        Name = Name,
                        Date = DateTime.Now.ToString("d"),
                        Unit = Unit,
                        Type = Type,
                        Comments = Comments
                    });
                    GUI.UpdateResources(Uploaded);
                }
            }
            
        }

        private string EncodeString(string Contents)
        {
            return Contents;
        }

        private string DecodeString(string Contents)
        {
            return Contents;
        }

        private async void DoDelete(string Name, string IsLink)
        {
            Console.WriteLine("Delete event has been called in controller");
            dynamic Data = new ExpandoObject();
            Data.Name = Name;
            Data.Class = CurrentClass;
            Data.Cookie = Cookie;
            Data.IsLink = IsLink;
            using (HttpClient Client = MakeClient())
            {
                StringContent Serialized = new StringContent(JsonConvert.SerializeObject(Data), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await Client.PostAsync("delete", Serialized);
                string SerializedResponse = response.Content.ReadAsStringAsync().Result;
                dynamic DesearializedResponse = JsonConvert.DeserializeObject(SerializedResponse);
                int StatusCode = (int)response.StatusCode;
                if(StatusCode == 409)
                {
                    throw new Exception("File to delete was not found on server! Class: " + CurrentClass + " Name: " + Name);
                }
                if(StatusCode == 403)
                {
                    throw new Exception("One of the fields in Dodelete was Null. Oops.");
                }
                if(StatusCode == 200)
                {
                    Console.WriteLine("Successfully deleted an item!");
                }
            }
        }

        private async void DoGetFile(string Name)
        {
            using (HttpClient client = MakeClient())
            {
                HttpResponseMessage response = await client.GetAsync("File?Name=" + Name + "&Class=" + CurrentClass + "&Cookie=" + Cookie);
                string StringResponse = response.Content.ReadAsStringAsync().Result;
                dynamic AsObj = JsonConvert.DeserializeObject(StringResponse);
                string Contents = AsObj.File;
                GUI.ShowFile(DecodeString(Contents), Name);
            }
        }

        private string NullIfEmpty(string s)
        {
            if(s == null || s.Equals(""))
            {
                return null;
            }

            return s;
        }

        /// <summary>
        /// Updates a resource. The caller of this function is expected to set the values of FierView2.TempOldResource and FilerView2.TempUpdatedResource.
        /// </summary>
        /// <param name="CurrentName"></param>
        /// <param name="IsLink"></param>
        /// <param name="UpdatedClass"></param>
        /// <param name="UpdatedName"></param>
        /// <param name="UpdatedContents"></param>
        /// <param name="UpdatedUnit"></param>
        /// <param name="UpdatedType"></param>
        /// <param name="UpdatedComments"></param>
        private async void DoUpdateResource(string CurrentName, string IsLink, string UpdatedClass, string UpdatedName, string UpdatedContents,
                                            string UpdatedUnit, string UpdatedType, string UpdatedComments)
        {
            dynamic[] Data = new ExpandoObject[2];
            Data[0] = new ExpandoObject();
            Data[1] = new ExpandoObject();
            Data[0].Class = CurrentClass;
            Data[0].Name = CurrentName;
            Data[0].Cookie = Cookie;
            Data[0].IsLink = IsLink;

            Data[1].Class = UpdatedClass;
            Data[1].Name = UpdatedName;
            Data[1].Contents = UpdatedContents;
            Data[1].Unit = UpdatedUnit;
            Data[1].Type = UpdatedType;
            Data[1].Comments = UpdatedComments;

            using (HttpClient Client = MakeClient())
            {
                StringContent Serialized = new StringContent(JsonConvert.SerializeObject(Data), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await Client.PostAsync("update", Serialized);
                string SerializedResponse = response.Content.ReadAsStringAsync().Result;
                dynamic DesearializedResponse = JsonConvert.DeserializeObject(SerializedResponse);
                int StatusCode = (int)response.StatusCode;
                if (StatusCode == 403)
                {
                    GUI.CurrentResources.Add(GUI.TempOldResource);
                    throw new Exception("Something was null in the first object..");
                }
                if (StatusCode == 409)
                {
                    GUI.CurrentResources.Add(GUI.TempOldResource);
                    throw new Exception("That name is already taken!");
                }
                if (StatusCode == 200)
                {
                    GUI.Comments.Clear();
                    Console.WriteLine("Succesfully updated an item!");
                    GUI.ResourcesLeftPanel.Controls.Clear();
                    GUI.ResourcesRightPanel.Controls.Clear();
                    GUI.UpdateResources(new List<ResourceData>() { GUI.TempUpdatedResource});
                }
            }
        }
    }
}
