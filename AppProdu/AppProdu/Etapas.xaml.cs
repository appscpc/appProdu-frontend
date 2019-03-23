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

            //await DisplayAlert("Item Tapped", "An item was tapped.", "OK");
            var crearRecPage = new CrearRecorrido();
            await Navigation.PushAsync(crearRecPage);

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}
