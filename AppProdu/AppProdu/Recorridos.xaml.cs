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
        List<string> dates = new List<string>();
        List<Path> recorridos;
        ObservableCollection<Path> Items = new ObservableCollection<Path> { };
        ObservableCollection<string> ItemsDates = new ObservableCollection<string> { };
        Fase currentFase = new Fase();

        public Recorridos()
        {
            InitializeComponent();

            Application.Current.Properties["muestras-mas"] = 0;

        }

        protected override async void OnAppearing()
        {
            dates = new List<string>();
            obtenerFechas();
            
            await obtenerSamplingAsync();

            await obtenerFaseAsync((int)Application.Current.Properties["id-fase"]);

            int preliminar = (int)Application.Current.Properties["preliminar-done"];

            //if para saber por cual fase se encuentra
            if (preliminar == 1)
            {
                //if para saber si ya se hizo la pregunta de si desea más muestras una vez llegada a la cantidad guardada
                if (currentFase.extraFlag == 0)
                {
                    bool answer = await DisplayAlert("Número de muestras alcanzada!", "Desea agregar más muestras a la fase preliminar?", "Sí", "No");
                    if (answer)
                    {
                        await PopupNavigation.Instance.PushAsync(new PopupMore(1)); //Se muestra popup de más muestras     
                        Application.Current.Properties["preliminar-done"] = 0;  //Se cambia de nuevo el valor para que no esté finalizado
                        muestrasRestantesLabel.Text = "Muestras restantes: " + Application.Current.Properties["muestras-mas"];  //Se actualiza etiqueta
                        await ActualizarFlag(1);   //Método para actualizar que se hizo la pregunta
                    }
                    else
                    {
                        Application.Current.Properties["preliminar-done"] = 2;  //Se cambia valor para indicar que se finaliza por completo la etapa
                        await cambiarFase();    //Método para cambiar de fase actual
                        var calcularPage = new Estadisticas(currentFase);
                        await Navigation.PushAsync(calcularPage);
                        this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);
                    }
                }
                else
                {
                    Application.Current.Properties["preliminar-done"] = 2;  //Se cambia valor para indicar que se finaliza por completo la etapa
                    await DisplayAlert("Número de muestras alcanzada!", "Se procederá a calcular la cantidad de muestras para la etapa definitiva!", "OK");
                    await cambiarFase();    //Método para cambiar de fase actual
                    var calcularPage = new Estadisticas(currentFase);
                    await Navigation.PushAsync(calcularPage);
                    this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);
                }
                
            }
            else if (preliminar == 2)
            {
                var definitive = (int)Application.Current.Properties["definitive-done"];

                //if para saber si la etapa definitiva finalizó en la preliminar
                if (definitive == 1)
                {
                    //if para saber si ya se hizo la pregunta de si desea más muestras una vez llegada a la cantidad guardada
                    if (currentFase.extraFlag == 0)
                    {
                        bool answer = await DisplayAlert("Número de muestras alcanzada!", "Desea agregar más muestras a la fase definitiva?", "Sí", "No");
                        if (answer)
                        {
                            await PopupNavigation.Instance.PushAsync(new PopupMore(2));  //Se muestra popup de más muestras   
                            Application.Current.Properties["definitive-done"] = 0;  //Se cambia de nuevo el valor para que no esté finalizado
                            muestrasRestantesLabel.Text = "Muestras restantes: " + Application.Current.Properties["muestras-mas"];  //Se actualiza etiqueta
                            await ActualizarFlag(1);    //Método para actualizar que se hizo la pregunta
                        }
                        else
                        {
                            Application.Current.Properties["definitive-done"] = 2;  //Se cambia valor para indicar que se finaliza por completo la etapa
                            await terminarFaseDef();    //Termina la fase definitiva
                            var estadisticas = new EstadisticasGenerales();
                            await Navigation.PushAsync(estadisticas);
                            this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);
                        }
                    }
                    else
                    {
                        Application.Current.Properties["definitive-done"] = 2;  //Se cambia valor para indicar que se finaliza por completo la etapa
                        await DisplayAlert("Número de muestras alcanzada!", "Se procederá a mostrar las estadísticas finales!", "OK");
                        await terminarFaseDef();    //Termina la fase definitiva
                        var estadisticas = new EstadisticasGenerales();
                        await Navigation.PushAsync(estadisticas);
                        this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);
                    }
                }
                else if (definitive == 3)
                {
                    //caso en el que N sea menor o igual a n
                    Application.Current.Properties["definitive-done"] = 2;  //Se cambia valor para indicar que se finaliza por completo la etapa
                    await DisplayAlert("Número de muestras alcanzada!", "Se procederá a mostrar las estadísticas finales!", "OK");
                    await terminarFaseDef();    //Termina la fase definitiva
                    var estadisticas = new EstadisticasGenerales();
                    await Navigation.PushAsync(estadisticas);
                    this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);
                }
                
            }
            
        }


        public class fechasPicker
        {
            public int? id { get; set; }
            public string fecha { get; set; }
            public string token { get; set; }
        }

        //Método para obtener las fechas en los que se realizó al menos un recorrido
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
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/paths/dates.json", content);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadAsStringAsync();
                var jobject = JObject.Parse(result);

                try
                {
                    dates = JsonConvert.DeserializeObject<List<String>>(jobject["fechas"].ToString());
                    getPaths();
                    
                    //fechaPicker.Items.Clear();
                    ItemsDates = new ObservableCollection<string> { };

                    for (int i = 0; i < dates.Count; i++)
                    {
                        string temp = dates[i];
                        ItemsDates.Add(temp);
                    }


                    fechaPicker.ItemsSource = ItemsDates;
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

        //Método para obtener en la fase en la que se encuentra el muestreo
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

            try
            {
                HttpResponseMessage response = await client.PostAsync("/fases/getfasebyid.json", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(result);
                    var data = JsonConvert.DeserializeObject<Fase>(jobject["fase"].ToString());

                    currentFase = data;
                }

            }
            catch (Exception)
            {
                //Error al realizar consulta al backend
            }
        }

        //Método para obtener el muestreo actual
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
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(result);
                    var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());

                    muestrasRestantesLabel.Text = "Muestras restantes: " + (data.cantMuestrasTotal - data.muestrasActual);
                    muestrasActualesLabel.Text = "Muestras actuales: " + data.muestrasActual;
                }
            }
            catch (Exception)
            {
                //Error al realizar consulta al backend
            }
        }

        //Método para actualizar la flag de si se realizó la pregunta
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

            try
            {
                HttpResponseMessage response = await client.PostAsync("/fases/updateflag.json", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(result);
                    var data = JsonConvert.DeserializeObject<Fase>(jobject["fase"].ToString());
                    currentFase = data;
                }
            }
            catch (Exception)
            {
                //Error al realizar consulta al backend
            }
        }

        //Método para cambiar la fase en la tabla de muestreo(Sampling)
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
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("/samplings/changefase.json", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(result);
                    var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());
                    Application.Current.Properties["fase"] = 2;
                    await crearFaseDef();
                }
            }
            catch (Exception)
            {
                //Error al realizar consulta al backend
            }
        }

        //Método para crear la fase definitiva cuando se finalice la preliminar
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
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("/fases/changefase.json", content);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(result);
                    var data = JsonConvert.DeserializeObject<Fase>(jobject["fase"].ToString());
                    Application.Current.Properties["id-fase"] = data.id;

                }
            }
            catch (Exception)
            {
                //Error al realizar consulta al backend
            }
        }

        //Método para cambiar la fase definitiva de la tabla de muestreo(Sampling)
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
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("/samplings/changefase.json", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(result);
                    var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());
                    Application.Current.Properties["fase"] = 3;

                }
            }
            catch (Exception)
            {
                //Error al realizar consulta al backend
            }
        }

        //Método para obtener los recorridos de la fecha específica seleccionada del picker
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
                    id = (int)Application.Current.Properties["id-sampling"], //Le paso el id del muestreo y no del path porque es el que necesito 
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
                    var dateTime = DateTimeOffset.Parse(pro.hora, null);
                    string hora = dateTime.ToString("HH:mm:ss");
                    pro.hora = hora;
                    Items.Add(pro);
                }


                MyListView.ItemsSource = Items;

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            catch (Exception)
            {
                Console.WriteLine("\nException Caught!");
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