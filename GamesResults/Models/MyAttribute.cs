using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models
{
    public class displayExpr : Attribute { }
    public class valueExpr : Attribute { }
    public class keyExpr : Attribute { }
    public class ObjectPropertyAttribute : Attribute {
        public string? Name { get; set; }

        public string? Title { get; set; }

        public string? GroupTitle { get; set; }

        public string? SubGroupTitle { get; set; }

        public string? Description { get; set; }

        public int? SortIndex { get; set; }

        public string? TypeName { get; set; }
        public string? NameSpace { get; set; }

        public string? DictionaryTypeName { get; set; }

        public string? DataFormat { get; set; }

        public string? DisplayExpr { get; set; }

        public string? RelatedField { get; set; }

        public string? RelatedType { get; set; }

        public bool? IsIdentifier { get; set; }

        public bool? IsNumeric { get; set; }

        public bool? IsBoolean { get; set; }

        public bool? IsString { get; set; }

        public bool? IsMultiline { get; set; }

        public bool? IsGuid { get; set; }

        public bool? IsDate { get; set; }

        public bool? IsObject { get; set; }

        public bool? IsInclude { get; set; }

        public bool? IsArray { get; set; }

        public bool? IsNullable { get; set; }

        public bool? IsHiddenByDefault { get; set; }

        public bool? IsHiddenInLogDetail { get; set; }

        public bool? IsReadOnly { get; set; }

        public bool? IsNotAvailable { get; set; }
    }
}
