using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Text;


using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AppProdu
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Proyectos : ContentPage
    {
        public ObservableCollection<string> Items { get; set; }
        List<Project> proyectos;

        public Proyectos()
        {
            InitializeComponent();
            /*
            BackendRequest client = new BackendRequest();
            var url = new Uri("https://app-produ.herokuapp.com/projects/mostrar/1/:token.json");
            var result = await client.Get<User>(url.ToString());
            if (result != null)
            {
                User user = new User();
                user.id = result.id;
                user.nombre = result.nombre;
                user.apellido1 = result.apellido1;
                user.apellido2 = result.apellido2;
                user.position_id = result.position_id;
                user.correo = result.correo;
                user.token = result.token;

                Application.Current.Properties["id"] = user.id;
                var proyectosPage = new Proyectos();
                await Navigation.PushAsync(proyectosPage);
            }*/

            getProjects();

        }

        async public void getProjects()
        {
            var client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync("https://app-produ.herokuapp.com/projects.json");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                Console.WriteLine(responseBody);
                proyectos = JsonConvert.DeserializeObject<List<Project>>(responseBody);
                Console.WriteLine(proyectos[0].nombre);
                Console.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");


                Items = new ObservableCollection<string> { };
                //string temp;
                for (int i = 0; i < proyectos.Count; i++)
                {
                    Project pro = proyectos[i];
                   // temp = "";
                    //temp += pro.nombre + "\n\n" + pro.descripcion;
                    Items.Add(pro.nombre);
                    Console.WriteLine(pro.nombre);
                }


                Console.WriteLine("ASSSSS");


                MyListView.ItemsSource = Items;
                Console.WriteLine("asdasdasdasd");





            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            //await DisplayAlert("Item Tapped", "An item was tapped.", "OK");
            var crearProyectoPage = new Estadisticas();
            await Navigation.PushAsync(crearProyectoPage);

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        private void crearProButton_Clicked(object sender, EventArgs e)
        {
            var crearProyectoPage = new CrearProyecto();
            Navigation.PushAsync(crearProyectoPage);
        }
    }
}
