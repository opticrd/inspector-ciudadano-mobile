﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Inspector.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReportDetailPage : ContentPage
    {
        public ReportDetailPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {

                
            }
        }
    }
}