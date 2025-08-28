namespace ApiCrmAlive.DTOs.Marketplaces
{
    public class TestConnectionDetailsDto
    {
        public int ResponseTimeMs { get; set; }
        public string ApiVersion { get; set; } = null!;
        public bool AccountVerified { get; set; }
    }
}