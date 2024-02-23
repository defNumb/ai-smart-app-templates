namespace DBTransferProject.Models
{
    public class PunchoutAccountData
    {
        public string Customer { get; set; }
        public string Identity { get; set; }
        public string Duns { get; set; }
        public string SharedSecret { get; set; }
        public string Keycode { get; set; }
        public int TaxExempt { get; set; } = 0;
        public string DeploymentMode { get; set; } = "test";
        public int DeploymentModeOverride { get; set; } = 1;
    }
}
