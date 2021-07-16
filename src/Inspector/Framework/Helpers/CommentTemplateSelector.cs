
using Inspector.Models;
using Xamarin.Forms;

namespace Inspector.Framework.Helpers
{
    public class CommentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate IncomingDataTemplate { get; set; }
        public DataTemplate OutgoingDataTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return ((Comment)item).IsOwner ? OutgoingDataTemplate : IncomingDataTemplate;
        }
    }
}
