using System;

namespace Backend.Database.Models
{

    public class StoredEvent
    {
        public int Id { get; set; }
        public NotificationEvent EventType { get; set; }
        public string Data { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Processed { get; set; } = false;
    }
}

