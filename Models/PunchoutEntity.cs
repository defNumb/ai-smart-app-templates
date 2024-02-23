namespace DBTransferProject.Models
{
    public class PunchoutEntity
    {
        public int Id { get; set; }
        public string? identity {  get; set; }
        public string? duns { get; set; }
        public string? sharedsecret { get; set; }
        public string? keycode { get; set; }
        public int taxExempt { get; set; }

        public string? customer {  get; set; }
        public string? deployment_mode { get; set; }
        public int deployment_mode_override { get; set; }

            }
}
