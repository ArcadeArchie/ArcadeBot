using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArcadeBot.DTO
{
    public class Relationship
    {        
        [JsonPropertyName("id")]
        public ulong Id { get; set; }
        [JsonPropertyName("user")]
        public User User { get; set; }
        [JsonPropertyName("type")]
        public RelationshipType Type { get; set; }
    }
}