using Server.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Data
{
    public class HistoryResponse
    {
        public string Type { get; set; } = "";
        public List<PrivateMessage> Messages { get; set; } = new();
    }
}
