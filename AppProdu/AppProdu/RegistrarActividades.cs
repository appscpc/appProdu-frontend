using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace AppProdu
{
	public class RegistrarActividades : ContentPage
	{
		public RegistrarActividades ()
		{
			
            var grid = new Grid();

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(75) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(75) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var Operarios = new Label { Text = "Operarios", HorizontalOptions = LayoutOptions.Center, FontSize = 18, FontAttributes = FontAttributes.Bold };
            var Actividad = new Label { Text = "Actividad", HorizontalOptions = LayoutOptions.Center, FontSize = 18, FontAttributes = FontAttributes.Bold };
            var Tipo = new Label { Text = "Tipo" , HorizontalOptions = LayoutOptions.Center , FontSize = 18, FontAttributes = FontAttributes.Bold };


            grid.Children.Add(Operarios, 0, 0);
            grid.Children.Add(Actividad, 1, 0);
            grid.Children.Add(Tipo, 2, 0);

            for(int i = 1; i<10; i++)
            {
                var Operarios1 = new Label { Text = "Operario " + i, HorizontalOptions = LayoutOptions.Center, FontSize = 18};
                var Actividad1 = new Label { Text = "Agregar", HorizontalOptions = LayoutOptions.Center, FontSize = 18 };
                var Tipo1 = new Label { Text = "", HorizontalOptions = LayoutOptions.Center, FontSize = 18 };

                grid.Children.Add(Operarios1, 0, i);
                grid.Children.Add(Actividad1, 1, i);
                grid.Children.Add(Tipo1, 2, i);
            }

            Content = grid;

        }
	}
}