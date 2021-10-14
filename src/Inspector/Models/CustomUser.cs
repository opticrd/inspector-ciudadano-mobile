using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Zammad.Client.Resources;

namespace Inspector.Models
{
    public class CustomUser : User
    {
        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FirstName))
                    return string.Empty;

                var initials = FirstName.Substring(0, 1).ToUpper();
                initials += LastName.Substring(0, 1).ToUpper();

                return initials;
            } 
        }

        public static CustomUser Cast(User user)
        {
            var serialized = JsonConvert.SerializeObject(user);
            return JsonConvert.DeserializeObject<CustomUser>(serialized);
        }
    }
}
