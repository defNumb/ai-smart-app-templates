using System;
using System.Collections.Generic;

namespace DBTransferProject.Models
{
    public class MessageModel
    {
        public MetaData Meta { get; set; }
        public Links links { get; set; }
        public List<MessageData> Data { get; set; }

        public class MetaData
        {
            public int PageSize { get; set; }
            public int Page { get; set; }
        }

        public class Links
        {
            public string Self { get; set; }
            public string First { get; set; }
            public string Prev { get; set; }
            public string Next { get; set; }
        }

        public class MessageData
        {
            public string Id { get; set; }
            public MessageAttributes Attributes { get; set; }

            public class MessageAttributes
            {
                public string ExternalId { get; set; }
                public string Channel { get; set; }
                public string App { get; set; }
                public string Direction { get; set; }
                public string Preview { get; set; }
                public string Subject { get; set; }
                public DateTime SentAt { get; set; }
                public DateTime CreatedAt { get; set; }
                public DateTime UpdatedAt { get; set; }
                public MessageMeta Meta { get; set; }

                public class MessageMeta
                {
                    public string From { get; set; }
                }
            }
        }
    }
}
