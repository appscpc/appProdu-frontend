using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public partial class Etapas : ContentPage
    {
        public ObservableCollection<string> Items { get; set; }

        public Etapas()
        {
            InitializeComponent();

            Items = new ObservableCollection<string>
            {
                "Preliminar",
                "Definitivo",
                "Estadísticas Finales"
            };
			
			MyListView.ItemsSource = Items;
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            int faseActual = (int)Application.Current.Properties["fase"];
            var faseSelected = (string)e.Item;
            if (faseActual == 1 && faseSelected.Equals("Preliminar"))
            {
                await obtenerFaseAsync(faseActual);
                var recorridosPage = new Recorridos();
                await Navigation.PushAsync(recorridosPage);
                

            }
            else if (faseActual == 2 && faseSelected.Equals("Definitivo"))
            {
                await obtenerFaseAsync(faseActual);
                var recorridosPage = new Recorridos();
                await Navigation.PushAsync(recorridosPage);
                
            }
            else if(faseActual == 3 && e.Item.Equals("Estadísticas Finales"))
            {
                var estadisticasGenPage = new EstadisticasGenerales();
                await Navigation.PushAsync(estadisticasGenPage);
            }
            else
            {
                await DisplayAlert("Error!", "Etapa Bloqueada.", "OK");
            }

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        public async Task obtenerFaseAsync(int faseActual)
        {

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newFase = new Fase
            {
                fase_type_id = faseActual,
                sampling_id = (int)Application.Current.Properties["id-sampling"],
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newFase);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("/fases/getfase.json", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(result);
                    var data = JsonConvert.DeserializeObject<List<Fase>>(jobject["fase"].ToString());
                    Application.Current.Properties["id-fase"] = data[0].id;

                }
            }
            catch (Exception)
            {
                //Error al realizar consulta al backend
            }
        }
    }
}
