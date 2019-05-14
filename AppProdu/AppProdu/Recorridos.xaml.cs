using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Services;
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
    public partial class Recorridos : ContentPage
    {
        Dictionary<int?, string> pathDates = new Dictionary<int?, string>();
        List<String> dates;
        List<Path> recorridos;
        ObservableCollection<Path> Items = new ObservableCollection<Path> { };
        Fase currentFase = new Fase();

        public Recorridos()
        {
            InitializeComponent();

            Application.Current.Properties["muestras-mas"] = 0;

        }

        protected override async void OnAppearing()
        {
            dates = new List<String>();
            obtenerFechas();
            await obtenerSamplingAsync();

            await obtenerFaseAsync((int)Application.Current.Properties["id-fase"]);
            int preliminar = (int)Application.Current.Properties["preliminar-done"];
            if (preliminar == 1)
            {
                if (currentFase.extraFlag == 0)
                {
                    bool answer = await DisplayAlert("Número de muestras alcanzada!", "Desea agregar más muestras a la fase preliminar?", "Sí", "No");
                    if (answer)
                    {
                        await PopupNavigation.Instance.PushAsync(new PopupMore(1)); 
                        Application.Current.Properties["preliminar-done"] = 0;
                        muestrasRestantesLabel.Text = "Muestras restantes: " + Application.Current.Properties["muestras-mas"];
                        await ActualizarFlag(1);
                    }
                    else
                    {
                        Application.Current.Properties["preliminar-done"] = 2;
                        await cambiarFase();
                        var calcularPage = new Estadisticas(currentFase);
                        await Navigation.PushAsync(calcularPage);
                        this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);
                    }
                }
                else
                {
                    Application.Current.Properties["preliminar-done"] = 2;
                    await DisplayAlert("Número de muestras alcanzada!", "Se procederá a calcular la cantidad de muestras para la etapa definitiva!", "OK");
                    await cambiarFase();
                    var calcularPage = new Estadisticas(currentFase);
                    await Navigation.PushAsync(calcularPage);
                    this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);
                }
                
            }
            else if (preliminar == 2)
            {
                var definitive = (int)Application.Current.Properties["definitive-done"];
                if (definitive == 1)
                {
                    if (currentFase.extraFlag == 0)
                    {
                        bool answer = await DisplayAlert("Número de muestras alcanzada!", "Desea agregar más muestras a la fase definitiva?", "Sí", "No");
                        if (answer)
                        {
                            await PopupNavigation.Instance.PushAsync(new PopupMore(2)); 
                            Application.Current.Properties["definitive-done"] = 0;
                            muestrasRestantesLabel.Text = "Muestras restantes: " + Application.Current.Properties["muestras-mas"];
                            await ActualizarFlag(1);
                        }
                        else
                        {
                            Application.Current.Properties["definitive-done"] = 2;
                            await terminarFaseDef();
                            var estadisticas = new EstadisticasGenerales();
                            await Navigation.PushAsync(estadisticas);
                            this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);
                        }
                    }
                    else
                    {
                        Application.Current.Properties["definitive-done"] = 2;
                        await DisplayAlert("Número de muestras alcanzada!", "Se procederá a mostrar las estadísticas finales!", "OK");
                        await terminarFaseDef();
                        var estadisticas = new EstadisticasGenerales();
                        await Navigation.PushAsync(estadisticas);
                        this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);
                    }
                }
                else if (definitive == 3)
                {
                    //caso en el que N sea menor o igual a n
                }
                
            }
            
        }


        public class fechasPicker
        {
            public int? id { get; set; }
            public string fecha { get; set; }
            public string token { get; set; }
        }


        async public void obtenerFechas()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newPath = new Path
            {
                sampling_id = (int)Application.Current.Properties["id-sampling"],
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newPath);
            Console.WriteLine("AQUI" + jsonData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/paths/dates.json", content);
            Console.WriteLine(response.StatusCode.ToString());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("AQUI2");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result.ToString());
                var jobject = JObject.Parse(result);
                Console.WriteLine("AQUI3 "+ result);

                try
                {

                    dates = JsonConvert.DeserializeObject<List<String>>(jobject["fechas"].ToString());
                    Console.WriteLine("AQUI5");


                    //pathDates = dates.ToDictionary(m => m.id, m => m.fecha);

                    

                    foreach (string type in dates)
                    {
                        fechaPicker.Items.Add(type);
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
                catch (Exception)
                {
                    Console.WriteLine("\nERROR!");
                }
            }
            else
            {
                Console.WriteLine("AQUI6\nNo se pudo crear el muestreo");
                //errorLabel.Text = "Error\nUsuario o contraseña inválido";
            }

        }

        private async Task crearRecorrido_Clicked(object sender, EventArgs e)
        {
            var crearRecoPage = new CrearRecorrido();
            await Navigation.PushAsync(crearRecoPage);
        }

        public async Task obtenerFaseAsync(int faseActual)
        {

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newFase = new Fase
            {
                id = (int)Application.Current.Properties["id-fase"],
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newFase);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/fases/getfasebyid.json", content);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadAsStringAsync();
                var jobject = JObject.Parse(result);
                var data = JsonConvert.DeserializeObject<Fase>(jobject["fase"].ToString());
                try
                {

                    currentFase = data;

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

            HttpResponseMessage response = await client.PostAsync("/samplings/getsampling.json", content);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadAsStringAsync();
                var jobject = JObject.Parse(result);
                var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());
                try
                {
                    muestrasRestantesLabel.Text = "Muestras restantes: " + (data.cantMuestrasTotal - data.muestrasActual);
                    muestrasActualesLabel.Text = "Muestras actuales: " + data.muestrasActual;

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

        public async Task ActualizarFlag(int newvalue)
        {

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newFase = new Fase
            {
                id = currentFase.id,
                extraFlag = newvalue,
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newFase);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/fases/updateflag.json", content);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("AQUI2");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result.ToString());
                var jobject = JObject.Parse(result);
                Console.WriteLine("AQUI3" + jobject["fase"].ToString());
                var data = JsonConvert.DeserializeObject<Fase>(jobject["fase"].ToString());
                try
                {

                    Console.WriteLine("AQUI5");
                    Console.WriteLine(data.id);
                    currentFase = data;

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

        public async Task cambiarFase()
        {

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newFase = new Sampling
            {
                id = (int)Application.Current.Properties["id-sampling"],
                fase = 2,
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newFase);
            Console.WriteLine("AQUI" + jsonData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/samplings/changefase.json", content);
            Console.WriteLine(response.StatusCode.ToString());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("AQUI2");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result.ToString());
                var jobject = JObject.Parse(result);
                Console.WriteLine("AQUI3" + jobject["muestreo"].ToString());
                var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());
                try
                {

                    Console.WriteLine("AQUI5");
                    Console.WriteLine(data.id);
                    Application.Current.Properties["fase"] = 2;
                    await crearFaseDef();
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

        public async Task crearFaseDef()
        {

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newFase = new Fase
            {
                id = currentFase.id,
                p = currentFase.p,
                q = currentFase.q,
                error = currentFase.error,
                z = currentFase.z,
                sampling_id = currentFase.sampling_id, 
                fase_type_id = 2,
                extraFlag = 0,
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newFase);
            Console.WriteLine("AQUI" + jsonData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/fases/changefase.json", content);
            Console.WriteLine(response.StatusCode.ToString());
            if (response.StatusCode == HttpStatusCode.Created)
            {
                Console.WriteLine("AQUI2");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result.ToString());
                var jobject = JObject.Parse(result);
                Console.WriteLine("AQUI61516516516516516516516516516516516" + jobject["fase"].ToString());
                var data = JsonConvert.DeserializeObject<Fase>(jobject["fase"].ToString());
                try
                {

                    Console.WriteLine("AQUI599999999999999999999999999999999999999999999999999999999999");
                    Console.WriteLine(data.id);
                    Application.Current.Properties["id-fase"] = data.id;

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

        public async Task terminarFaseDef()
        {

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newFase = new Sampling
            {
                id = (int)Application.Current.Properties["id-sampling"],
                fase = 3,
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newFase);
            Console.WriteLine("AQUI" + jsonData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/samplings/changefase.json", content);
            Console.WriteLine(response.StatusCode.ToString());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("AQUI2");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result.ToString());
                var jobject = JObject.Parse(result);
                Console.WriteLine("AQUI3" + jobject["muestreo"].ToString());
                var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());
                try
                {

                    Console.WriteLine("AQUI5");
                    Console.WriteLine(data.id);
                    Application.Current.Properties["fase"] = 3;
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

        async public void getPaths()
        {
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://app-produ.herokuapp.com")
                };
                var fechaSelected = new Path
                {
                    fecha = fechaPicker.SelectedItem.ToString(),
                    token = Application.Current.Properties["currentToken"].ToString()
                };
                string jsonData = JsonConvert.SerializeObject(fechaSelected);
                //Console.WriteLine("AQUI");
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/paths/datepaths.json", content);

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var jobject = JObject.Parse(responseBody);
                Console.WriteLine("AQUI3" + jobject["recorrido"].ToString());
                recorridos = JsonConvert.DeserializeObject<List<Path>>(jobject["recorrido"].ToString());

                Items = new ObservableCollection<Path>();

                for (int i = 0; i < recorridos.Count; i++)
                {
                    Path pro = recorridos[i];
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

        private void FechaPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            getPaths();
        }

        private async Task MyListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;


            Path temp = (Path)e.Item;
            var detallePage = new DetalleRecorrido(temp);
            await Navigation.PushAsync(detallePage);
        }
    }
}