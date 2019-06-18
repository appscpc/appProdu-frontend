using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppProdu
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Muestreos : ContentPage
    {
        ObservableCollection<Sampling> Items;
        List<Sampling> muestreos;

        public Muestreos()
        {
            InitializeComponent();


        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Items = new ObservableCollection<Sampling>();
            getSamplings();
        }

        async public void getSamplings()
        {
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://app-produ.herokuapp.com")
                };
                var userLogged = new User
                {
                    id = (int)Application.Current.Properties["id-project"],
                    token = Application.Current.Properties["currentToken"].ToString()
                };
                string jsonData = JsonConvert.SerializeObject(userLogged);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/samplings/projectsamplings.json", content);

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var jobject = JObject.Parse(responseBody);
                muestreos = JsonConvert.DeserializeObject<List<Sampling>>(jobject["samplings"].ToString());

                Items = new ObservableCollection<Sampling> { };
                for (int i = 0; i < muestreos.Count; i++)
                {
                    Sampling pro = muestreos[i];
                    Items.Add(pro);
                    Console.WriteLine(pro.nombre);
                }
                

                MyListView.ItemsSource = Items;

            }
            catch (HttpRequestException e)
            {
            }
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            var samplingSelected = (Sampling)e.Item;
            Application.Current.Properties["fase"] = samplingSelected.fase;
            Application.Current.Properties["id-sampling"] = samplingSelected.id;

            Application.Current.Properties["sampling-type-id"] = samplingSelected.sampling_type_id;
            if(samplingSelected.fase == 1)
            {
                Application.Current.Properties["preliminar-done"] = 0;
                Application.Current.Properties["definitive-done"] = 0;
            }
            else
            {
                Application.Current.Properties["preliminar-done"] = 2;
                Application.Current.Properties["definitive-done"] = 0;
            }
            var etapasPage = new Etapas();
            await Navigation.PushAsync(etapasPage);


            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        async private void crearMuesButton_Clicked(object sender, EventArgs e)
        {
            var crearMuePage = new CrearMuestreo();
            await Navigation.PushAsync(crearMuePage);
        }
    }
}
