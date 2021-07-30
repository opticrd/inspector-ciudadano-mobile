using Inspector.ViewModels;
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
    }
}
