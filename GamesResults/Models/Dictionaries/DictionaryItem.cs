﻿namespace GamesResults.Models
{
    /// <summary>
    /// Элемент Справочника
    /// </summary>
    public class DictionaryItem : Object
    {
        public DictionaryType? DictionaryType { get; set; }
        public long? DictionaryTypeId { get; set; }
        [displayExpr]
        public string? Name { get; set; } = null!;
        public bool? IsNotUsed { get; set; }
    }
}
