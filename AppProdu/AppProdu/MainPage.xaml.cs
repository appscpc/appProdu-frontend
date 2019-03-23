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
        public MainPage()
        {
            InitializeComponent();

            var registrarLabel = this.FindByName<Label>("registrar");

            var registrarEvent = new TapGestureRecognizer();
            registrarEvent.Tapped += async(s, e) =>
            {
                var registrarPage = new RegistrarUsuario();
                await Navigation.PushAsync(registrarPage);
            };
            registrarLabel.GestureRecognizers.Add(registrarEvent);
        }

        async private void ingresarButton_Clicked(object sender, EventArgs e)
        {
            /*
            BackendRequest client = new BackendRequest();
            var url = new Uri("https://app-produ.herokuapp.com");
            string jsonData = @"{""correo"" : "+correo+", "+"password"+" : "+pass+"}";
            Console.WriteLine(correo+" "+pass);*/

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var userLogged = new User
            {
                correo = correoEntry.Text,
                password = passEntry.Text
            };
            string jsonData = JsonConvert.SerializeObject(userLogged);
            Console.WriteLine("AQUI");
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
      
            HttpResponseMessage response = await client.PostAsync("/users/login.json", content);
            Console.WriteLine(response.StatusCode.ToString());
            if(response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("AQUI2");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result.ToString());
                var jobject = JObject.Parse(result);
                Console.WriteLine("AQUI3" + jobject.ToString());
                var data = JsonConvert.DeserializeObject<List<User>>(jobject["usuario"].ToString());
                Console.WriteLine("AQUI4");

                try
                {
                    Console.WriteLine(data[0].id + " & " + data[0].nombre);

                    //var result = await client.Get<User>(url.ToString(), jsonData);

                    Application.Current.Properties["id"] = data[0].id;
                    Application.Current.Properties["currentToken"] = data[0].token;
                    var proyectosPage = new Proyectos();
                    await Navigation.PushAsync(proyectosPage); 
                    
                }
                catch (Exception)
                {
                    Console.WriteLine("AQUI6\nError al iniciar sesión!");
                    errorLabel.Text = "Error\nUsuario o contraseña inválido";
                }
            }
            else
            {
                Console.WriteLine("AQUI6\nError al iniciar sesión!");
                errorLabel.Text = "Error\nUsuario o contraseña inválido";
            }

        }
    }
}
