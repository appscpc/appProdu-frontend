﻿using Newtonsoft.Json;
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


        public AgregarActividad ()
		{
			InitializeComponent ();
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
            public int activity_type { get; set; }
            public int sampling_type { get; set; }
            public string token { get; set; }
        }

        public class OperatorRegister
        {
            public int activity_id { get; set; }
            public int path_id { get; set; }
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
                sampling_type_id = (int)Application.Current.Properties["sampling-typ-id"],
                token = Application.Current.Properties["currentToken"].ToString()
            };
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
                    //string temp;
                    for (int i = 0; i < actividades.Count; i++)
                    {
                        Actividad pro = actividades[i];
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


            newAct = (Actividad)e.Item;
            Console.WriteLine("YESSSSS " + newAct.nombre);
            bool answer = await DisplayAlert("Desea agregar esta actividad al recorrido?", "La actividad será registrada en el recorrido actual", "Sí", "No");
            Console.WriteLine("Answer: " + answer);
            if (answer)
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://app-produ.herokuapp.com")
                };
                var newRegister = new OperatorRegister
                {
                    activity_id = newAct.id,
                    path_id = (int)Application.Current.Properties["id-path"],
                    token = Application.Current.Properties["currentToken"].ToString()
                };
                string jsonData = JsonConvert.SerializeObject(newRegister);
                Console.WriteLine("AQUI" + jsonData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/operator_registers/newregister.json", content);
                Console.WriteLine(response.StatusCode.ToString());
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Console.WriteLine("AQUI2");
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result.ToString());
                    //var jobject = JObject.Parse(result);
                    //Console.WriteLine("AQUI3" + jobject["colaborador"].ToString());
                    //colaboradores = JsonConvert.DeserializeObject<List<Colaborador>>(jobject["colaborador"].ToString());
                    //await Navigation.PopAsync();

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

        private async Task agregarActividad_Clicked(object sender, EventArgs e)
        {
            var recorridosPage = new Recorridos();
            await Navigation.PushAsync(recorridosPage);
        }
    }
}