namespace ProfilesAPI.Options;

public class MessageDefferOptions
{
    public const string SectionName = "MessageDefferOptions";
    public TimeSpan DeferTime { get; set; } = TimeSpan.FromSeconds(30);
}
