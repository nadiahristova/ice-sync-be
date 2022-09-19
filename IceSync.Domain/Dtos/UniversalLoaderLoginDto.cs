namespace IceSync.Domain.Dtos;

/// <summary>
/// Creadntials needed for Universal Loader Api authentication
/// </summary>
/// <param name="ApiCompanyId"></param>
/// <param name="ApiUserId"></param>
/// <param name="ApiUserSecret"></param>
public record UniversalLoaderLoginDto(string ApiCompanyId, string ApiUserId, string ApiUserSecret);
