#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD00020_Control;

public class SettingsObject
{
    [JsonPropertyName("job")] public List<JobEntry> Job { get; set; }

    // コマンド名（"switch_on"など）をキーにして
    // bytes と comment を持つオブジェクトを格納する辞書
    [JsonExtensionData] public Dictionary<string, JsonElement> ExtensionData { get; set; }

    // コマンド辞書（bytesとcommentをパースして格納）
    [JsonIgnore] public Dictionary<string, CommandData> CommandMap { get; private set; }

    public void InitializeCommandMap()
    {
        CommandMap = new Dictionary<string, CommandData>();

        foreach (var kv in ExtensionData)
        {
            var key = kv.Key;

            // job は除外
            if (key == "job") continue;

            try
            {
                var commandData = kv.Value.Deserialize<CommandData>();
                if (commandData != null)
                {
                    CommandMap[key] = commandData;
                }
            }
            catch
            {
                // パース失敗時はスキップなど適宜対応
            }
        }
    }
}

public class CommandData
{
    [JsonPropertyName("bytes")] public List<string> Bytes { get; set; }

    [JsonPropertyName("comment")] public string Comment { get; set; }
}

public class JobEntry
{
    [JsonPropertyName("hh")] public string Hour { get; set; }

    [JsonPropertyName("command")] public string Command { get; set; }
}