using System;
using System.Collections.Generic;

namespace DBTransferProject.Models
{
    public class CustomerMessage
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Org { get; set; }
        public string Partition { get; set; }
        public MessageData Data { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Persist { get; set; }
        public string Client { get; set; }
        public string SourceId { get; set; }
        public string SourceType { get; set; }
        public string DataId { get; set; }
        public MessageMeta Meta { get; set; }
    }

    public class MessageData
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public MessageAttributes Attributes { get; set; }
        public MessageRelationships Relationships { get; set; }
    }

    public class MessageAttributes
    {
        public string ExternalId { get; set; }
        public string Channel { get; set; }
        public string App { get; set; }
        public int Size { get; set; }
        public string Direction { get; set; }
        public string DirectionType { get; set; }
        public string Preview { get; set; }
        public string Subject { get; set; }
        public MessageMeta Meta { get; set; }
        public string Status { get; set; }
        public bool Auto { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Redacted { get; set; }
    }

    public class MessageMeta
    {
        public string Subject { get; set; }
        public string From { get; set; }
        public List<Recipient> To { get; set; }
        public List<object> Cc { get; set; }
        public Recipient Recipient { get; set; }
    }

    public class Recipient
    {
        public string Email { get; set; }
    }

    public class MessageRelationships
    {
        public Relationship Org { get; set; }
        public Relationship Customer { get; set; }
        public ConversationRelationship Conversation { get; set; }
        public Attachments Attachments { get; set; }
    }

    public class Relationship
    {
        public Link Links { get; set; }
        public DataReference Data { get; set; }
    }

    public class ConversationRelationship
    {
        public Link Links { get; set; }
        public DataReference Data { get; set; }
    }

    public class Attachments
    {
        public Link Links { get; set; }
        public List<DataReference> Data { get; set; }
    }

    public class Link
    {
        public string Self { get; set; }
    }

    public class DataReference
    {
        public string Type { get; set; }
        public string Id { get; set; }
    }
}
