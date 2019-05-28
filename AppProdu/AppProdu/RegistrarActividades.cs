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
            //Your code here
            this.Title = "Registrar Actividades";
            index = (int)Application.Current.Properties["index-activity"];
            var contentView = new ContentView();
            var scrollView = new ScrollView();
            var grid = new Grid();

            contentView.Content = scrollView;
            scrollView.Padding = new Thickness(10);
            scrollView.Content = grid;

            //grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var Operarios = new Label { Text = "Operarios", HorizontalOptions = LayoutOptions.Center, FontSize = 16, FontAttributes = FontAttributes.Bold };
            var Actividad = new Label { Text = "Actividad", HorizontalOptions = LayoutOptions.Center, FontSize = 16, FontAttributes = FontAttributes.Bold };
            var Tipo = new Label { Text = "Tipo", HorizontalOptions = LayoutOptions.Center, FontSize = 16, FontAttributes = FontAttributes.Bold };


            grid.Children.Add(Operarios, 0, 0);
            grid.Children.Add(Actividad, 1, 0);
            grid.Children.Add(Tipo, 2, 0);
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
                
                var Operarios1 = new Label { Text = "Operario " + i, HorizontalOptions = LayoutOptions.Center, FontSize = 12 };
                var Actividad1 = new Label { Text = listaOperarios[i - 1].nombre_activity, HorizontalOptions = LayoutOptions.Center, FontSize = 12, TextColor = Color.Blue, FontAttributes = FontAttributes.Bold };
                var Tipo1 = new Label { Text = listaOperarios[i - 1].nombre_activity_type, HorizontalOptions = LayoutOptions.Center, FontSize = 12 };
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
            var buttonAgregar = new Button { Text = "Agregar Actividades", HorizontalOptions = LayoutOptions.Center, FontSize = 16, CornerRadius = 10, BackgroundColor = Color.FromHex("#439dbb"), TextColor = Color.White, HeightRequest = 45, WidthRequest = 200 };
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
                for (int i = 0; i < listaOperarios.Count; i++)
                {
                    listaRegistros.Add(new OperatorRegister
                    {
                        activity_id = listaOperarios[i].activity_id,
                        path_id = (int)Application.Current.Properties["id-path"],
                        token = Application.Current.Properties["currentToken"].ToString()
                    });
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
                obj.Add("registers", listaRegistros);
                string jsonData = JsonConvert.SerializeObject(obj);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/operator_registers/newregister.json", content);
                Console.WriteLine(response.StatusCode.ToString());
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Console.WriteLine("AQUI2");
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result.ToString());
                    await guardarProductividad();
                    await guardarMuestras();

                    var commentPage = new AgregarComentario();
                    await Navigation.PushAsync(commentPage);
                    this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);

                }
                else
                {
                    Console.WriteLine("AQUI6\nNo se pudo crear");
                    //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                }
            }
            else
            {
                await DisplayAlert("Error!", "Faltan actividades por registrar!\nPor favor asigne todas las actividades solicitadas!", "OK");
            }
            
        }

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
            Console.WriteLine("AQUI" + jsonData);
            Console.WriteLine("P: " + pG);
            Console.WriteLine("Q: " + qG);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/fases/addpq.json", content);
            Console.WriteLine(response.StatusCode.ToString());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("AQUI2");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result.ToString());

            }
            else
            {
                Console.WriteLine("AQUI6\nNo se pudo crear");
                //errorLabel.Text = "Error\nUsuario o contraseña inválido";
            }
        }

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
            Console.WriteLine("AQUI" + jsonData);
            Console.WriteLine("N: " + (pG + qG));
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/samplings/addsamplings.json", content);
            Console.WriteLine(response.StatusCode.ToString());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("AQUI2");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result.ToString());
                var samplingResult = JsonConvert.DeserializeObject<Sampling>(result);

                if(samplingResult.fase == 1)
                {
                    if(samplingResult.muestrasActual == samplingResult.cantMuestras)
                    {
                        Application.Current.Properties["preliminar-done"] = 1;
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
                        Console.WriteLine("AQUI" + jsonData);
                        content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                        response = await client.PostAsync("/samplings/addmoresamplings.json", content);
                        Console.WriteLine(response.StatusCode.ToString());
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            Console.WriteLine("AQUI2");
                            result = await response.Content.ReadAsStringAsync();
                            Console.WriteLine(result.ToString());
                            
                            try
                            {
                                var jobject = JObject.Parse(result);
                                Console.WriteLine("AQUI3" + jobject["muestreo"].ToString());
                                var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());
                                Application.Current.Properties["preliminar-done"] = 1;

                            }
                            catch (Exception)
                            {
                                Console.WriteLine("AQUI6\nNo se pudo crear el muestreo");
                                //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                            }
                        }
                    }

                }
                else
                {
                    if(samplingResult.muestrasActual == samplingResult.cantMuestrasTotal)
                    {
                        Application.Current.Properties["definitive-done"] = 1;
                    }else if (samplingResult.muestrasActual > samplingResult.cantMuestrasTotal)
                    {
                        newSampling = new Sampling
                        {
                            id = samplingResult.id,
                            cantMuestras = 0,
                            cantMuestrasTotal = samplingResult.muestrasActual - samplingResult.cantMuestrasTotal,
                            token = Application.Current.Properties["currentToken"].ToString()
                        };
                        jsonData = JsonConvert.SerializeObject(newSampling);
                        Console.WriteLine("AQUI" + jsonData);
                        content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                        response = await client.PostAsync("/samplings/addmoresamplings.json", content);
                        Console.WriteLine(response.StatusCode.ToString());
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            Console.WriteLine("AQUI2");
                            result = await response.Content.ReadAsStringAsync();
                            Console.WriteLine(result.ToString());

                            try
                            {
                                var jobject = JObject.Parse(result);
                                Console.WriteLine("AQUI3" + jobject["muestreo"].ToString());
                                var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());
                                Application.Current.Properties["definitive-done"] = 1;

                            }
                            catch (Exception)
                            {
                                Console.WriteLine("AQUI6\nNo se pudo crear el muestreo");
                                //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("AQUI6\nNo se pudo crear");
                //errorLabel.Text = "Error\nUsuario o contraseña inválido";
            }
        }
    }
}