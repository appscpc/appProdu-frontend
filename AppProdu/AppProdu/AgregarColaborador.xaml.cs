using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public partial class AgregarColaborador : ContentPage
    {
        ObservableCollection<User> Items;
        List<User> colaboradores;
        User newColab = new User();

        public AgregarColaborador()
        {
            InitializeComponent();
        }

        public class Cadena{
            public string cadena { get; set; }
            public string token { get; set; }
        }

        public class Colaborador
        {
            public int project_id { get; set; }
            public int user_id { get; set; }
            public string token { get; set; }
        }

        private async Task SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            //thats all you need to make a search  
            /*
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                list.ItemsSource = tempdata;
            }

            else
            {
                list.ItemsSource = tempdata.Where(x => x.Name.StartsWith(e.NewTextValue));
            }*/
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newCadena = new Cadena
            {
                cadena = buscar.Text,
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newCadena);
            Console.WriteLine("AQUI" + jsonData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/users/finduser.json", content);
            Console.WriteLine(response.StatusCode.ToString());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("AQUI2");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result.ToString());
                var jobject = JObject.Parse(result);
                Console.WriteLine("AQUI3" + jobject["usuario"].ToString());
                colaboradores = JsonConvert.DeserializeObject<List<User>>(jobject["usuario"].ToString());
                
                try
                {
                    Console.WriteLine("AQUI5");
                    Items = new ObservableCollection<User> { };
                    //string temp;
                    for (int i = 0; i < colaboradores.Count; i++)
                    {
                        User pro = colaboradores[i];
                        // temp = "";
                        //temp += pro.nombre + "\n\n" + pro.descripcion;
                        Items.Add(pro);
                        //Console.WriteLine(pro.nombre);
                    }
                    Console.WriteLine("HEY");

                    list.ItemsSource = Items;


                }
                catch (Exception)
                {
                    Console.WriteLine("AQUI6\nERROR");
                    //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                }
            }
            else
            {
                Console.WriteLine("AQUI6\nNo se pudo crear el proyecto");
                //errorLabel.Text = "Error\nUsuario o contraseña inválido";
            }
        }


        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            
            newColab = (User)e.Item;
            Console.WriteLine("YESSSSS " + newColab.nombre);
            bool answer = await DisplayAlert("Desea agregar este colaborador?", "El usuario "+ newColab.correo +" será un colaborador", "Sí", "No");
            Console.WriteLine("Answer: " + answer);
            if (answer)
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://app-produ.herokuapp.com")
                };
                var newColaborador = new Colaborador
                {
                    project_id = (int)Application.Current.Properties["id-project"],
                    user_id = newColab.id,
                    token = Application.Current.Properties["currentToken"].ToString()
                };
                string jsonData = JsonConvert.SerializeObject(newColaborador);
                Console.WriteLine("AQUI" + jsonData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/colaborators/addcolaborator.json", content);
                Console.WriteLine(response.StatusCode.ToString());
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Console.WriteLine("AQUI2");
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result.ToString());
                    //var jobject = JObject.Parse(result);
                    //Console.WriteLine("AQUI3" + jobject["colaborador"].ToString());
                    //colaboradores = JsonConvert.DeserializeObject<List<Colaborador>>(jobject["colaborador"].ToString());
                    await Navigation.PopAsync();

                }
                else
                {
                    Console.WriteLine("AQUI6\nNo se pudo crear");
                    //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                }
            }
            else
            {
                Console.WriteLine("NADA");
            }

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        private void agregarColab_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("HHH " + newColab.id);
            if(newColab.id != 0)
            {
                Console.WriteLine("ENTRA");
                /*var muestreosPage = new Muestreos();
                await Navigation.PushAsync(muestreosPage);*/
            }
            else
            {
                Console.WriteLine("NO ENTRA");
            }
        }
    }
}