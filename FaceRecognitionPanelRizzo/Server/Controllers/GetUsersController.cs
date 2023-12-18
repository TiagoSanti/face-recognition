using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using face_recognition.Shared;
using System.Security.Permissions;
using DBContext;

namespace face_recognition.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GetUsersController : ControllerBase
    {
        
        public Dictionary<int, string> errors= MyErrors.Codes_errors;

        [HttpGet]
        public List<User> Get([FromQuery] string token){
            var db = new FaceContext();
            List<User> vuota = new List<User>();
            if (token == Variables.Token)
            {
                List<User> all_utenti = db.Users.ToList();
                return all_utenti;
            }else{
                return vuota;
            }
        }
    }
}