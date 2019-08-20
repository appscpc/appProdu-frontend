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

namespace AppProdu
{
	public class RegistrarActividades : ContentPage
	{
        List<RegistroUsuario> listaOperarios = new List<RegistroUsuario>();
        int index = 0;
        int totalOperarios;
        int pG = 0;
        int qG = 0;
        

		public RegistrarActividades (int cantOperarios)
		{
            NavigationPage.SetHasBackButton(this, false);
            totalOperarios = cantOperarios;
            Application.Current.Properties["index-activity"] = index;
            
        }

        protected override void OnAppearing()
        {
            this.Title = "Registrar Actividades";
            index = (int)Application.Current.Properties["index-activity"];
            var contentView = new ContentView();
            var scrollView = new ScrollView();
            var grid = new Grid();

            contentView.Content = scrollView;
            scrollView.Padding = new Thickness(10);
            scrollView.Content = grid;

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var Operarios = new Label { Text = "Operarios", HorizontalOptions = LayoutOptions.Center, FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor= Color.FromHex("#D37506") };
            var Actividad = new Label { Text = "Actividad", HorizontalOptions = LayoutOptions.Center, FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#D37506") };
            var Tipo = new Label { Text = "Tipo", HorizontalOptions = LayoutOptions.Center, FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#D37506") };


            grid.Children.Add(Operarios, 0, 0);
            grid.Children.Add(Actividad, 1, 0);
            grid.Children.Add(Tipo, 2, 0);
            //for para generar las etiquetas de los operarios y actividades
            for (int i = 1; i <= totalOperarios; i++)
            {
                if(listaOperarios.Count != totalOperarios)
                {
                    listaOperarios.Add(new RegistroUsuario
                    {
                        path_id = (int)Application.Current.Properties["id-path"],
                        activity_id = 0,
                        nombre_activity = "Agregar",
                        activity_type_id = 0,
                        nombre_activity_type = "",
                        token = Application.Current.Properties["currentToken"].ToString()
                    });
                }
                
                var Operarios1 = new Label { Text = "Operario " + i, HorizontalOptions = LayoutOptions.Center, FontSize = 16 };
                var Actividad1 = new Label { Text = listaOperarios[i - 1].nombre_activity, HorizontalOptions = LayoutOptions.Center, FontSize = 16, TextColor = Color.FromHex("#9F2B00"), FontAttributes = FontAttributes.Bold };
                var Tipo1 = new Label { Text = listaOperarios[i - 1].nombre_activity_type, HorizontalOptions = LayoutOptions.Center, FontSize = 16, TextColor = Color.FromHex("#D37506") };
                var registrarEvent = new TapGestureRecognizer();
                registrarEvent.Tapped += async (s, e) =>
                {
                    var agregarPage = new AgregarActividad(listaOperarios);
                    await Navigation.PushAsync(agregarPage);
                };
                Actividad1.GestureRecognizers.Add(registrarEvent);
                
                grid.Children.Add(Operarios1, 0, i);
                grid.Children.Add(Actividad1, 1, i);
                grid.Children.Add(Tipo1, 2, i);
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            }
            var buttonAgregar = new Button { Text = "Agregar Actividades", HorizontalOptions = LayoutOptions.Center, FontSize = 18, CornerRadius = 10, BackgroundColor = Color.FromHex("#D37506"), TextColor = Color.White, HeightRequest = 45, WidthRequest = 200 };
            buttonAgregar.Clicked += async (sender, args) => await agregarActividad_Clicked();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.Children.Add(buttonAgregar, 0, totalOperarios+1);
            Grid.SetColumnSpan(buttonAgregar, 3);

            Content = contentView;
        }

        public class OperatorRegister
        {
            public int activity_id { get; set; }
            public int path_id { get; set; }
            public string token { get; set; }
        }


        private async Task agregarActividad_Clicked()
        {
            if (index >= listaOperarios.Count)
            {
                pG = 0;
                qG = 0;
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://app-produ.herokuapp.com")
                };
                List<OperatorRegister> listaRegistros = new List<OperatorRegister>();
                //for para pasar la lista de las actividades de los operarios a la de actividades que se guardan en el backend
                for (int i = 0; i < listaOperarios.Count; i++)
                {
                    listaRegistros.Add(new OperatorRegister
                    {
                        activity_id = listaOperarios[i].activity_id,
                        path_id = (int)Application.Current.Properties["id-path"],
                        token = Application.Current.Properties["currentToken"].ToString()
                    });
                    //Se lleva la cuenta de las actividades productivas y las no productivas
                    if (listaOperarios[i].activity_type_id == 1)
                    {
                        pG = pG + 1;
                    }
                    else
                    {
                        qG = qG + 1;
                    }
                }
                var obj = new Dictionary<string, List<OperatorRegister>>();
                //Se crea un diccionario para convertirlo a json y poder acceder a la lista de registros en el backend
                obj.Add("registers", listaRegistros);
                string jsonData = JsonConvert.SerializeObject(obj);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/operator_registers/newregister.json", content);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    await guardarProductividad();
                    await guardarMuestras();

                    var commentPage = new AgregarComentario();
                    await Navigation.PushAsync(commentPage);
                    this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);

                }
            }
            else
            {
                await DisplayAlert("Error!", "Faltan actividades por registrar!\nPor favor asigne todas las actividades solicitadas!", "OK");
            }
            
        }

