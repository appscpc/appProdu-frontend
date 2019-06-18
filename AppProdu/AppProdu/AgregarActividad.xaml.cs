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
        ObservableCollection<Actividad> Items;  //Lista que guarda los elementos del listView
        List<Actividad> actividades;    //Lista que guarda las actividades recibidades del backend
        Actividad newAct = new Actividad(); //Actividad seleccionada para ser ingresada en la lista de operarios
        List<RegistroUsuario> listaDeOperarios; //Lista de operarios que sirve para poder ingresar las actividades y que se guarden en la página anterior
        int index;

        //Constructor que recibe un parámetro que es la lista de operarios. La recibe para guardar los cambios de las actividades
        //seleccionadas en la página anterior
        public AgregarActividad ( List<RegistroUsuario> listaOperarios)
		{
			InitializeComponent ();
            index = (int)Application.Current.Properties["index-activity"];  //Se obtiene el índice guardado en la memoria local de la app
            listaDeOperarios = listaOperarios;  //Se hace esta asignación para poder ingresar las actividades.

            //If para mostrar el mensajito de cuántos faltan o si ya se ingresaron todas las actividades.
            if (index >= listaDeOperarios.Count)
            {
                operarioActualLabel.Text = "Actividades asignadas a todos los operarios!";
            }
            else
            {
                operarioActualLabel.Text = "Asignar actividad al Operario " + (index + 1);
            }
        }

        //Clase que sirve para enviar el string del search bar y hacer la consulta
        public class Cadena
        {
            public string cadena { get; set; }
            public int sampling_type_id { get; set; }
            public string token { get; set; }
        }

        //Clase que contiene los atributos de una actividad para ser recibida del backend
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
            string jsonData = JsonConvert.SerializeObject(newCadena);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("/activities/findactivity.json", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(result);
                    actividades = JsonConvert.DeserializeObject<List<Actividad>>(jobject["actividad"].ToString());

                    Items = new ObservableCollection<Actividad> { };
                    for (int i = 0; i < actividades.Count; i++)
                    {
                        Actividad pro = actividades[i];
                        Items.Add(pro);
                    }

                    list.ItemsSource = Items;

                }
                else
                {
                    //La consulta devolvió un código de status distinto. Ocurrió un error en la consulta en el backend
                }


            }
            catch (Exception)
            {
                //Error al realizar consulta al backend
            }
            
        }

        //Método que se ejecuta al seleccionar algún elemento de la lista que contiene las actividades devueltas al buscarlas 
        //en el search bar
        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;


            newAct = (Actividad)e.Item; //e.Item es el objeto seleccionado de la lista. (La lista tiene objetos de tipo Actividad)
            bool answer = await DisplayAlert("Desea agregar esta actividad al recorrido?", "La actividad será registrada en el recorrido actual", "Sí", "No");
            if (answer)
            {
                //Este if es para saber si se tiene que agregar más actividades a la lista de operarios del recorrido
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
                    Application.Current.Properties["index-activity"] = index;   //Se guarda el índice en la memoria local de la app
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

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

    }
}