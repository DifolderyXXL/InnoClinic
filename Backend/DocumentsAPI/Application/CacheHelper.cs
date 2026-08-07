namespace DocumentsAPI.Application;

public static class CacheHelper
{
    private const int FromHours = 60 * 60;
    public const int PublicCacheTime = 6 * FromHours;
    public const int PublicRestCacheTime = 5 * FromHours;

    public const int SensitiveCacheTime = 3 * FromHours;
    public const int SensitiveRestCacheTime = 2 * FromHours;
}