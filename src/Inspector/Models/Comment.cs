using System;
using System.Collections.Generic;
using System.Text;
using Zammad.Client.Resources;

namespace Inspector.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string UserName { get; set; }
        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(UserName))                
                    return string.Empty;
                
                string[] nameSplit = UserName.Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

                string initials = "";

                foreach (string item in nameSplit)
                {
                    initials += item.Substring(0, 1).ToUpper();
                }

                return initials;
            }
        }

        public string Body { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<TicketAttachment> Attachments { get; set; }
        public string ImageSource { get; set; }

        public bool IsOwner { get; set; }
        public bool HasAttachments => Attachments?.Count > 0;
    }
}
