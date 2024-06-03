using Microsoft.Extensions.Caching.Distributed;

namespace ClassLibrary1.Cads;

public class CadLocator: ICadLocator
{
    private readonly IDistributedCache _cache;

    public CadLocator(IDistributedCache cache)
    {
        _cache = cache;
    }

    private static string BuildCadKey(string organizationId, string prcId)
    {
        var cadKey = $"CadHandlerProxy-{organizationId}-{prcId}";
        return cadKey;
    }

    public async Task<string?> GetCadNode(string organizationId, string prcId)
    {
        return await _cache.GetStringAsync(BuildCadKey(organizationId, prcId));
    }

    public async Task SetCadNode(string organizationId, string prcId, string nodeUrl)
    {
        await _cache.SetStringAsync(BuildCadKey(organizationId, prcId), nodeUrl, options: new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        });
    }

    public async Task DeleteCadInformation(string organizationId, string prcId)
    {
        await _cache.RemoveAsync(BuildCadKey(organizationId, prcId));
    }
}