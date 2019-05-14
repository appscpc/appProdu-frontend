using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppProdu
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Estadisticas : ContentPage
	{
        Dictionary<string, string> zValues = new Dictionary<string, string>();
        Fase faseData = new Fase();

        public Estadisticas (Fase pFase)
		{
			InitializeComponent ();

            faseData = pFase;

            zValues.Add("1.65","0,90");
            zValues.Add("1.96", "0,95");
            zValues.Add("2.245", "0,975");
            zValues.Add("2.575", "0,99");
            foreach (string type in zValues.Values)
            {
                zPicker.Items.Add(type);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            pEntry.Text = (faseData.p / (faseData.p + faseData.q)).ToString();
            qEntry.Text = (faseData.q / (faseData.p + faseData.q)).ToString();
        }

        private async Task calcular_Clicked(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(errorEntry.Text) && !String.IsNullOrEmpty(zPicker.SelectedItem.ToString()))
            {
                double error;
                double.TryParse(errorEntry.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out error);
                Console.WriteLine("ERROR= " + error + " " + errorEntry.Text);
                if(error <= 0.1 && error >= 0.01)
                {
                    double p = (faseData.p / (faseData.p + faseData.q));
                    double q = (faseData.q / (faseData.p + faseData.q)); 
                    double z;
                    double.TryParse(zValues.FirstOrDefault(x => x.Value == zPicker.SelectedItem.ToString()).Key, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out z);
                    Console.WriteLine("HOLAAAAAAA " + z);
                    double n = Math.Pow(z, 2) * p * q / Math.Pow(error, 2);
                    Console.WriteLine("El N= " + n);

                    faseData.error = (float)error;
                    Console.WriteLine("KEVIN"+faseData.error);
                    faseData.z = (float)z;
                    var calculosPage = new CalculosHechos(System.Convert.ToInt32(Math.Round(n)), faseData, System.Convert.ToInt32(faseData.p + faseData.q));
                    await Navigation.PushAsync(calculosPage);
                }
                else
                {
                    Console.WriteLine("AQUI7\nError no entra en rango");
                    await DisplayAlert("Error!", "El error debe ser un número entre 0,01 y 0,1!", "OK");
                }
                
            }
            else
            {
                Console.WriteLine("AQUI6\nEspacios vacíos");
                await DisplayAlert("Error!", "Espacios vacíos!\nPor favor inserte el error y seleccione el Za/2!", "OK");
            }
        }
    }
}