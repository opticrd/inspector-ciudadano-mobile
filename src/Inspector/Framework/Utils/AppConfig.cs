using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.Framework.Utils
{
    public sealed class AppConfig
    {
        private readonly static AppConfig _instance = new AppConfig();

        public static AppConfig Instance => _instance;

        public void Init()
        {
            LoadAppStyles();
        }

        void LoadAppStyles()
        {
#if RELEASE_AGENT || DEBUG_AGENT
            //Application.Current.Resources.MergedDictionaries.Add(new App1Style());
#else
            //Application.Current.Resources.MergedDictionaries.Add(new App1Style());
#endif
            //Application.Current.Resources.MergedDictionaries.Add(new GeneralStyle());
        }

        public string LabelsFile
        {
            get
            {
#if RELEASE_AGENT || DEBUG_AGENT
                return "Inspector.Resources.Labels.General";
#else
                return "Inspector.Resources.Labels.General";
#endif
            }
        }

        public string ImageSufixName
        {
            get
            {
#if RELEASE_AGENT || DEBUG_AGENT
                return "Agent";
#else
                return "User";
#endif
            }
        }

        public string ApiUrl
        {
            get
            {
#if RELEASE_AGENT || DEBUG_AGENT
                return "https:MyApi1";
#else
                return "https:MyApi2";
#endif
            }
        }
    }
}
