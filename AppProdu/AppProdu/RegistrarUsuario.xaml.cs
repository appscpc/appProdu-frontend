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
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("/users/signup.json", content);

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(result);
                    var data = JsonConvert.DeserializeObject<User>(jobject["usuario"].ToString());

                    try
                    {
                        Application.Current.Properties["id"] = data.id;
                        Application.Current.Properties["currentToken"] = data.token;
                        await DisplayAlert("El usuario ha sido creado con éxito!", "Se desplegará a la pantalla de proyectos del usuario!", "OK");
                        var proyectosPage = new Proyectos();
                        await Navigation.PushAsync(proyectosPage);

                    }
                    catch (Exception)
                    {
                        
                    }
                }
                else
                {
                    await DisplayAlert("Error!", "Correo ya está registrado!\nPor favor utilice otro!", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error!", "Espacios vacíos!\nPor favor rellenar todos los espacios!", "OK");
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

                types = JsonConvert.DeserializeObject<List<UserType>>(responseBody);
                userTypes = types.ToDictionary(m => m.id, m => m.nombre);

                
                foreach (string type in userTypes.Values)
                {
                    tipo.Items.Add(type);
                }
            }
            catch (HttpRequestException e)
            {
            }
        }
    }
}