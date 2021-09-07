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
                    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.Orange;
                    ((ShipState)bindable).shipState.Fill = Brush.Orange;
                    ((ShipState)bindable).shipStateText.Text = "Nuevo";
                    break;
                case (int)TicketState.Open:
                    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.LimeGreen;
                    ((ShipState)bindable).shipState.Fill = Brush.LimeGreen;
                    ((ShipState)bindable).shipStateText.Text = "Abierto";
                    break;
                case (int)TicketState.PendingReminder:
                    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.Crimson;
                    ((ShipState)bindable).shipState.Fill = Brush.Crimson;
                    ((ShipState)bindable).shipStateText.Text = "Pendiente Recordatorio";
                    break;
                case (int)TicketState.Closed:
                    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.DarkGray;
                    ((ShipState)bindable).shipState.Fill = Brush.DarkGray;
                    ((ShipState)bindable).shipStateText.Text = "Cerrado";
                    break;
                case (int)TicketState.Merged:
                    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.DarkOrchid;
                    ((ShipState)bindable).shipState.Fill = Brush.DarkOrchid;
                    ((ShipState)bindable).shipStateText.Text = "Fusionado";
                    break;
                case (int)TicketState.InProgress:
                    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.DodgerBlue;
                    ((ShipState)bindable).shipState.Fill = Brush.DodgerBlue;
                    ((ShipState)bindable).shipStateText.Text = "En progreso";
                    break;
                case (int)TicketState.PendingClose:
                    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.Firebrick;
                    ((ShipState)bindable).shipState.Fill = Brush.Firebrick;
                    ((ShipState)bindable).shipStateText.Text = "Pendiente Cerrar";
                    break;
                default:
                    ((ShipState)bindable).shipBackgroundColor.Fill = Brush.LightGray;
                    ((ShipState)bindable).shipState.Fill = Brush.LightGray;
                    ((ShipState)bindable).shipStateText.Text = "Indefinido";
                    break;
            }

            
        }
            
    }
}