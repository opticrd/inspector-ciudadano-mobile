using System;
using System.Collections.Generic;
using System.Text;
using Zammad.Client.Resources;

namespace Inspector.Models
{
    public class Comment
    {
        public string UserName { get; set; }
        public string Body { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<TicketAttachment> Attachments { get; set; }

        public bool IsOwner { get; set; }
        public bool HasAttachments => Attachments?.Count > 0;
    }
}
