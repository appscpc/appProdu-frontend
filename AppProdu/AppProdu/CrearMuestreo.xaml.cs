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
	public partial class CrearMuestreo : ContentPage
	{
        Dictionary<int, string> samplingTypes = new Dictionary<int, string>();
        List<UserType> types;
        ObservableCollection<Sampling> Items;
        List<Sampling> muestreos;

        public CrearMuestreo ()
		{
			InitializeComponent ();

            obtenerActividades();
		}


        private async Task crearMue_Clicked(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(nombreMuestreoEntry.Text) && !String.IsNullOrEmpty(cantMuestrasEntry.Text))
            {
                if (await getSamplings(nombreMuestreoEntry.Text) == false)
                {
                    var client = new HttpClient
                    {
                        BaseAddress = new Uri("https://app-produ.herokuapp.com")
                    };
                    var idType = samplingTypes.FirstOrDefault(x => x.Value == tipo.SelectedItem.ToString()).Key;
                    var newSampling = new Sampling
                    {
                        nombre = nombreMuestreoEntry.Text,
                        cantMuestras = System.Convert.ToInt32(cantMuestrasEntry.Text),
                        cantMuestrasTotal = System.Convert.ToInt32(cantMuestrasEntry.Text),
                        descripcion = descripcionMuestreoEditor.Text,
                        fase = 1,
                        sampling_type_id = idType,
                        project_id = (int)Application.Current.Properties["id-project"],
                        muestrasActual = 0,
                        token = Application.Current.Properties["currentToken"].ToString()
                    };
                    string jsonData = JsonConvert.SerializeObject(newSampling);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("/samplings/newsampling.json", content);
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var jobject = JObject.Parse(result);
                        var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());


                        try
                        {
                            Application.Current.Properties["id-sampling"] = data.id;
                            Application.Current.Properties["fase"] = data.fase;
                            Application.Current.Properties["sampling-type-id"] = data.sampling_type_id;
                            await crearFaseAsync();
                            var etapasPage = new Etapas();
                            await Navigation.PushAsync(etapasPage);
                            this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);

                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Error!", "El nombre del muestreo ya existe.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error!", "Espacios vacíos!\nPor favor inserte el Nombre de Muestreo y la Cantidad de Muestras!", "OK");
            }

        }

        async public void obtenerActividades()
        {
            var client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync("https://app-produ.herokuapp.com/sampling_types.json");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                types = JsonConvert.DeserializeObject<List<UserType>>(responseBody);

                samplingTypes = types.ToDictionary(m => m.id, m => m.nombre);


                foreach (string type in samplingTypes.Values)
                {
                    tipo.Items.Add(type);
                }
            }
            catch (HttpRequestException e)
            {
            }
        }

        public async Task crearFaseAsync()
        {

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newFase = new Fase
            {
                fase_type_id = 1,
                sampling_id = (int)Application.Current.Properties["id-sampling"],
                extraFlag = 0,
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newFase);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/fases/newfase.json", content);
            if (response.StatusCode == HttpStatusCode.Created)
            {
                var result = await response.Content.ReadAsStringAsync();
                var jobject = JObject.Parse(result);
                var data = JsonConvert.DeserializeObject<Sampling>(jobject["fase"].ToString());


                try
                {
                    Application.Current.Properties["id-fase"] = data.id;
                    Application.Current.Properties["preliminar-done"] = 0;
                    Application.Current.Properties["definitive-done"] = 0;

                }
                catch (Exception)
                {
                }
            }
        }

        private void Tipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idType = samplingTypes.FirstOrDefault(x => x.Value == tipo.SelectedItem.ToString()).Key;
        }


        public async Task<bool> getSamplings(string nombreMuestreo)
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
                    if (pro.nombre == nombreMuestreo)
                        return true;
                }
                return false;
            }
            catch (HttpRequestException e)
            {
            }
            return false;
        }
    }
}