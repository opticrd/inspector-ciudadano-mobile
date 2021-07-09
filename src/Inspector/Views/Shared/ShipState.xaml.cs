using Inspector.Framework.Dtos;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Inspector.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShipState : ContentView
    {
        public ShipState()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty StateProperty = BindableProperty
            .Create("State", typeof(int), typeof(ShipState), default(int?), BindingMode.OneWay, propertyChanged: HandleStateChanged);

        public int? State
        {
            get => (int)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        private static void HandleStateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue == null)
                return;

            switch ((int)newValue)
            {
                case (int)TicketState.New:
                    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.Blue;
                    ((ShipState)bindable).shipState.Fill = Brush.Blue;
                    ((ShipState)bindable).shipStateText.Text = "Nuevo";
                    break;
                case (int)TicketState.Open:
                    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.Green;
                    ((ShipState)bindable).shipState.Fill = Brush.Green;
                    ((ShipState)bindable).shipStateText.Text = "Abierto";
                    break;
                case (int)TicketState.PendingReminder:
                    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.Yellow;
                    ((ShipState)bindable).shipState.Fill = Brush.Yellow;
                    ((ShipState)bindable).shipStateText.Text = "Pendiente Recordatorio";
                    break;
                case (int)TicketState.Closed:
                    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.DarkRed;
                    ((ShipState)bindable).shipState.Fill = Brush.DarkRed;
                    ((ShipState)bindable).shipStateText.Text = "Cerrado";
                    break;
                case (int)TicketState.Merged:
                    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.Magenta;
                    ((ShipState)bindable).shipState.Fill = Brush.Magenta;
                    ((ShipState)bindable).shipStateText.Text = "Fusionado";
                    break;
                //case (int)TicketState.InProgress:
                //    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.Green;
                //    ((ShipState)bindable).shipState.Fill = Brush.Green;
                //    ((ShipState)bindable).shipStateText.Text = "En progreso";
                //    break;
                case (int)TicketState.PendingClose:
                    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.OrangeRed;
                    ((ShipState)bindable).shipState.Fill = Brush.OrangeRed;
                    ((ShipState)bindable).shipStateText.Text = "Pendiente Cerrar";
                    break;
                default:
                    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.Gray;
                    ((ShipState)bindable).shipState.Fill = Brush.Gray;
                    ((ShipState)bindable).shipStateText.Text = "Indefinido";
                    break;
            }

            
        }
            
    }
}