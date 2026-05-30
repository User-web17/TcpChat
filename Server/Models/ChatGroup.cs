using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Models
{
    public class ChatGroup
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<GroupMember> Members { get; set; } = [];
    }
}
