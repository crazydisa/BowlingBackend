using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Utils
{
    public class PdfUploadDto
    {
        public string Tournament { get; set; } = "Нет данных";
        public int Year { get; set; }
        public string City { get; set; } = "Нет данных";
        public string Description { get; set; } = "Нет данных";
    }
}
