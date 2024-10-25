namespace ChatComponents;

public class Message(Role role, string content, int order)
{
    public int Order { get; set; } = order;
    public string? Content { get; set; } = content;
    public Role Role { get; set; } = role;
    public DateTime TimeStamp { get; set; } = DateTime.Now;
    public bool IsActiveStreaming { get; set; }
    public string? ImageUrl { get; set; }
    //public bool IsModify { get; set; }

    public static Message UserMessage(string content, int order)
    {
        return new Message(Role.User, content, order);
    }
    public static Message UserMessage(string content, string imageUrl, int order)
    {
        return new Message(Role.User, content, order) { ImageUrl = imageUrl };
    }
    public static Message AssistantMessage(string content, int order)
    {
        return new Message(Role.Assistant, content, order);
    }

    public string CssClass => Role.ToString().ToLower();
}

public class FileUpload
{
    public string? FileBase64 { get; set; }
    public string? FileName { get; set; }
    public byte[] FileBytes { get; set; } = [];
}
public enum Role
{
    User, Assistant
}