using GamesResults.Interfaces;
using GamesResults.Models;
using System.Text.Json.Serialization;

namespace GamesResults.Models
{
    public class User : INamed, ITitled
    {
        public long Id { get; set; }

        public Guid SapsanId { get; set; }

        public string Name { get; set; } = null!;
        [displayExpr]
        public string Title { get; set; } = null!;


        public bool IsBlocked { get; set; }

        public bool IsAdmin => Roles != null ? Roles.Any(r => r.IsAdmin) : false;

        public List<Role>? Roles { get; set; }
        public DateTime DtChange { get; set; }
 
        public string? Login { get; set; }
        
        public string? Surname { get; set; }

        public string? Patronymic { get; set; }

        public string? Inn { get; set; }

        public string? Phone { get; set; }

        public string? EMail { get; set; }

        public string? Remark { get; set; }

        public int? CanBeGip { get; set; }

        public long? IdOrganization { get; set; }

        public string? EmpNumber { get; set; }

        public long? IdRoom { get; set; }

        public DateTime? Birthday { get; set; }

        public long? PmObjectId { get; set; }

        public string? Placement { get; set; }

        public byte[]? Sid { get; set; }

        public int? IdInt { get; set; }

        public long? IdPosition { get; set; }

        public long? IdSubdivision { get; set; }

        public long? IdOtdel { get; set; }
        [JsonIgnore]
        public DateTime? DtStart { get; set; }
        [JsonIgnore]
        public DateTime? DtFinish { get; set; }

        public int? Greid { get; set; }

        public string? Host { get; set; }

        public string? IpAdress { get; set; }

        public string? DataSource { get; set; }

        public bool? IsChief { get; set; }

        public int? Status { get; set; }

        public long? IdFuncSubdivision { get; set; }

        public long? IdFuncOtdel { get; set; }

        public string? ShortName { get; set; }

        public long? IdExternal { get; set; }

        public int IdType { get; set; }

        public string? ExternalKey { get; set; }
        
        //public virtual Sapsan.SprSubdivision? IdFuncOtdelNavigation { get; set; }
        
        //public virtual Sapsan.SprSubdivision? IdFuncSubdivisionNavigation { get; set; }
        
        //public virtual Sapsan.SprOrganization? IdOrganizationNavigation { get; set; }
        
        //public virtual Sapsan.SprSubdivision? IdOtdelNavigation { get; set; }

        //public virtual Sapsan.SprPositionEmployee? IdPositionNavigation { get; set; }

        //public virtual Sapsan.SprSubdivision? IdSubdivisionNavigation { get; set; }
        //public virtual ICollection<TzKit> TzKitNavigations { get; set; } = new List<TzKit>();
        
    }
}
