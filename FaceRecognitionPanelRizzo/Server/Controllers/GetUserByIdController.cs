using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using face_recognition.Shared;
using DBContext;

namespace face_recognition.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GetUserById : ControllerBase
    {
        public Dictionary<int, string> errors= MyErrors.Codes_errors;

        [HttpGet]
        public User Get([FromQuery] string token, [FromQuery] int id){
            var db = new FaceContext();
            if (token == Variables.Token)
            {
                // Assuming you want to retrieve a specific user by ID
                User utente = db.Users.FirstOrDefault(u => u.Id == id);

                // Check if the user with the specified ID was found
                if (utente != null)
                {
                    return utente;
                }
                else
                {
                    // If the user with the specified ID was not found, return an empty list
                    return new User();
                }
            }
            else
            {
                // If the token is not valid, return an empty list
                return new User();
            }
        }
    }
}