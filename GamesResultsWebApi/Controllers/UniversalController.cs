
using GamesResults;
using GamesResults.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;
using System.Reflection;
//using System.Web;
//using System.Web.Http;
using Microsoft.Net.Http.Headers;
using System.Net;
//using PirAppLib.Models.Smpd;
using System.Text.Json;
using Microsoft.Extensions.Logging;
//using SapsanLibPost.Models;
using System;
using System.Net.Mail;
using System.Resources;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using GamesResults.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.CodeDom;
using System.IO;
using OfficeOpenXml;
using System.Threading;
using GamesResults.Utils;
using System.Text.RegularExpressions;

namespace GamesResults.Controllers.System
{
    [ApiController]
    
    public class UploadController : ControllerBase
    {

        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [HttpPost("/upload-template")]
        public async Task<IActionResult> UploadTemplate()
        {
            //appClient.BeginAction(MethodBase.GetCurrentMethod());

            // Request's .Form.Files property will
            // contain QUploader's files.
            var files = this.Request.Form.Files;
            foreach (var file in files)
            {
                if (file == null || file.Length <= 0)
                    return BadRequest("File is empty");
                if (!Regex.IsMatch(Path.GetExtension(file.FileName), ".xls[x,m]", RegexOptions.IgnoreCase))
                {
                    return BadRequest("File extension is not supported");
                }

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorkbook workBook = package.Workbook;
                        if (workBook != null)
                        {
                            var fileName = file.Name;
                            //ExcelUtils excelUtils = new ExcelUtils(sapsanContext, nsContext, workBook, fileName);

                            //var tabel = excelUtils.ReadTabel();
                            //if (tabel != null)
                            //{
                            //    nsContext.Tabels.Add(tabel);
                            //}
                        }

                    }
                }

                //return Ok($"{file.FileName} Uploaded");
            }

            //appClient.EndAction();
            //nsContext.SaveChanges();
            return Ok();
        }

        
    }
    

}
