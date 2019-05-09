using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppProdu
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CalculosHechos : ContentPage
	{
        int N, n;
        Fase faseData = new Fase();
        Dictionary<string, string> zValues = new Dictionary<string, string>();

        public CalculosHechos (int pN, Fase pFaseData, int pn)
		{
			InitializeComponent ();

            zValues.Add("1,65", "0,90");
            zValues.Add("1,96", "0,95");
            zValues.Add("2,245", "0,975");
            zValues.Add("2,575", "0,99");

            N = pN;
            n = pn;
            faseData = pFaseData;
            Console.WriteLine("N= " + N + " n= " + n);

            NEntry.Text = N.ToString();
            nEntry.Text = n.ToString();
            nDefEntry.Text = (N-n).ToString();
        }

        private void back_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private async Task aceptar_Clicked(object sender, EventArgs e)
        {
            if(n >= N)
            {
                Application.Current.Properties["definitive-done"] = 3;
                var recorridosPage = new Recorridos();
                await Navigation.PushAsync(recorridosPage);
                //terminar
            }
            else
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://app-produ.herokuapp.com")
                };
                var newSampling = new Sampling
                {
                    id = (int)Application.Current.Properties["id-sampling"],
                    cantMuestras = 0,
                    cantMuestrasTotal = N-n,
                    token = Application.Current.Properties["currentToken"].ToString()
                };
                string jsonData = JsonConvert.SerializeObject(newSampling);
                Console.WriteLine("AQUI" + jsonData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/samplings/addmoresamplings.json", content);
                Console.WriteLine(response.StatusCode.ToString());
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("AQUI2");
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result.ToString());
                    var jobject = JObject.Parse(result);
                    Console.WriteLine("AQUI3" + jobject["muestreo"].ToString());
                    var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());


                    try
                    {
                        Console.WriteLine("AQUI5" + data.nombre);
                        Console.WriteLine(data.id + " & " + data.nombre);
                        
                        await guardarErrorZ();
                        var etapasPage = new Etapas();
                        await Navigation.PushAsync(etapasPage);

                    }
                    catch (Exception)
                    {
                        Console.WriteLine("AQUI6\nNo se pudo crear el muestreo");
                        //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                    }
                }
            }
        }

        public async Task guardarErrorZ()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newFase = new Fase
            {
                id = faseData.id,
                error = faseData.error,
                z = faseData.z,
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newFase);
            Console.WriteLine("AQUI" + jsonData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/fases/updateerrorz.json", content);
            Console.WriteLine(response.StatusCode.ToString());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("AQUI2");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result.ToString());
                var jobject = JObject.Parse(result);
                Console.WriteLine("AQUI3" + jobject["fase"].ToString());
                var data = JsonConvert.DeserializeObject<Fase>(jobject["fase"].ToString());
                try
                {

                    Console.WriteLine("AQUI5");
                    Console.WriteLine(data.id + " " + (int)Application.Current.Properties["id-fase"]);
                    newFase.id = (int)Application.Current.Properties["id-fase"];
                    newFase.error = 0;
                    jsonData = JsonConvert.SerializeObject(newFase);
                    Console.WriteLine("AQUI" + jsonData);
                    content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    response = await client.PostAsync("/fases/updateerrorz.json", content);
                    Console.WriteLine(response.StatusCode.ToString());
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine("AQUI22");
                        result = await response.Content.ReadAsStringAsync();
                    }

                }
                catch (Exception)
                {
                    Console.WriteLine("AQUI6\nNo se pudo acceder a datos recuperados");
                    //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                }
            }
            else
            {
                Console.WriteLine("AQUI7\nNo se pudo obtener fases");
                //errorLabel.Text = "Error\nUsuario o contraseña inválido";
            }
        }
    }
}