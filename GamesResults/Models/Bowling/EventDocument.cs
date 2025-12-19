using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GamesResults.Models.Bowling
{
    public class TournamentDocument: Object
    {
        [StringLength(500)]
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Уникальное имя файла для хранения
        /// </summary>
        [StringLength(500)]
        public string? StoredFileName { get; set; }

        /// <summary>
        /// MIME-тип файла
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ContentType { get; set; }

        /// <summary>
        /// Размер файла в байтах
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Файл в бинарном виде (если хранится в БД)
        /// </summary>
        [Column(TypeName = "bytea")]
        public byte[] FileData { get; set; }

        /// <summary>
        /// Путь к файлу на диске (если хранится на файловой системе)
        /// </summary>
        [StringLength(1000)]
        public string? FilePath { get; set; }

        /// <summary>
        /// Тип документа (PDF, Excel, Word и т.д.)
        /// </summary>
        [StringLength(50)]
        public string? DocumentType { get; set; } = "PDF";

        /// <summary>
        /// Хэш файла для проверки целостности
        /// </summary>
        [StringLength(64)]
        public string? FileHash { get; set; }

        /// <summary>
        /// MD5 хэш файла (альтернатива)
        /// </summary>
        [StringLength(32)]
        public string? Md5Hash { get; set; }

        // --- Связи ---

        /// <summary>
        /// Внешний ключ для связи с Tournament
        /// </summary>
        [Required]
        public long TournamentId { get; set; }

        /// <summary>
        /// Навигационное свойство для Tournament
        /// </summary>
        [ForeignKey(nameof(TournamentId))]
        public virtual Tournament Tournament { get; set; }

        // --- Вычисляемые свойства ---

        /// <summary>
        /// Форматированный размер файла
        /// </summary>
        [NotMapped]
        public string FormattedFileSize
        {
            get
            {
                if (FileSize < 1024) return $"{FileSize} B";
                if (FileSize < 1024 * 1024) return $"{(FileSize / 1024.0):F1} KB";
                return $"{(FileSize / (1024.0 * 1024.0)):F1} MB";
            }
        }

        /// <summary>
        /// Расширение файла
        /// </summary>
        [NotMapped]
        public string FileExtension => Path.GetExtension(OriginalFileName)?.ToLower();

        /// <summary>
        /// Имя файла без расширения
        /// </summary>
        [NotMapped]
        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(OriginalFileName);
    }
}
