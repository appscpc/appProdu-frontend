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
    public partial class Muestreos : ContentPage
    {
        public ObservableCollection<string> Items { get; set; }

        public Muestreos()
        {
            InitializeComponent();

            Items = new ObservableCollection<string>
            {
                "Item 1",
                "Item 2",
                "Item 3",
                "Item 4",
                "Item 5"
            };
			
			MyListView.ItemsSource = Items;
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            //await DisplayAlert("Item Tapped", "An item was tapped.", "OK");
            var crearMuePage = new AgregarComentario();
            await Navigation.PushAsync(crearMuePage);


            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        async private void crearMuesButton_Clicked(object sender, EventArgs e)
        {
            var crearMuePage = new CrearMuestreo();
            await Navigation.PushAsync(crearMuePage);
        }
    }
}
