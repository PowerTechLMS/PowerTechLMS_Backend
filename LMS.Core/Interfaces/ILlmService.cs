namespace LMS.Core.Interfaces;

public interface ILlmService
{
    Task<string> GenerateResponseAsync(string systemPrompt, string userPrompt);
    Task<LlmChatResponse> ChatWithToolsAsync(List<LlmChatMessage> messages, List<LlmTool>? tools = null);
}

public class LlmChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string? Content { get; set; }
    public List<LlmToolCall>? ToolCalls { get; set; }
    public string? ToolCallId { get; set; }
}

public class LlmChatResponse
{
    public string? Message { get; set; }
    public List<LlmToolCall>? ToolCalls { get; set; }
}

public class LlmToolCall
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
}

public class LlmTool
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object Parameters { get; set; } = new();
}
