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
using Xamarin.Forms.Xaml;

namespace AppProdu
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RegistrarUsuario : ContentPage
	{
        Dictionary<int, string> userTypes = new Dictionary<int, string>();
        List<UserType> types;

        public RegistrarUsuario ()
		{
			InitializeComponent ();

            obtenerUserTypes();
            

            var iniciarLabel = this.FindByName<Label>("iniciar");

            var iniciarEvent = new TapGestureRecognizer();
            iniciarEvent.Tapped += async (s, e) =>
            {
                var iniciarPage = new MainPage();
                await Navigation.PushAsync(iniciarPage);
            };
            iniciarLabel.GestureRecognizers.Add(iniciarEvent);
        }

        async private void registrarButton_Clicked(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(nombreEntry.Text) && !String.IsNullOrEmpty(apellido1Entry.Text)&& !String.IsNullOrEmpty(apellido2Entry.Text) &&
                !String.IsNullOrEmpty(tipo.SelectedItem.ToString()) && !String.IsNullOrEmpty(correoEntry.Text) && !String.IsNullOrEmpty(passEntry.Text))
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://app-produ.herokuapp.com")
                };
                var idType = userTypes.FirstOrDefault(x => x.Value == tipo.SelectedItem.ToString()).Key;
                Console.WriteLine("HOLAAAAAAA " + idType);
                var newUser = new User
                {
                    nombre = nombreEntry.Text,
                    apellido1 = apellido1Entry.Text,
                    apellido2 = apellido2Entry.Text,
                    position_id = idType,
                    correo = correoEntry.Text,
                    password = passEntry.Text
                };

                string jsonData = JsonConvert.SerializeObject(newUser);
                Console.WriteLine("AQUI");
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("/users/signup.json", content);
                Console.WriteLine(response.StatusCode.ToString());
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Console.WriteLine("AQUI2");
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result.ToString());
                    var jobject = JObject.Parse(result);
                    Console.WriteLine("AQUI3" + jobject["usuario"].ToString());
                    var data = JsonConvert.DeserializeObject<User>(jobject["usuario"].ToString());

                    try
                    {
                        Console.WriteLine("AQUI5");
                        Console.WriteLine(data.id + " & " + data.nombre);

                        //var result = await client.Get<User>(url.ToString(), jsonData);

                        Application.Current.Properties["id"] = data.id;
                        Application.Current.Properties["currentToken"] = data.token;
                        var proyectosPage = new Proyectos();
                        await Navigation.PushAsync(proyectosPage);

                    }
                    catch (Exception)
                    {
                        Console.WriteLine("AQUI6\nError al registrar usuario!");
                        errorLabel.Text = "Error\nCorreo ya registrado";
                    }
                }
                else
                {
                    Console.WriteLine("AQUI6\nError al registrar usuario!");
                    errorLabel.Text = "Error\nCorreo ya registrado";
                }
            }
            else
            {
                Console.WriteLine("AQUI6\nEspacios vacíos");
                errorLabel.Text = "Error\nPor favor rellenar todos los campos!";
            }
        }

        async public void obtenerUserTypes()
        {
            var client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync("https://app-produ.herokuapp.com/positions.json");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                Console.WriteLine(responseBody);
                types = JsonConvert.DeserializeObject<List<UserType>>(responseBody);
                Console.WriteLine(types[0].nombre);

                userTypes = types.ToDictionary(m => m.id, m => m.nombre);


                foreach (string type in userTypes.Values)
                {
                    tipo.Items.Add(type);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }
    }
}