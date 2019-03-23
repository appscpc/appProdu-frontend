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
	public partial class CrearRecorrido : ContentPage
	{
		public CrearRecorrido ()
		{
			InitializeComponent ();
		}

        private void crearRec_Clicked(object sender, EventArgs e)
        {
            var agregarActPage = new AgregarActividad();
            Navigation.PushAsync(agregarActPage);
        }
    }
}