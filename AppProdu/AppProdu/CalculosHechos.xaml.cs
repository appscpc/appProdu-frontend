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
	public partial class CalculosHechos : ContentPage
	{
        int N, n;   //N guarda la cantidad de muestras en total por hacer y el n guarda la cantidad de muestras preliminares(hechas)
        Fase faseData = new Fase(); //Para recibir y manipular la fase actual y guardar los valores correspondientes(N, Z, error)

        //Constructor que recibe el N nuevo calculado, el n actual y la fase actual
        public CalculosHechos (int pN, Fase pFaseData, int pn)
		{
			InitializeComponent ();

            N = pN;
            n = pn;
            faseData = pFaseData;

            //Despliega los datos en la pantalla
            NEntry.Text = N.ToString();
            nEntry.Text = n.ToString();
            if ((N-n) < 0)
            {
                nDefEntry.Text = (0).ToString();
            }
            else
            {
                nDefEntry.Text = (N - n).ToString();
            }
        }

        //Método para regresar a la página anterior y repetir los cálculos
        private void back_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        //Método para aceptar los cálculos realizados. Guarda los valores y pasa a la siguiente fase
        private async Task aceptar_Clicked(object sender, EventArgs e)
        {
            //Caso en el que el nuevo N calculado sea menor al n ya realizado. Es decir, ya se hicieron las muestras necesarias
            if(n >= N)
            {
                Application.Current.Properties["definitive-done"] = 3;  //Guarda este valor como 3 para saber que ocurrió este caso
                await guardarErrorZ();  //Guarda los datos de la etapa preliminar. La etapa definitiva la guarda en la página Recorridos
                this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]); //Hace pop a la página anterior
                await Navigation.PopAsync(); //Hace pop a esta página
            }
            else
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://app-produ.herokuapp.com")
                };
                var newSampling = new Sampling
                {
                    id = (int)Application.Current.Properties["id-sampling"],
                    cantMuestras = 0,
                    cantMuestrasTotal = N-n,
                    token = Application.Current.Properties["currentToken"].ToString()
                };
                string jsonData = JsonConvert.SerializeObject(newSampling);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                try
                {
                    //Acá añade las nuevas muestras al registro en la base de datos
                    HttpResponseMessage response = await client.PostAsync("/samplings/addmoresamplings.json", content);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var jobject = JObject.Parse(result);
                        var data = JsonConvert.DeserializeObject<Sampling>(jobject["muestreo"].ToString());


                        try
                        {
                            await guardarErrorZ();

                            this.Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]); //Hace pop de la página anterior
                            await Navigation.PopAsync(); //Hace pop a esta página

                        }
                        catch (Exception)
                        {
                            //Error al realizar consulta al backend
                        }
                    }
                }
                catch(Exception)
                {
                    //Error al realizar consulta al backend
                }


            }
        }

        //Método que guarda el error y el z de alfa/2 elegido para los cálculos, tanto en la fase preliminar como en la nueva definitiva
        public async Task guardarErrorZ() 
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://app-produ.herokuapp.com")
            };
            var newFase = new Fase
            {
                id = faseData.id,
                error = faseData.error,
                z = faseData.z,
                token = Application.Current.Properties["currentToken"].ToString()
            };
            string jsonData = JsonConvert.SerializeObject(newFase);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                //Acá guarda el error y el Z de alfa/2 en la fase preliminar
                HttpResponseMessage response = await client.PostAsync("/fases/updateerrorz.json", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(result);
                    var data = JsonConvert.DeserializeObject<Fase>(jobject["fase"].ToString());
                    try
                    {
                        newFase.id = (int)Application.Current.Properties["id-fase"];
                        newFase.error = 0;
                        jsonData = JsonConvert.SerializeObject(newFase);
                        content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                        //Acá guarda el error y el Z de alfa/2 en la fase definitiva
                        response = await client.PostAsync("/fases/updateerrorz.json", content);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            result = await response.Content.ReadAsStringAsync();
                        }

                    }
                    catch (Exception)
                    {
                        //Error al realizar consulta al backend
                    }
                }
            }
            catch (Exception)
            {
                //Error al realizar consulta al backend
            }

            
        }
    }
}