        //Método que guarda la productividad (P) y la improductividad (Q) en el backend
        public async Task guardarProductividad()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newFase = new Fase
            {
                id = (int)Application.Current.Properties["id-fase"],
                p = pG,
                q = qG,
                error = 0,
                z = 0,
                fase_type_id = 0,
                sampling_id = 0,
                extraFlag = 0,
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newFase);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("/fases/addpq.json", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception)
            {
                //Error al realizar consulta al backend
            }
        }
        
        //Método que guarda la cantidad de muestras hechas 
        public async Task guardarMuestras()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newSampling = new Sampling
            {
                id = (int)Application.Current.Properties["id-sampling"],
                muestrasActual = pG + qG,
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newSampling);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/samplings/addsamplings.json", content);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadAsStringAsync();
                var samplingResult = JsonConvert.DeserializeObject<Sampling>(result);


                //if para saber en cuál fase está
                if(samplingResult.fase == 1)
                {
                    //if para saber si se llegó a la cantidad o si se sobrepasó. Si se sobrepasa se debe actualizar la cantidad en la base de datos
                    if(samplingResult.muestrasActual == samplingResult.cantMuestras)
                    {
                        Application.Current.Properties["preliminar-done"] = 1;  //Valor para saber si se finalizó la etapa preliminar
                    }
                    else if (samplingResult.muestrasActual > samplingResult.cantMuestras)
                    {
                        newSampling = new Sampling
                        {
                            id = samplingResult.id,
                            cantMuestras = samplingResult.muestrasActual - samplingResult.cantMuestras,
                            cantMuestrasTotal = samplingResult.muestrasActual - samplingResult.cantMuestras,
                            token = Application.Current.Properties["currentToken"].ToString()
                        };
                        jsonData = JsonConvert.SerializeObject(newSampling);
                        content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                        response = await client.PostAsync("/samplings/addmoresamplings.json", content);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            result = await response.Content.ReadAsStringAsync();

                            try
                            {
                                var jobject = JObject.Parse(result);
                                var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());
                                Application.Current.Properties["preliminar-done"] = 1;

                            }
                            catch (Exception)
                            {
                            }
                        }
                    }

                }
                else
                {
                    //if para saber si se llegó a la cantidad o si se sobrepasó. Si se sobrepasa se debe actualizar la cantidad en la base de datos
                    if (samplingResult.muestrasActual == samplingResult.cantMuestrasTotal)
                    {
                        Application.Current.Properties["definitive-done"] = 1;  //Valor para saber si se finalizó la etapa preliminar
                    }
                    else if (samplingResult.muestrasActual > samplingResult.cantMuestrasTotal)
                    {
                        newSampling = new Sampling
                        {
                            id = samplingResult.id,
                            cantMuestras = 0,
                            cantMuestrasTotal = samplingResult.muestrasActual - samplingResult.cantMuestrasTotal,
                            token = Application.Current.Properties["currentToken"].ToString()
                        };
                        jsonData = JsonConvert.SerializeObject(newSampling);
                        content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                        response = await client.PostAsync("/samplings/addmoresamplings.json", content);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            result = await response.Content.ReadAsStringAsync();

                            try
                            {
                                var jobject = JObject.Parse(result);
                                var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());
                                Application.Current.Properties["definitive-done"] = 1;

                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
        }
    }
}