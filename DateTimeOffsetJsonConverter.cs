using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace jiraWebhookTest2
{
  public class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
  {
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => DateTimeOffset.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:sss", CultureInfo.InvariantCulture));
  }
}
