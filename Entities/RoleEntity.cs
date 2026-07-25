using Microsoft.AspNetCore.Identity;

namespace AutoCare_Club_Api.Entities
{
    public class RoleEntity : IdentityRole
    {
        public string Descripcion { get; set; }
    }
}
