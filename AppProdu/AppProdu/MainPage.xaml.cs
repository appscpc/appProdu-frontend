using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AppProdu
{
    public partial class MainPage : ContentPage
    {
        User userLogged = new User();
        public MainPage()
        {
            InitializeComponent();
            //imgMain.Source = ImageSource.FromResource("AppProdu.herramientas.jpg");

            var registrarLabel = this.FindByName<Label>("registrar");

            var registrarEvent = new TapGestureRecognizer();
            registrarEvent.Tapped += async(s, e) =>
            {
                var registrarPage = new RegistrarUsuario();
                await Navigation.PushAsync(registrarPage);
            };
            registrarLabel.GestureRecognizers.Add(registrarEvent);
            
            BindingContext = userLogged;
        }

        async private void ingresarButton_Clicked(object sender, EventArgs e)
        {

            if (!String.IsNullOrEmpty(correoEntry.Text) && !String.IsNullOrEmpty(passEntry.Text))
            {
                userLogged.correo = correoEntry.Text;
                userLogged.password = passEntry.Text;
                userLogged.correo = userLogged.correo.Trim();
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://app-produ.herokuapp.com")
                };
                
                string jsonData = JsonConvert.SerializeObject(userLogged);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                
                try
                {
                    HttpResponseMessage response = await client.PostAsync("/users/login.json", content);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var jobject = JObject.Parse(result);
                        var data = JsonConvert.DeserializeObject<List<User>>(jobject["usuario"].ToString());
                        Application.Current.Properties["id"] = data[0].id;
                        Application.Current.Properties["currentToken"] = data[0].token;
                        var proyectosPage = new Proyectos();
                        await Navigation.PushAsync(proyectosPage);
                    }
                    else
                    {
                        await DisplayAlert("Error!", "Usuario o contraseña inválidos!", "OK");
                    }

                }
                catch (Exception)
                {
                    await DisplayAlert("Error!", "Usuario o contraseña inválidos!", "OK");
                }

            }
            else
            {
                await DisplayAlert("Error!", "Espacios vacíos!\nPor favor rellenar todos los espacios!", "OK");
            }
        }

        protected override async void OnAppearing()
        {
            try
            {
                int idUserLogged = (int)Application.Current.Properties["id"];
                if (idUserLogged != 0)
                {
                    var proyectosPage = new Proyectos();
                    await Navigation.PushAsync(proyectosPage);
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
