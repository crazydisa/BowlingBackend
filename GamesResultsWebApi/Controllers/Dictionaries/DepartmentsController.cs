using GamesResults;
using GamesResults.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamesResults.Controllers.System
{
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly GamesResults.AppService service;

        public DepartmentsController(AppService service)
        {
            this.service = service;
        }

        
    }
}
