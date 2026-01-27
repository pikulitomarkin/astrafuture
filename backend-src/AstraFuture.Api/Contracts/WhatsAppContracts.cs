namespace AstraFuture.Api.Contracts;

// Webhook do WhatsApp (Evolution API / Cloud API)
public record WhatsAppWebhookRequest
{
    public string Event { get; init; } = string.Empty;
    public string Instance { get; init; } = string.Empty;
    public WhatsAppMessageData Data { get; init; } = new();
}

public record WhatsAppMessageData
{
    public WhatsAppKey Key { get; init; } = new();
    public WhatsAppMessage Message { get; init; } = new();
    public string PushName { get; init; } = string.Empty;
    public long MessageTimestamp { get; init; }
}

public record WhatsAppKey
{
    public string RemoteJid { get; init; } = string.Empty;
    public bool FromMe { get; init; }
}

public record WhatsAppMessage
{
    public string? Conversation { get; init; }
    public string? ExtendedTextMessage { get; init; }
}

// Requisição simplificada para criar customer via WhatsApp
public record WhatsAppCreateCustomerRequest
{
    public string PhoneNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
}

// Requisição simplificada para criar appointment via WhatsApp
public record WhatsAppCreateAppointmentRequest
{
    public string CustomerPhone { get; init; } = string.Empty;
    public string? ResourceId { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string? Notes { get; init; }
}

// Response com webhook URL
public record WebhookUrlResponse
{
    public string WebhookUrl { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string Instructions { get; init; } = string.Empty;
}
