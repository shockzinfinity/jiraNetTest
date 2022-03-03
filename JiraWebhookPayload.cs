using System.Text.Json.Serialization;

namespace jiraWebhookTest2
{
  public class Priority
  {
    public string self { get; set; }
    public string iconUrl { get; set; }
    public string name { get; set; }
    public string id { get; set; }
  }

  public class Fields
  {
    public string summary { get; set; }
    //public string created { get; set; }
    [JsonConverter(typeof(DateTimeOffsetJsonConverter))]
    public DateTimeOffset created { get; set; }
    public string description { get; set; }
    public List<string> labels { get; set; }
    public Priority priority { get; set; }
  }

  public class Issue
  {
    public string id { get; set; }
    public string self { get; set; }
    public string key { get; set; }
    public Fields fields { get; set; }
  }

  public class AvatarUrls
  {
    public string _16x16 { get; set; }
    public string _48x48 { get; set; }
  }

  public class User
  {
    public string self { get; set; }
    public string accoundId { get; set; }
    public string accountType { get; set; }
    public AvatarUrls avatarUrls { get; set; }
    public string displayName { get; set; }
    public bool active { get; set; }
    public string timeZone { get; set; }
  }

  public class Item
  {
    public string toString { get; set; }
    public string to { get; set; }
    public string fromString { get; set; }
    public string from { get; set; }
    public string fieldtype { get; set; }
    public string field { get; set; }
  }

  public class Changelog
  {
    public List<Item> items { get; set; }
    public int id { get; set; }
  }

  public class JiraWebhookPayload
  {
    public Issue issue { get; set; }
    public User user { get; set; }
    public Changelog changelog { get; set; }
    public long timestamp { get; set; }
    public string webhookEvent { get; set; }
    public string issue_event_type_name { get; set; }
  }
}
