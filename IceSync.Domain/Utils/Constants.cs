namespace IceSync.Domain.Utils;

public static class Constants
{
    public static class TokenManagement
    {
        public const string UniversalLoaderTokenKey = "UniLoad_Token";
    }

    public const string IdempotencyKeyHeader = "IdempotencyKey";

    public const int EFParameterLimit = 2_100;
}
