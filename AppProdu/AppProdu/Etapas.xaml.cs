using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppProdu
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Etapas : ContentPage
    {
        public ObservableCollection<string> Items { get; set; }

        public Etapas()
        {
            InitializeComponent();

            Items = new ObservableCollection<string>
            {
                "Preliminar",
                "Definitivo"
            };
			
			MyListView.ItemsSource = Items;
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            int faseActual = (int)Application.Current.Properties["fase"];
            Console.WriteLine("CERO");
            var faseSelected = (string)e.Item;
            Console.WriteLine("PRIMERO");
            if (faseActual == 1 && faseSelected.Equals("Preliminar"))
            {
                Console.WriteLine("SEGUNDO");
                var recorridosPage = new Recorridos();
                await Navigation.PushAsync(recorridosPage);
                

            }
            else if (faseActual == 2 && faseSelected.Equals("Definitivo"))
            {
                Console.WriteLine("TERCERO");
                var recorridosPage = new Recorridos();
                await Navigation.PushAsync(recorridosPage);
                
            }
            else
            {
                Console.WriteLine("NO HACE NADA!!!!!");
                await DisplayAlert("Error!", "Etapa Bloqueada.", "OK");
            }
            //await DisplayAlert("Item Tapped", "An item was tapped.", "OK");
            

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}
