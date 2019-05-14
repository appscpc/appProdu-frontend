using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public partial class CrearProyecto : ContentPage
	{

        public ObservableCollection<string> Items { get; set; }

        public CrearProyecto ()
		{
			InitializeComponent ();

        }
        

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            await DisplayAlert("Item Tapped", "An item was tapped.", "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        private async Task crearPro_Clicked(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(nombreProyectoEntry.Text)) {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://app-produ.herokuapp.com")
                };
                var newProject = new Project
                {
                    nombre = nombreProyectoEntry.Text,
                    descripcion = descripcionProyectoEditor.Text,
                    user_id = (int)Application.Current.Properties["id"],
                    token = Application.Current.Properties["currentToken"].ToString()
                };
                string jsonData = JsonConvert.SerializeObject(newProject);
                Console.WriteLine("AQUI" + jsonData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/projects/newproject.json", content);
                Console.WriteLine(response.StatusCode.ToString());
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Console.WriteLine("AQUI2");
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result.ToString());
                    var jobject = JObject.Parse(result);
                    Console.WriteLine("AQUI3" + jobject["proyecto"].ToString());
                    var data = JsonConvert.DeserializeObject<Project>(jobject["proyecto"].ToString());


                    try
                    {
                        Console.WriteLine("AQUI5");
                        Console.WriteLine(data.id + " & " + data.nombre);

                        Application.Current.Properties["id-project"] = data.id;
                        var muestreosPage = new Muestreos();
                        await Navigation.PushAsync(muestreosPage);
                        Console.WriteLine("Se ha cerrado y seguido!");
                        this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);


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
            else {
                await DisplayAlert("Error!", "Espacios vacíos!\nPor favor inserte el Nombre del Proyecto!", "OK");
            }
        }
        /*
        private async Task agregarColab_Clicked()
        {
            var agregarColab = new AgregarColaborador();
            await Navigation.PushAsync(agregarColab);
            var muestreosPage = new Muestreos();
            await Navigation.PushAsync(muestreosPage);
        }*/
    }
}