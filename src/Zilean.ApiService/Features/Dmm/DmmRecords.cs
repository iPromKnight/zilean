namespace Zilean.ApiService.Features.Dmm;

public record ExtractedDmmEntry(string Filename, string InfoHash, long Filesize);
public record DmmQueryRequest(string QueryText);
