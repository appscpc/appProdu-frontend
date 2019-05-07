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
            Console.WriteLine("CERO");
            var faseSelected = (string)e.Item;
            Console.WriteLine("PRIMERO");
            if (faseActual == 1 && faseSelected.Equals("Preliminar"))
            {
                Console.WriteLine("SEGUNDO");
                await obtenerFaseAsync(faseActual);
                var recorridosPage = new Recorridos();
                await Navigation.PushAsync(recorridosPage);
                

            }
            else if (faseActual == 2 && faseSelected.Equals("Definitivo"))
            {
                Console.WriteLine("TERCERO");
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
                Console.WriteLine("NO HACE NADA!!!!!");
                await DisplayAlert("Error!", "Etapa Bloqueada.", "OK");
            }
            //await DisplayAlert("Item Tapped", "An item was tapped.", "OK");
            

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
            Console.WriteLine("AQUI" + jsonData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/fases/getfase.json", content);
            Console.WriteLine(response.StatusCode.ToString());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("AQUI2");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result.ToString());
                var jobject = JObject.Parse(result);
                Console.WriteLine("AQUI3" + jobject["fase"].ToString());
                var data = JsonConvert.DeserializeObject<List<Fase>>(jobject["fase"].ToString());
                try
                {
                    
                    Console.WriteLine("AQUI5");
                    Console.WriteLine(data[0].id);

                    Application.Current.Properties["id-fase"] = data[0].id;

                }
                catch (Exception)
                {
                    Console.WriteLine("AQUI6\nNo se pudo acceder a datos recuperados");
                    //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                }
            }
            else
            {
                Console.WriteLine("AQUI7\nNo se pudo obtener fases");
                //errorLabel.Text = "Error\nUsuario o contraseña inválido";
            }
        }
    }
}
