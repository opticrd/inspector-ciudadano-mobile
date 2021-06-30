using Inspector.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Inspector.Framework.Helpers.Extensions
{
    [ContentProperty("Text")]
    public class TranslateExtension : IMarkupExtension
    {
        static string ResourceId = AppConfig.Instance.LabelsFile;
        private static ResourceManager resourceManager;        

        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null)
                return null;

            return GetValue(Text);
        }

        public static string GetValue(string text)
        {
            return GetResourceManager().GetString(text, CultureInfo.CurrentCulture);
        }

        private static ResourceManager GetResourceManager()
        {
            if (ReferenceEquals(resourceManager, null))
            {
                ResourceManager temp = new ResourceManager(ResourceId, typeof(TranslateExtension).Assembly);
                resourceManager = temp;
            }

            return resourceManager;
        }
    }
}
