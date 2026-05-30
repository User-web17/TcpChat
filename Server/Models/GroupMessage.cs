using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Models
{
    public class GroupMessage
    {
        public int Id { get; set; }

        public int GroupId { get; set; }

        public int SenderId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
