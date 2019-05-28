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
	public partial class AgregarActividad : ContentPage
	{
        ObservableCollection<Actividad> Items;
        List<Actividad> actividades;
        Actividad newAct = new Actividad();
        List<RegistroUsuario> listaDeOperarios;
        int index;


        public AgregarActividad ( List<RegistroUsuario> listaOperarios)
		{
			InitializeComponent ();
            index = (int)Application.Current.Properties["index-activity"];
            Console.WriteLine("HELLO MOTO: ", index);
            listaDeOperarios = listaOperarios;
            if (index >= listaDeOperarios.Count)
            {
                operarioActualLabel.Text = "Actividades asignadas a todos los operarios!";
            }
            else
            {
                operarioActualLabel.Text = "Asignar actividad al Operario " + (index + 1);
            }
        }

        public class Cadena
        {
            public string cadena { get; set; }
            public int sampling_type_id { get; set; }
            public string token { get; set; }
        }

        public class Actividad
        {
            public int id { get; set; }
            public string nombre { get; set; }
            public int activity_type_id { get; set; }
            public int sampling_type_id { get; set; }
            public string token { get; set; }
        }


        private async Task SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newCadena = new Cadena
            {
                cadena = buscar.Text,
                sampling_type_id = (int)Application.Current.Properties["sampling-type-id"],
                token = Application.Current.Properties["currentToken"].ToString()
            };
            Console.WriteLine("ID SAMPLING: " + newCadena.sampling_type_id);
            string jsonData = JsonConvert.SerializeObject(newCadena);
            Console.WriteLine("AQUI" + jsonData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("/activities/findactivity.json", content);
            Console.WriteLine(response.StatusCode.ToString());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("AQUI2");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result.ToString());
                var jobject = JObject.Parse(result);
                Console.WriteLine("AQUI3" + jobject["actividad"].ToString());
                actividades = JsonConvert.DeserializeObject<List<Actividad>>(jobject["actividad"].ToString());

                try
                {
                    Console.WriteLine("AQUI5");
                    Items = new ObservableCollection<Actividad> { };
                    for (int i = 0; i < actividades.Count; i++)
                    {
                        Actividad pro = actividades[i];
                        Items.Add(pro);
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


            newAct = (Actividad)e.Item;
            Console.WriteLine("YESSSSS " + newAct.nombre);
            bool answer = await DisplayAlert("Desea agregar esta actividad al recorrido?", "La actividad será registrada en el recorrido actual", "Sí", "No");
            Console.WriteLine("Answer: " + answer);
            if (answer)
            {
                Console.WriteLine("CANT: " + listaDeOperarios.Count + " & " + index);
                if (index >= listaDeOperarios.Count)
                {
                    await DisplayAlert("Error!", "Ya se agregaron todas las actividades!", "OK");
                }
                else
                {
                    listaDeOperarios[index].activity_id = newAct.id;
                    listaDeOperarios[index].nombre_activity = newAct.nombre;
                    listaDeOperarios[index].activity_type_id = newAct.activity_type_id;
                    if (listaDeOperarios[index].activity_type_id == 1)
                    {
                        listaDeOperarios[index].nombre_activity_type = "Trabajo productivo";
                    }
                    else if (listaDeOperarios[index].activity_type_id == 2)
                    {
                        listaDeOperarios[index].nombre_activity_type = "Trabajo contributivo";
                    }
                    else if (listaDeOperarios[index].activity_type_id == 3)
                    {
                        listaDeOperarios[index].nombre_activity_type = "Trabajo no productivo";
                    }
                    index = index + 1;
                    Application.Current.Properties["index-activity"] = index;
                    if (index >= listaDeOperarios.Count)
                    {
                        operarioActualLabel.Text = "Actividades asignadas a todos los operarios!";
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        operarioActualLabel.Text = "Asignar actividad al Operario " + (index + 1);
                    }
                }
            }
            else
            {
                Console.WriteLine("NADA");
            }

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        private async Task agregarActividad_Clicked(object sender, EventArgs e)
        {
            var recorridosPage = new Recorridos();
            await Navigation.PushAsync(recorridosPage);
        }
    }
}