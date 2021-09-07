using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.Framework.Dtos
{
    public enum TicketState
    {
        New = 1,
        Open = 2,
        PendingReminder = 3,
        Closed = 4,
        Merged = 5,
        InProgress = 6,
        PendingClose = 7,
        Indefine = 0
    }
}
