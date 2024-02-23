using AutoMapper;
using DBTransferProject.Models;
using System;


namespace DBTransferProject.Mappings
{
    public class PunchoutAcctMapper : Profile
    {
        public PunchoutAcctMapper()
        {
            CreateMap<ExcelDataRow, PunchoutEntity>()
                // No direct mapping for ASN, CXML, etc.

                // SchoolOrganization maps to multiple fields with manipulation
                .ForMember(dest => dest.customer, opt => opt.MapFrom(src => src.SchoolOrganization))
                .ForMember(dest => dest.identity, opt => opt.MapFrom(src => RemoveSpaces(src.SchoolOrganization)))
                .ForMember(dest => dest.duns, opt => opt.MapFrom(src => RemoveSpaces(src.SchoolOrganization)))

                // KeyCode is the short version of the organization name, truncated to 10 characters
                .ForMember(dest => dest.keycode, opt => opt.MapFrom(src => src.SchoolOrganization.Length <= 10 ? src.SchoolOrganization : src.SchoolOrganization.Substring(0, 10)))

                // Default values for taxExempt, deployment_mode, and deployment_mode_override
                .ForMember(dest => dest.taxExempt, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.deployment_mode, opt => opt.MapFrom(src => "test")) // assuming "test" is a string representation
                .ForMember(dest => dest.deployment_mode_override, opt => opt.MapFrom(src => 1))

                // After mapping, generate the sharedsecret independently
                .AfterMap((src, dest) =>
                {
                    dest.sharedsecret = GenerateSharedSecret();
                });
        }

        // Helper method to remove spaces from a string
        private string RemoveSpaces(string input)
        {
            return input.Replace(" ", string.Empty);
        }

        // Helper method to generate a shared secret
        private string GenerateSharedSecret()
        {
            // Your logic to generate a shared secret
            // Example: a GUID without dashes
            return Guid.NewGuid().ToString("N");
        }
    }
}
