using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Models
{
    public class MessageDto
    {
        public string Sender { get; set; }

        public string Content { get; set; }

        public DateTime SentAt { get; set; }
    }
}
