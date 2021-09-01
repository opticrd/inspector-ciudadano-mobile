using Inspector.Framework.Interfaces;
using Inspector.ViewModels;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Inspector.Views
{
    public partial class AddReportPage : ContentPage
    {
        AddReportPageViewModel _context;

        public AddReportPage()
        {            
             InitializeComponent();            
        }

        private void MaterialTextField_Unfocused(object sender, FocusEventArgs e)
        {
            if (_context == null)
                _context = (AddReportPageViewModel)BindingContext;

            _context.ValidateIDCommand.Execute(null);
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () => 
            {
                var result = await DisplayAlert("Confirmar cancelación", "¿Esta seguro que desea cancelar la creación de este caso?", "OK", "Cancelar");

                if (result) 
                    await this.Navigation.PopAsync(); 
            });

            return true;
        }

    }
}
