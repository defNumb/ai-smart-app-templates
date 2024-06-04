using System;
using System.Collections.Generic;

namespace DBTransferProject.Models
{
    public class CustomerConversation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Org { get; set; }
        public string Partition { get; set; }
        public ConversationData Data { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Persist { get; set; }
        public string Client { get; set; }
        public string SourceId { get; set; }
        public string SourceType { get; set; }
        public string DataId { get; set; }
        public ConversationMeta Meta { get; set; }
    }

    public class ConversationData
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public ConversationAttributes Attributes { get; set; }
        public ConversationRelationships Relationships { get; set; }
    }

    public class ConversationAttributes
    {
        public string Name { get; set; }
        public List<string> Channels { get; set; }
        public string Status { get; set; }
        public ConversationOpen Open { get; set; }
        public int MessageCount { get; set; }
        public int NoteCount { get; set; }
        public int Satisfaction { get; set; }
        public SatisfactionLevel SatisfactionLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastActivityAt { get; set; }
        public bool Spam { get; set; }
        public bool Ended { get; set; }
        public object ImportedAt { get; set; }
        public List<object> Tags { get; set; }
        public List<object> SuggestedTags { get; set; }
        public List<object> Predictions { get; set; }
        public List<object> SuggestedShortcuts { get; set; }
        public FirstMessage FirstMessageIn { get; set; }
        public FirstMessage FirstMessageOut { get; set; }
        public LastMessage LastMessageIn { get; set; }
        public LastMessage LastMessageOut { get; set; }
        public LastMessage LastMessageUnrespondedTo { get; set; }
        public LastMessage LastMessageUnrespondedToSinceLastDone { get; set; }
        public List<string> AssignedUsers { get; set; }
        public List<object> AssignedTeams { get; set; }
        public Response FirstResponse { get; set; }
        public Response FirstResponseSinceLastDone { get; set; }
        public Response LastResponse { get; set; }
        public Response FirstDone { get; set; }
        public Response LastDone { get; set; }
        public string Direction { get; set; }
        public int OutboundMessageCount { get; set; }
        public int InboundMessageCount { get; set; }
        public int Rev { get; set; }
        public int Priority { get; set; }
        public List<object> RoleGroupVersions { get; set; }
        public List<object> AccessOverride { get; set; }
        public ModificationHistory ModificationHistory { get; set; }
        public Assistant Assistant { get; set; }
        public string Phase { get; set; }
        public List<object> MatchedTimeBasedRules { get; set; }
    }

    public class ConversationOpen
    {
        public DateTime StatusAt { get; set; }
    }

    public class SatisfactionLevel
    {
        public List<object> SentByTeams { get; set; }
        public List<object> Answers { get; set; }
    }

    public class FirstMessage
    {
        public List<object> CreatedByTeams { get; set; }
    }

    public class LastMessage
    {
        public List<object> CreatedByTeams { get; set; }
    }

    public class Response
    {
        public List<object> CreatedByTeams { get; set; }
        public List<object> AssignedTeams { get; set; }
        public List<string> AssignedUsers { get; set; }
    }

    public class ModificationHistory
    {
        public DateTime NameAt { get; set; }
        public DateTime PriorityAt { get; set; }
        public DateTime ChannelAt { get; set; }
        public DateTime AssignedTeamsAt { get; set; }
        public DateTime AssignedUsersAt { get; set; }
        public object BrandAt { get; set; }
        public object DefaultLangAt { get; set; }
        public DateTime StatusAt { get; set; }
        public object TagsAt { get; set; }
        public object CustomAt { get; set; }
    }

    public class Assistant
    {
        public Fac Fac { get; set; }
        public List<object> AssistantId { get; set; }
    }

    public class Fac
    {
        public List<object> Reasons { get; set; }
        public List<object> Exclusions { get; set; }
        public Source Source { get; set; }
    }

    public class Source
    {
    }

    public class ConversationRelationships
    {
        public Relationship Messages { get; set; }
        public Relationship Org { get; set; }
        public Relationship Customer { get; set; }
        public Relationship Brand { get; set; }
    }

    public class Data
    {
        public string Type { get; set; }
        public string Id { get; set; }
    }

    public class ConversationMeta
    {
        public bool CustomChanged { get; set; }
    }
}
