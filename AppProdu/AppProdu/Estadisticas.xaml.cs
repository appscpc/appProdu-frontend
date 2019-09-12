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
	public partial class Estadisticas : ContentPage
	{
        Dictionary<string, string> zValues = new Dictionary<string, string>();
        Fase faseData = new Fase();
        ObservableCollection<User> Items;
        static int userPosition;

        public Estadisticas (Fase pFase)
		{
			InitializeComponent ();

            faseData = pFase;

            zValues.Add("1.65","0,90");
            zValues.Add("1.96", "0,95");
            zValues.Add("2.245", "0,975");
            zValues.Add("2.575", "0,99");
            foreach (string type in zValues.Values)
            {
                zPicker.Items.Add(type);
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            pEntry.Text = (faseData.p / (faseData.p + faseData.q)).ToString();
            qEntry.Text = (faseData.q / (faseData.p + faseData.q)).ToString();

            await obtenerPositionAsync();
            //Si es maestro de obras
            if (userPosition == 1)
            {
                double error = 0.05; //Numero predeterminado
                double p = (faseData.p / (faseData.p + faseData.q));
                double q = (faseData.q / (faseData.p + faseData.q));
                double z = 0.95; //Numero predeterminado
                double n = Math.Pow(z, 2) * p * q / Math.Pow(error, 2);

                faseData.error = (float)error;
                faseData.z = (float)z;
                var calculosPage = new CalculosHechos(System.Convert.ToInt32(Math.Round(n)), faseData, System.Convert.ToInt32(faseData.p + faseData.q));
                await Navigation.PushAsync(calculosPage);
            }

        }

        private async Task calcular_Clicked(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(errorEntry.Text) && !String.IsNullOrEmpty(zPicker.SelectedItem.ToString()))
            {
                double error;
                //El método TryParse trata de hacer la conversión a double utilizando el formato de US, el cual utiliza el punto como dividor de decimales
                double.TryParse(errorEntry.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out error);

                if (error <= 0.1 && error >= 0.01)
                {
                    double p = (faseData.p / (faseData.p + faseData.q));
                    double q = (faseData.q / (faseData.p + faseData.q));
                    double z;
                    //El método TryParse trata de hacer la conversión a double utilizando el formato de US, el cual utiliza el punto como dividor de decimales
                    double.TryParse(zValues.FirstOrDefault(x => x.Value == zPicker.SelectedItem.ToString()).Key, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out z);
                    double n = Math.Pow(z, 2) * p * q / Math.Pow(error, 2);

                    faseData.error = (float)error;
                    faseData.z = (float)z;
                    var calculosPage = new CalculosHechos(System.Convert.ToInt32(Math.Round(n)), faseData, System.Convert.ToInt32(faseData.p + faseData.q));
                    await Navigation.PushAsync(calculosPage);
                }
                else
                {
                    await DisplayAlert("Error!", "El error debe ser un número entre 0,01 y 0,1!", "OK");
                }

            }
            else
            {
                await DisplayAlert("Error!", "Espacios vacíos!\nPor favor inserte el error y seleccione el Za/2!", "OK");
            }
        }


        public async Task obtenerPositionAsync()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newUser = new User
            {
                id = (int)Application.Current.Properties["id"],
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newUser);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("/users/getposition.json", content);
                Console.WriteLine(response.StatusCode.ToString());
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(result);
                    var data = JsonConvert.DeserializeObject<User>(jobject["usuario"].ToString());
                    userPosition = data.position_id;
                    string posicion = "Posicion: " + (data.position_id);
                    Console.WriteLine(posicion);
                }
                else
                {
                    Console.WriteLine("NOO FUNCIONAA");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROOOR", e);
            }
        }

    }
}