using ApiCrmAlive.Utils;

namespace ApiCrmAlive.DTOs.Analytics;

public sealed class VehiclesByStatusDto
{
    public VehicleStatusEnum Status { get; init; }
    public int Count { get; init; }
}

