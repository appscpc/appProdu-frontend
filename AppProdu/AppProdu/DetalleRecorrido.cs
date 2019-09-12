using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AppProdu
{
    public class DetalleRecorrido : ContentPage
    {
        Path pathSelected;
        List<OperatorRegister> listaOperarios = new List<OperatorRegister>();
        List<OperatorRegisterID> listaOperariosID = new List<OperatorRegisterID>();
        List<Actividad> listaActividades = new List<Actividad>();
        Dictionary<int, Actividad> dictAct = new Dictionary<int, Actividad>();
        float valorQ;
        float valorP;
        int faseActual = (int)Application.Current.Properties["fase"];

        public DetalleRecorrido(Path path)
        {
            pathSelected = path;

        }

        protected override async void OnAppearing()
        {
            await obtenerFaseAsync(faseActual); //Obtiene fase para p y q

            base.OnAppearing();
            this.Title = "Detalle Recorrido";
            await getRegisters();
            await getActivities();

            var contentView = new ContentView();
            var scrollView = new ScrollView();
            var grid = new Grid();
            var stack = new StackLayout();

            contentView.Content = scrollView;
            scrollView.Padding = new Thickness(10);
            scrollView.Content = grid;

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var cantLabel = new Label { Text = "Cantidad de Operarios: " + pathSelected.cantOperarios, HorizontalOptions = LayoutOptions.Center, FontSize = 18 };
            stack.Children.Add(cantLabel);
            var tempLabel = new Label { Text = "Temperatura: " + pathSelected.temperatura, HorizontalOptions = LayoutOptions.Center, FontSize = 18 };
            stack.Children.Add(tempLabel);
            var humLabel = new Label { Text = "Humedad: " + pathSelected.humedad, HorizontalOptions = LayoutOptions.Center, FontSize = 18 };
            stack.Children.Add(humLabel);
            var fechaLabel = new Label { Text = "Fecha: " + pathSelected.fecha, HorizontalOptions = LayoutOptions.Center, FontSize = 18 };
            stack.Children.Add(fechaLabel);
            var dateTime = DateTimeOffset.Parse(pathSelected.hora, null);
            string hora = dateTime.ToString("HH:mm:ss");
            var horaLabel = new Label { Text = "Hora: " + hora, HorizontalOptions = LayoutOptions.Center, FontSize = 18 };
            stack.Children.Add(horaLabel);
            var comLabel = new Label { Text = "Comentario:\n" + pathSelected.comentario, HorizontalOptions = LayoutOptions.Center, FontSize = 18 };
            stack.Children.Add(comLabel);
            var pLabel = new Label { Text = "Valor P: " + valorP, HorizontalOptions = LayoutOptions.Center, FontSize = 18 };
            stack.Children.Add(pLabel);
            var qLabel = new Label { Text = "Valor Q: " + valorQ, HorizontalOptions = LayoutOptions.Center, FontSize = 18 };
            stack.Children.Add(qLabel);

            grid.Children.Add(stack, 0, 0);
            Grid.SetColumnSpan(stack, 3);

            var Operarios = new Label { Text = "Operarios", HorizontalOptions = LayoutOptions.Center, FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#D37506") };
            var Actividad = new Label { Text = "Actividad", HorizontalOptions = LayoutOptions.Center, FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#D37506") };
            var Tipo = new Label { Text = "Tipo", HorizontalOptions = LayoutOptions.Center, FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#D37506") };

            grid.Children.Add(Operarios, 0, 1);
            grid.Children.Add(Actividad, 1, 1);
            grid.Children.Add(Tipo, 2, 1);
            for (int i = 1; i <= listaOperarios.Count; i++)
            {

                var Operarios1 = new Label { Text = "Operario " + i, HorizontalOptions = LayoutOptions.Center, FontSize = 16 };
                var Actividad1 = new Label { Text = dictAct[listaOperarios[i - 1].activity_id].nombre, HorizontalOptions = LayoutOptions.Center, FontSize = 16, TextColor = Color.FromHex("#9F2B00"), FontAttributes = FontAttributes.Bold };
                if (dictAct[listaOperarios[i - 1].activity_id].activity_type_id == 1)
                {
                    var Tipo1 = new Label { Text = "Productiva", HorizontalOptions = LayoutOptions.Center, FontSize = 16, TextColor = Color.FromHex("#D37506") };
                    grid.Children.Add(Tipo1, 2, i + 1);
                }
                else if (dictAct[listaOperarios[i - 1].activity_id].activity_type_id == 2)
                {
                    var Tipo1 = new Label { Text = "Colaborativa", HorizontalOptions = LayoutOptions.Center, FontSize = 16, TextColor = Color.FromHex("#D37506") };
                    grid.Children.Add(Tipo1, 2, i + 1);
                }
                else
                {
                    var Tipo1 = new Label { Text = "No productiva", HorizontalOptions = LayoutOptions.Center, FontSize = 16, TextColor = Color.FromHex("#D37506") };
                    grid.Children.Add(Tipo1, 2, i + 1);
                }

                grid.Children.Add(Operarios1, 0, i + 1);
                grid.Children.Add(Actividad1, 1, i + 1);

                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            }

            Content = contentView;
        }

        public async Task getRegisters()
        {
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://app-produ.herokuapp.com")
                };
                var fechaSelected = new Path
                {
                    id = pathSelected.id,
                    token = Application.Current.Properties["currentToken"].ToString()
                };
                string jsonData = JsonConvert.SerializeObject(fechaSelected);

                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("/operator_registers/pathoperatorregister.json", content);

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var jobject = JObject.Parse(responseBody);

                listaOperarios = JsonConvert.DeserializeObject<List<OperatorRegister>>(jobject["registro"].ToString());

                listaOperariosID = JsonConvert.DeserializeObject<List<OperatorRegisterID>>(jobject["registro"].ToString());

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            catch (Exception)
            {
            }
        }


        public async Task getActivities()
        {
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://app-produ.herokuapp.com")
                };

                var obj = new Dictionary<string, List<OperatorRegisterID>>();
                obj.Add("registers", listaOperariosID);
                string jsonData = JsonConvert.SerializeObject(obj);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/activities/getactivities.json", content);

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var jobject = JObject.Parse(responseBody);

                listaActividades = JsonConvert.DeserializeObject<List<Actividad>>(jobject["actividad"].ToString());

                dictAct = listaActividades.ToDictionary(x => x.id, x => x);

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            catch (Exception)
            {
            }
        }

        public class OperatorRegister
        {
            public int activity_id { get; set; }
            public int path_id { get; set; }
            public string nombre { get; set; }
            public int activity_type_id { get; set; }
            public string token { get; set; }
        }

        public class OperatorRegisterID
        {
            public int activity_id { get; set; }
        }

        public class Actividad
        {
            public int id { get; set; }
            public string nombre { get; set; }
            public int activity_type_id { get; set; }
            public int sampling_type_id { get; set; }
            public string token { get; set; }
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
                    Application.Current.Properties["id-fase"] = data[0].id;
                    valorP = data[0].p;
                    valorQ = data[0].q;

                }
            }
            catch (Exception)
            {
                //Error al realizar consulta al backend
            }
        }
    }
}