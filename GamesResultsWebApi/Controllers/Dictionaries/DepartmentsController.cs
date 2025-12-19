
using Microsoft.AspNetCore.Mvc;


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
