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
        ObservableCollection<Project> ItemsProyectos = new ObservableCollection<Project> { };
        List<Project> proyectos;

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
                if (await getProjects(nombreProyectoEntry.Text) == false) {
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
                }
                else
                {
                    await DisplayAlert("Error!", "El nombre del proyecto ya existe.", "OK");
                }
            }
            else {
                await DisplayAlert("Error!", "Espacios vacíos!\nPor favor inserte el Nombre del Proyecto!", "OK");
            }
        }


        public async Task<bool> getProjects(string nombreProyecto)
        {
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://app-produ.herokuapp.com")
                };
                var userLogged = new User
                {
                    id = (int)Application.Current.Properties["id"],
                    token = Application.Current.Properties["currentToken"].ToString()
                };
                string jsonData = JsonConvert.SerializeObject(userLogged);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/projects/userprojects.json", content);

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var jobject = JObject.Parse(responseBody);
                proyectos = JsonConvert.DeserializeObject<List<Project>>(jobject["projects"].ToString());


                ItemsProyectos = new ObservableCollection<Project>();
                for (int i = 0; i < proyectos.Count; i++)
                {
                    Project pro = proyectos[i];
                    ItemsProyectos.Add(pro);
                    Console.WriteLine(pro.nombre);
                    if (pro.nombre == nombreProyecto)
                        return true;
                }
                return false;

            }
            catch (HttpRequestException e)
            {
            }
            return false;
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