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
	public partial class CrearRecorrido : ContentPage
	{
		public CrearRecorrido ()
		{
			InitializeComponent ();

            var time = DateTime.Now.TimeOfDay;
            BindingContext = time.ToString();

        }

        private async Task crearRec_Clicked(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(cantOpEntry.Text) && !String.IsNullOrEmpty(temperaturaEntry.Text) && !String.IsNullOrEmpty(humedadEntry.Text))
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://app-produ.herokuapp.com")
                };
                var newProject = new Path
                {
                    cantOperarios = System.Convert.ToInt32(cantOpEntry.Text),
                    temperatura = System.Convert.ToInt32(temperaturaEntry.Text),
                    humedad = System.Convert.ToInt32(humedadEntry.Text),
                    hora = horaEntry.Text,
                    fecha = fechaPicker.Date.ToString(),
                    fase_id = (int)Application.Current.Properties["fase"],
                    token = Application.Current.Properties["currentToken"].ToString()
                };
                string jsonData = JsonConvert.SerializeObject(newProject);
                Console.WriteLine("AQUI" + jsonData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/paths/newpath.json", content);
                Console.WriteLine(response.StatusCode.ToString());
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Console.WriteLine("AQUI2");
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result.ToString());
                    var jobject = JObject.Parse(result);
                    Console.WriteLine("AQUI3" + jobject["recorrido"].ToString());
                    var data = JsonConvert.DeserializeObject<Path>(jobject["recorrido"].ToString());


                    try
                    {
                        Console.WriteLine("AQUI5");
                        Console.WriteLine(data.id + " & " + data.cantOperarios);

                        Application.Current.Properties["id-path"] = data.id;
                        var registrarActPage = new RegistrarActividades();
                        await Navigation.PushAsync(registrarActPage);


                    }
                    catch (Exception)
                    {
                        Console.WriteLine("AQUI6\nNo se pudo crear el proyecto");
                        //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                    }
                }
                else
                {
                    Console.WriteLine("AQUI6\nNo se pudo crear el proyecto");
                    //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                }
            }
            else
            {
                Console.WriteLine("AQUI6\nEspacios vacíos");
            }
        }
    }
}