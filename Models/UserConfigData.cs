namespace DBTransferProject.Models
{
    public class UserConfigData
    {
        // Assume these properties match the UserConfig table schema
        public string Key { get; set; }
        public string Name { get; set; }
        public string ProviderCredential { get; set; }
        public string UserIdentity { get; set; }
        // Add other properties as needed
    }
}
