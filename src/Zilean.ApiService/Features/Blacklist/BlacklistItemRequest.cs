// ReSharper disable InconsistentNaming
namespace Zilean.ApiService.Features.Blacklist;

public class BlacklistItemRequest
{
    public required string info_hash { get; set; }
    public required string reason { get; set; }
}
