using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Services;
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
	public partial class PopupMore
	{
        int etapa;

		public PopupMore (int pEtapa)
		{
			InitializeComponent ();

            etapa = pEtapa;
		}

        private async Task agregarMuestras_Clicked(object sender, EventArgs e)
        {
            if(etapa == 1)
            {
                if (!String.IsNullOrEmpty(masMuestrasEntry.Text))
                {
                    var client = new HttpClient
                    {
                        BaseAddress = new Uri("https://app-produ.herokuapp.com")
                    };
                    var newSampling = new Sampling
                    {
                        id = (int)Application.Current.Properties["id-sampling"],
                        cantMuestras = System.Convert.ToInt32(masMuestrasEntry.Text),
                        cantMuestrasTotal = System.Convert.ToInt32(masMuestrasEntry.Text),
                        token = Application.Current.Properties["currentToken"].ToString()
                    };
                    string jsonData = JsonConvert.SerializeObject(newSampling);
                    Console.WriteLine("AQUI" + jsonData);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("/samplings/addmoresamplings.json", content);
                    Console.WriteLine(response.StatusCode.ToString());
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine("AQUI2");
                        var result = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(result.ToString());
                        var jobject = JObject.Parse(result);
                        Console.WriteLine("AQUI3" + jobject["muestreo"].ToString());
                        var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());


                        try
                        {
                            Console.WriteLine("AQUI5");
                            Console.WriteLine(data.id + " & " + data.nombre);

                            Application.Current.Properties["muestras-mas"] = System.Convert.ToInt32(masMuestrasEntry.Text);

                            var recorridosPage = new Recorridos(true);
                            await Navigation.PushAsync(recorridosPage);

                            await PopupNavigation.Instance.PopAsync(true);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("AQUI6\nNo se pudo crear el muestreo");
                            //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                        }
                    }
                    else
                    {
                        Console.WriteLine("AQUI6\nNo se pudo crear el muestreo");
                        //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                    }
                }
                else
                {
                    Console.WriteLine("AQUI6\nEspacios vacíos");
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(masMuestrasEntry.Text))
                {
                    var client = new HttpClient
                    {
                        BaseAddress = new Uri("https://app-produ.herokuapp.com")
                    };
                    var newSampling = new Sampling
                    {
                        id = (int)Application.Current.Properties["id-sampling"],
                        cantMuestras = 0,
                        cantMuestrasTotal = System.Convert.ToInt32(masMuestrasEntry.Text),
                        token = Application.Current.Properties["currentToken"].ToString()
                    };
                    string jsonData = JsonConvert.SerializeObject(newSampling);
                    Console.WriteLine("AQUI" + jsonData);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("/samplings/addmoresamplings.json", content);
                    Console.WriteLine(response.StatusCode.ToString());
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine("AQUI2");
                        var result = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(result.ToString());
                        var jobject = JObject.Parse(result);
                        Console.WriteLine("AQUI3" + jobject["muestreo"].ToString());
                        var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());


                        try
                        {
                            Console.WriteLine("AQUI5");
                            Console.WriteLine(data.id + " & " + data.nombre);

                            Application.Current.Properties["muestras-mas"] = System.Convert.ToInt32(masMuestrasEntry.Text);
                            
                            await PopupNavigation.Instance.PopAsync(true);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("AQUI6\nNo se pudo crear el muestreo");
                            //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                        }
                    }
                    else
                    {
                        Console.WriteLine("AQUI6\nNo se pudo crear el muestreo");
                        //errorLabel.Text = "Error\nUsuario o contraseña inválido";
                    }
                }
                else
                {
                    Console.WriteLine("AQUI6\nEspacios vacíos");
                }
            }
        }
    }
}