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
    public class DeleteUserController : ControllerBase
    {
        public Dictionary<int, string> errors= MyErrors.Codes_errors;

        [HttpGet]
        public string Get([FromQuery] string token, [FromQuery] int id){
            var db = new FaceContext();
            if(token == Variables.Token){
                User utente = db.Users.FirstOrDefault(u => u.Id == id);
                string path = "utenti/" + utente.Name + "_" + utente.Surname;
                if(Directory.Exists(path)){
                    Directory.Delete(path);
                }
                db.Users.Remove(utente);
                db.SaveChanges();
                return "User " + utente.Name + " deleted";
            }else{
                return "Invalid API token";
            }
        }
    }
}