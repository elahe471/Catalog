

namespace Catalog.Contracts.IntegrationEvents;

public sealed record CatalogItemRemindMessageEvent(Guid UserId,string Slug,string Message,NotificationChannel Channel);
[Flags]
public enum NotificationChannel
{
    Email=1,
    Sms=2,
    PushNotification=3,
    Teams=4,
    Telegram=5,
}