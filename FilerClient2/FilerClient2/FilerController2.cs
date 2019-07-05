using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public FilerController2(FilerView2 _GUI)
        {
            GUI = _GUI;
            GetClassesAsync();
        }

        private async void GetClassesAsync()
        {
            using(HttpClient client = MakeClient())
            {
                HttpResponseMessage response = await client.GetAsync("Classes?Cookie=A Cookie");
                string StringResponse = response.Content.ReadAsStringAsync().Result;
                dynamic AsOjb = JsonConvert.DeserializeObject(StringResponse);
                for(int i = 0; i < AsOjb.Count; i++)
                {
                    string cl = (string)AsOjb[i].Class;
                    Debug.Print(cl);
                }
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
    }
}
