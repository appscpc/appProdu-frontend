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
using Newtonsoft.Json.Linq;

namespace AppProdu
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Proyectos : ContentPage
    {
        ObservableCollection<Project> Items = new ObservableCollection<Project> { };
        List<Project> proyectos;

        public Proyectos()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            getProjects();
        }

        async public void getProjects()
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
                //Console.WriteLine("AQUI");
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/projects/userprojects.json", content);

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var jobject = JObject.Parse(responseBody);
                Console.WriteLine("AQUI3" + jobject["projects"].ToString());
                proyectos = JsonConvert.DeserializeObject<List<Project>>(jobject["projects"].ToString());


                Items = new ObservableCollection<Project>();
                for (int i = 0; i < proyectos.Count; i++)
                {
                    Project pro = proyectos[i];
                    Items.Add(pro);
                }


                MyListView.ItemsSource = Items;

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

            var temp = (Project)e.Item;
            //await DisplayAlert("Item Tapped", "An item was tapped. " + temp.nombre, "OK");
            var idType = MyListView;
            Application.Current.Properties["id-project"] = temp.id;
            var muestreoPage = new Muestreos();
            await Navigation.PushAsync(muestreoPage);

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }


        async void OnPreviousPageButtonClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Desea cerrar sesión?", "El usuario cerrará cesión", "Sí", "No");
            Console.WriteLine("Answer: " + answer);
            if (answer)
            {
                await Navigation.PopAsync();
            }
            
        }

        private void crearProButton_Clicked(object sender, EventArgs e)
        {
            var crearProyectoPage = new CrearProyecto();
            Navigation.PushAsync(crearProyectoPage);
        }

        protected override bool OnBackButtonPressed()
        {
            //await Navigation.PopAsync(true);
            base.OnBackButtonPressed();
            
            Device.BeginInvokeOnMainThread(async () =>
            {
                bool answer = await DisplayAlert("Cerrar Sesión!", "Está seguro que desea cerrar sesión?", "Sí", "No");
                if (answer)
                {
                    Application.Current.Properties["id"] = 0;
                    Application.Current.Properties["currentToken"] = "";
                    await Navigation.PopAsync(true);
                }
            });

            return true;

        }

    }
}
