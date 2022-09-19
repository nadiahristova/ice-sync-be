namespace IceSync.Domain.Settings;

public record UniversalLoaderSettings : UrlSettings
{
    public string ApiCompanyId { get; init; } = null!;
    public string ApiUserId { get; init; } = null!;
    public string ApiUserSecret { get; init; } = null!;
}

