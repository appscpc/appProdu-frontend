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
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/projects/newproject.json", content);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(result);
                    var data = JsonConvert.DeserializeObject<Project>(jobject["proyecto"].ToString());


                    try
                    {
                        Application.Current.Properties["id-project"] = data.id;
                        var muestreosPage = new Muestreos();
                        await Navigation.PushAsync(muestreosPage);

                        this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);


                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
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