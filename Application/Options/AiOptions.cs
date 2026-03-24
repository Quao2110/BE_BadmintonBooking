namespace Application.Options;

public class AiOptions
{
    /// <summary>
    /// Enable automatic AI replies when admin hasn't responded
    /// </summary>
    public bool EnableAutoReply { get; set; } = true;

    /// <summary>
    /// Time in minutes to wait for admin reply before AI replies
    /// </summary>
    public int AdminReplyTimeoutMinutes { get; set; } = 1;

    /// <summary>
    /// Topics the AI should handle (simple filter). Example: ["badminton"]
    /// </summary>
    public string[] Topics { get; set; } = new[] { "badminton" };
}
