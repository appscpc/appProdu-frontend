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
                Console.WriteLine("AQUI " + jsonData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/samplings/projectsamplings.json", content);

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var jobject = JObject.Parse(responseBody);
                Console.WriteLine("AQUI3" + jobject["samplings"].ToString());
                muestreos = JsonConvert.DeserializeObject<List<Sampling>>(jobject["samplings"].ToString());

                Console.WriteLine(responseBody);
                //muestreos = JsonConvert.DeserializeObject<List<Sampling>>(responseBody);
                //Console.WriteLine(proyectos[0].nombre);
                Console.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");


                Items = new ObservableCollection<Sampling> { };
                //string temp;
                for (int i = 0; i < muestreos.Count; i++)
                {
                    Sampling pro = muestreos[i];
                    // temp = "";
                    //temp += pro.nombre + "\n\n" + pro.descripcion;
                    Items.Add(pro);
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
            var samplingSelected = (Sampling)e.Item;
            Application.Current.Properties["fase"] = samplingSelected.fase;
            Application.Current.Properties["id-sampling"] = samplingSelected.id;
            Console.WriteLine("DAMN BRO: " + samplingSelected.sampling_type_id + " & " + samplingSelected.nombre);
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
