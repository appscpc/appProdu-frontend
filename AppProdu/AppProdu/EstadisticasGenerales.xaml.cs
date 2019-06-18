using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.DeviceOrientation;
using Plugin.DeviceOrientation.Abstractions;
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
	public partial class EstadisticasGenerales : ContentPage
	{
        Dictionary<string, string> zValues = new Dictionary<string, string>();
        Fase fasePrem = new Fase();
        Fase faseDef = new Fase();
        Sampling sampling = new Sampling();

        public EstadisticasGenerales ()
		{
			InitializeComponent ();

            zValues.Add("1.65", "0,9");
            zValues.Add("1.96", "0,95");
            zValues.Add("2.245", "0,975");
            zValues.Add("2.575", "0,99");

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            CrossDeviceOrientation.Current.LockOrientation(DeviceOrientations.Undefined);
            await obtenerFaseAsync(1);
            await obtenerFaseAsync(2);
            await obtenerSamplingAsync();

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

                    if (faseActual == 1)
                    {
                        fasePrem = data[0];
                    }
                    else if (faseActual == 2)
                    {
                        faseDef = data[0];
                    }
                }
            }
            catch (Exception)
            {
                //Error al realizar consulta al backend
            }     
        }


        public async Task obtenerSamplingAsync()
        {

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newSampling = new Sampling
            {
                id = (int)Application.Current.Properties["id-sampling"],
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newSampling);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("/samplings/getsampling.json", content);
                Console.WriteLine(response.StatusCode.ToString());
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(result);
                    var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());

                    sampling = data;
                }
            }
            catch (Exception)
            {
                //Error al realizar consulta al backend
            }
        }

        public async Task guardarError()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newFase = new Fase
            {
                id = faseDef.id,
                error = faseDef.error,
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newFase);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("/fases/updateerror.json", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(result);
                    var data = JsonConvert.DeserializeObject<Fase>(jobject["fase"].ToString());

                }
            }
            catch (Exception)
            {
                //Error al realizar consulta al backend
            }

        }

        private async Task cargarDatos_Clicked(object sender, EventArgs e)
        {
            

            nP.Text = sampling.cantMuestras.ToString();
            pP.Text = (fasePrem.p/(fasePrem.p + fasePrem.q)).ToString();
            qP.Text = (fasePrem.q / (fasePrem.p + fasePrem.q)).ToString();
            eP.Text = fasePrem.error.ToString();
            zP.Text = fasePrem.z.ToString();

            double p = (faseDef.p / (faseDef.p + faseDef.q));
            double q = (faseDef.q / (faseDef.p + faseDef.q));

            Dictionary<string, string> reversedDict = zValues.ToDictionary(
            kvp => kvp.Value,
            kvp => kvp.Key);
            double z;
            //El método TryParse trata de hacer la conversión a double utilizando el formato de US, el cual utiliza el punto como dividor de decimales
            double.TryParse(faseDef.z.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out z);
            double error = Math.Sqrt(Math.Pow(z, 2) * p * q / System.Convert.ToDouble(sampling.cantMuestrasTotal.ToString()));
            faseDef.error = float.Parse(error.ToString());

            nD.Text = sampling.cantMuestrasTotal.ToString();
            pD.Text = (faseDef.p / (faseDef.p + faseDef.q)).ToString();
            qD.Text = (faseDef.q / (faseDef.p + faseDef.q)).ToString();
            eD.Text = faseDef.error.ToString();
            zD.Text = faseDef.z.ToString();

            await guardarError();
        }

        private async Task cargarGraficos_Clicked(object sender, EventArgs e)
        {
            var graficosPage = new Graficos();
            await Navigation.PushAsync(graficosPage);
        }
    }
}