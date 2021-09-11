
using Inspector.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Picker), typeof(CustomPickerRenderer))]
namespace Inspector.iOS.Renderers
{
    class CustomPickerRenderer : PickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);
            //var view = e.NewElement as Picker;

            if (Control != null)
            {
                Control.BorderStyle = UITextBorderStyle.None;
            }            
        }
    }
}