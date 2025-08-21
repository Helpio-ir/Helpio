using AutoMapper;
using System.Text.RegularExpressions;

namespace Helpio.Ir.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Add your entity mappings here
            // Example:
            // CreateMap<YourEntity, YourEntityDto>();
            // CreateMap<YourEntityCreateDto, YourEntity>();
        }

        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Convert to lowercase
            string slug = input.ToLower();
            
            // Remove special characters
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            
            // Replace spaces with hyphens
            slug = Regex.Replace(slug, @"\s+", "-");
            
            // Remove multiple hyphens
            slug = Regex.Replace(slug, @"-+", "-");
            
            // Trim hyphens from start and end
            slug = slug.Trim('-');
            
            return slug;
        }
    }
}