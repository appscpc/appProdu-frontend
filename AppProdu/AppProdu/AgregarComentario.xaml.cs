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
	public partial class AgregarComentario : ContentPage
	{
		public AgregarComentario ()
		{
			InitializeComponent ();
		}

        private async Task agregarComentario_Clicked(object sender, EventArgs e)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newPath = new Path
            {
                id = (int)Application.Current.Properties["id-path"],
                comentario = comentarioEditor.Text,
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newPath);
            Console.WriteLine("AQUI" + jsonData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/paths/newcomment.json", content);
            Console.WriteLine(response.StatusCode.ToString());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("AQUI2");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result.ToString());
                var jobject = JObject.Parse(result);
                try
                {
                    Console.WriteLine("AQUI3" + jobject["recorrido"].ToString());
                    var data = JsonConvert.DeserializeObject<Path>(jobject["recorrido"].ToString());
                    Console.WriteLine("AQUI5");
                    var recorridosPage = new Recorridos();
                    await Navigation.PushAsync(recorridosPage);

                }
                catch (Exception)
                {
                    Console.WriteLine("AQUI6\nERROR");
                    //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                }
            }
            else
            {
                Console.WriteLine("AQUI6\nNo se pudo crear el muestreo");
                //errorLabel.Text = "Error\nUsuario o contraseña inválido";
            }
        }
    }
}