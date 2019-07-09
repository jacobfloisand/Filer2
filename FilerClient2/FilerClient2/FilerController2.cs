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
                if(StatusCode == 403)
                {
                    throw new Exception("Some of the fields were missing from the Upload request. Failed.");
                }
                if(StatusCode == 409)
                {
                    throw new Exception("You already have afile saved under that name in that class. Choose a different name please.");
                }
                if(StatusCode == 201)
                {
                    Console.WriteLine("File successfully saved on the server!");
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
