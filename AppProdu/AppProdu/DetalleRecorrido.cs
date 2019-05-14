using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

using Xamarin.Forms;

namespace AppProdu
{
    public class DetalleRecorrido : ContentPage
    {
        Path pathSelected;
        List<OperatorRegister> listaOperarios = new List<OperatorRegister>();

        public DetalleRecorrido(Path path)
        {
            pathSelected = path;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.Title = "Registrar Actividades";

            getRegisters();
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
            for (int i = 1; i <= listaOperarios.Count; i++)
            {

                var Operarios1 = new Label { Text = "Operario " + i, HorizontalOptions = LayoutOptions.Center, FontSize = 12 };
                //var Actividad1 = new Label { Text = listaOperarios[i - 1].nombre_activity, HorizontalOptions = LayoutOptions.Center, FontSize = 12, TextColor = Color.Blue, FontAttributes = FontAttributes.Bold };
                //var Tipo1 = new Label { Text = listaOperarios[i - 1].nombre_activity_type, HorizontalOptions = LayoutOptions.Center, FontSize = 12 };
                var registrarEvent = new TapGestureRecognizer();
                registrarEvent.Tapped += async (s, e) =>
                {
                    //var agregarPage = new AgregarActividad(listaOperarios);
                    //await Navigation.PushAsync(agregarPage);
                };
                //Actividad1.GestureRecognizers.Add(registrarEvent);

                grid.Children.Add(Operarios1, 0, i);
                //grid.Children.Add(Actividad1, 1, i);
                //grid.Children.Add(Tipo1, 2, i);
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            }
            var buttonAgregar = new Button { Text = "Agregar Actividades", HorizontalOptions = LayoutOptions.Center, FontSize = 16, CornerRadius = 10, BackgroundColor = Color.FromHex("#439dbb"), TextColor = Color.White, HeightRequest = 45, WidthRequest = 200 };
            //buttonAgregar.Clicked += async (sender, args) => await agregarActividad_Clicked();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            //grid.Children.Add(buttonAgregar, 0, totalOperarios + 1);
            Grid.SetColumnSpan(buttonAgregar, 3);

            Content = contentView;
        }

        async public void getRegisters()
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
                //Console.WriteLine("AQUI");
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/operator_registers/pathoperatorregister.json", content);

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var jobject = JObject.Parse(responseBody);
                Console.WriteLine("AQUI3" + jobject["registro"].ToString());
                listaOperarios = JsonConvert.DeserializeObject<List<OperatorRegister>>(jobject["registro"].ToString());


            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public class OperatorRegister
        {
            public int activity_id { get; set; }
            public int path_id { get; set; }
            public string token { get; set; }
        }
    }
}