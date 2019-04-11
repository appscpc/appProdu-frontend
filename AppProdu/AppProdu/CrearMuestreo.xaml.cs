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
	public partial class CrearMuestreo : ContentPage
	{
        Dictionary<int, string> samplingTypes = new Dictionary<int, string>();
        List<UserType> types;

        public CrearMuestreo ()
		{
			InitializeComponent ();

            obtenerActividades();
		}

        public class Fase
        {
            public int fase_type_id { get; set; }
            public int sampling_id { get; set; }
            public string token { get; set; }
        }

        private async Task crearMue_Clicked(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(nombreMuestreoEntry.Text) && !String.IsNullOrEmpty(cantMuestrasEntry.Text) && !String.IsNullOrEmpty(descripcionMuestreoEditor.Text))
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://app-produ.herokuapp.com")
                };
                var idType = samplingTypes.FirstOrDefault(x => x.Value == tipo.SelectedItem.ToString()).Key;
                Console.WriteLine("HOLAAAAAAA " + idType);
                var newSampling = new Sampling
                {
                    nombre = nombreMuestreoEntry.Text,
                    cantMuestras = System.Convert.ToInt32(cantMuestrasEntry.Text),
                    cantMuestrasTotal = System.Convert.ToInt32(cantMuestrasEntry.Text),
                    descripcion = descripcionMuestreoEditor.Text,
                    fase = 1,
                    tipo = idType,
                    project_id = (int)Application.Current.Properties["id-project"],
                    token = Application.Current.Properties["currentToken"].ToString()
                };
                string jsonData = JsonConvert.SerializeObject(newSampling);
                Console.WriteLine("AQUI" + jsonData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/samplings/newsampling.json", content);
                Console.WriteLine(response.StatusCode.ToString());
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Console.WriteLine("AQUI2");
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result.ToString());
                    var jobject = JObject.Parse(result);
                    Console.WriteLine("AQUI3" + jobject["muestreo"].ToString());
                    var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());


                    try
                    {
                        Console.WriteLine("AQUI5");
                        Console.WriteLine(data.id + " & " + data.nombre);

                        Application.Current.Properties["id-sampling"] = data.id;
                        var etapasPage = new Etapas();
                        await Navigation.PushAsync(etapasPage);

                    }
                    catch (Exception)
                    {
                        Console.WriteLine("AQUI6\nNo se pudo crear el muestreo");
                        //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                    }
                }
                else
                {
                    Console.WriteLine("AQUI6\nNo se pudo crear el muestreo");
                    //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                }
            }
            else
            {
                Console.WriteLine("AQUI6\nEspacios vacíos");
            }

        }

        async public void obtenerActividades()
        {
            var client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync("https://app-produ.herokuapp.com/sampling_types.json");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                Console.WriteLine(responseBody);
                types = JsonConvert.DeserializeObject<List<UserType>>(responseBody);
                Console.WriteLine(types[0].nombre);

                samplingTypes = types.ToDictionary(m => m.id, m => m.nombre);


                foreach (string type in samplingTypes.Values)
                {
                    tipo.Items.Add(type);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public async Task crearFaseAsync()
        {

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newSampling = new Fase
            {
                fase_type_id = 1,
                sampling_id = (int)Application.Current.Properties["id-sampling"],
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newSampling);
            Console.WriteLine("AQUI" + jsonData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/samplings/newsampling.json", content);
            Console.WriteLine(response.StatusCode.ToString());
        }
    }
}