using ApiCrmAlive.Utils;

namespace ApiCrmAlive.DTOs.Analytics;

public sealed class FunnelStageDto
{
    public LeadStatusEnum Status { get; init; }
    public int Count { get; init; }
}

