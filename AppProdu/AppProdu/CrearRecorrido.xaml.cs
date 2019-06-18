using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
	public partial class CrearRecorrido : ContentPage
	{
        Dictionary<int, string> humedadDict = new Dictionary<int, string>();
        List<UserType> humedadList;

        public CrearRecorrido ()
		{
			InitializeComponent ();

            var time = DateTime.Now.TimeOfDay;
            BindingContext = time.ToString();

            for(int i = 1; i<=100; i++)
            {
                humedadDict.Add(i, i + "%");
            }

            foreach (string type in humedadDict.Values)
            {
                humedadPicker.Items.Add(type);
            }

        }

        private async Task crearRec_Clicked(object sender, EventArgs e)
        {
            double temp;
            double.TryParse(temperaturaEntry.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out temp);
            string datePicked = String.Format("{0:d/M/yyyy}", fechaPicker.Date);
            var humedadValue = humedadDict.FirstOrDefault(x => x.Value == humedadPicker.SelectedItem.ToString()).Key;
            if (!String.IsNullOrEmpty(cantOpEntry.Text) && !String.IsNullOrEmpty(temperaturaEntry.Text) && !String.IsNullOrEmpty(humedadPicker.SelectedItem.ToString()))
            {
                if(temp > 0 && temp < 40)
                {
                    var client = new HttpClient
                    {
                        BaseAddress = new Uri("https://app-produ.herokuapp.com")
                    };
                    var newPath = new Path
                    {
                        cantOperarios = System.Convert.ToInt32(cantOpEntry.Text),
                        temperatura = temp,
                        humedad = humedadValue,
                        hora = horaEntry.Text,
                        fecha = datePicked,
                        fase_id = (int)Application.Current.Properties["id-fase"],
                        sampling_id = (int)Application.Current.Properties["id-sampling"],
                        token = Application.Current.Properties["currentToken"].ToString()
                    };
                    string jsonData = JsonConvert.SerializeObject(newPath);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    
                    HttpResponseMessage response = await client.PostAsync("/paths/newpath.json", content);
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var jobject = JObject.Parse(result);
                        var data = JsonConvert.DeserializeObject<Path>(jobject["recorrido"].ToString());

                        try
                        {
                            Application.Current.Properties["id-path"] = data.id;
                            var registrarActPage = new RegistrarActividades(newPath.cantOperarios);
                            await Navigation.PushAsync(registrarActPage);
                            this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Error de temperatura!", "La temperatura no debe ser menor a 0° ni mayor a 40°!\nPor favor cambiar este valor!", "OK");
                }
                
            }
            else
            {
                await DisplayAlert("Error!", "Espacios vacíos!\nPor favor rellenar todos los espacios!", "OK");
            }
        }
    }
}