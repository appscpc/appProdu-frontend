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

            var registrarLabel = this.FindByName<Label>("registrar");

            var registrarEvent = new TapGestureRecognizer();
            registrarEvent.Tapped += async(s, e) =>
            {
                var registrarPage = new RegistrarUsuario();
                await Navigation.PushAsync(registrarPage);/*
                var Page = new PruebaComaPunto();
                await Navigation.PushAsync(Page);*/
            };
            registrarLabel.GestureRecognizers.Add(registrarEvent);
            
            BindingContext = userLogged;
        }

        async private void ingresarButton_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine(userLogged.correo + " " + userLogged.password);

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
                Console.WriteLine("AQUI " + jsonData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                
                try
                {

                    HttpResponseMessage response = await client.PostAsync("/users/login.json", content);
                    //Console.WriteLine(response.StatusCode.ToString());
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine("AQUI2");
                        var result = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(result.ToString());
                        var jobject = JObject.Parse(result);
                        Console.WriteLine("AQUI3" + jobject.ToString());
                        var data = JsonConvert.DeserializeObject<List<User>>(jobject["usuario"].ToString());
                        Console.WriteLine("AQUI4");
                        Application.Current.Properties["id"] = data[0].id;
                        Application.Current.Properties["currentToken"] = data[0].token;
                        var proyectosPage = new Proyectos();
                        await Navigation.PushAsync(proyectosPage);


                    }
                    else
                    {
                        Console.WriteLine("AQUI6\nError al iniciar sesión!");
                        await DisplayAlert("Error!", "Usuario o contraseña inválidos!", "OK");
                    }

                }
                catch (Exception)
                {
                    Console.WriteLine("AQUI6\nError al iniciar sesión!");
                }

            }
            else
            {
                Console.WriteLine("AQUI6\nEspacios vacíos!");
                await DisplayAlert("Error!", "Espacios vacíos!\nPor favor rellenar todos los espacios!", "OK");
            }
        }

        protected override async void OnAppearing()
        {
            //Your code here
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
