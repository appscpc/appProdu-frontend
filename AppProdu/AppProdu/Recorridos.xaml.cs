using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppProdu
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Recorridos : ContentPage
    {
        Dictionary<int, string> pathDates = new Dictionary<int, string>();
        List<fechasPicker> dates;

        public Recorridos()
        {
            InitializeComponent();


            obtenerFechas();
            fechaPicker.SelectedIndexChanged += (object sender, EventArgs e) =>
            {
                Console.WriteLine("ES ESTE: " + fechaPicker.SelectedItem.ToString());
                fechaPicker.Unfocus();
                fechaPicker.SelectedIndex = 0;//On the screen picker stay at the old value
            };
        }


        public class fechasPicker
        {
            public int id { get; set; }
            public string fecha { get; set; }
            public string token { get; set; }
        }


        async public void obtenerFechas()
        {
            var client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync("https://app-produ.herokuapp.com/paths/dates.json");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                Console.WriteLine(responseBody);
                dates = JsonConvert.DeserializeObject<List<fechasPicker>>(responseBody);
                Console.WriteLine(dates[0].fecha);

                pathDates = dates.ToDictionary(m => m.id, m => m.fecha);


                foreach (string type in pathDates.Values)
                {
                    fechaPicker.Items.Add(type);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        private async Task crearRecorrido_Clicked(object sender, EventArgs e)
        {
            var crearRecoPage = new CrearRecorrido();
            await Navigation.PushAsync(crearRecoPage);
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
        }
    }
}