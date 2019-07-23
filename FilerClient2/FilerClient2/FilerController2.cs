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
            Data.Contents = Contents;
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

        private string NullIfEmpty(string s)
        {
            if(s == null || s.Equals(""))
            {
                return null;
            }

            return s;
        }
    }
}
