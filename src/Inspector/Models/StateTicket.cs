using Inspector.Framework.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.Models
{
    public class StateTicket
    {
        public int State { get; set; }
        public string Name { get; set; }

        public static List<StateTicket> GetStatesForNewTicket()
        {
            return new List<StateTicket>()
            {
                new StateTicket() { Name = "Abierto", State = (int)TicketState.Open },
                new StateTicket() { Name = "En progreso", State = (int)TicketState.InProgress }
            };
        }
    }
}
