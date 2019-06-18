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
            NavigationPage.SetHasBackButton(this, false);   //Desaparece el botón de atrás de la app
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
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("/paths/newcomment.json", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(result);
                    var data = JsonConvert.DeserializeObject<Path>(jobject["recorrido"].ToString());

                    await Navigation.PopAsync(); //Eliminar la esta página de la pila
                }
                else
                {
                    //La consulta devolvió un código de status distinto. Ocurrió un error en la consulta en el backend
                }

            }
            catch (Exception)
            {
                //Error al realizar consulta al backend
            }


        }
    }
}