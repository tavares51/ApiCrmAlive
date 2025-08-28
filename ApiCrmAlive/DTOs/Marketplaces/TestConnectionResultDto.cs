namespace ApiCrmAlive.DTOs.Marketplaces
{
    public class TestConnectionResultDto
    {
        public string Status { get; set; } = null!;
        public string Message { get; set; } = null!;
        public TestConnectionDetailsDto Details { get; set; } = null!;
    }
}