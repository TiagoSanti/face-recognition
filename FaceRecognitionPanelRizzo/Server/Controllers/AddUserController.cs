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
    public class AddUserController : ControllerBase
    {
        public Dictionary<int, string> errors= MyErrors.Codes_errors;

        [HttpGet]
        public JsonResult Get([FromQuery] string token, [FromQuery] string name, [FromQuery] string surname){
            var db = new FaceContext();
            string Output = "";
            int code = 200;
            string path = "utenti/";
            if(token == Variables.Token){
                if(name!=""){
                    string dir = name+"_"+surname;
                    if(!Directory.Exists(path+dir)){
                        code = 210;
                        Directory.CreateDirectory(path+dir);
                        db.Add(new User{ Name=name, Surname=surname });
                        db.SaveChanges(); /* problema erore non trova tabella Classes */
                        errors.TryGetValue(code, out Output);
                        return new JsonResult( new { Code = code, msg = Output, utenti = Users.Users_db } );
                    }else{
                        code = 211;
                        errors.TryGetValue(code, out Output);
                        return new JsonResult( new { Code = code, msg = Output, utenti = Users.Users_db } );
                    }
                }else{
                    code = 212;
                    errors.TryGetValue(code, out Output);
                    return new JsonResult( new { Code = code, msg = Output } );
                }
            }else{
                code = 403;
                errors.TryGetValue(code, out Output);
                return new JsonResult( new { Code = code, msg = Output, Details = "Token non valido" } );
            }
            
            // if(token == Variables.Token){
            //     var result = Array.Find(Users.Users_db, element => element == name);
            //     if(result==name){
            //         code = 211;
            //     }else{
            //         code = 210;
            //         Users.Users_db.Append(name);  
            //     }
            //     errors.TryGetValue(code, out Output);
            //     return new JsonResult( new { code = 200, msg = Output, utenti = Users.Users_db } );
            // }else{
            //     errors.TryGetValue(403, out Output);
            //     return new JsonResult( new { code = 403, msg = Output, Details = "Token non valido" } );
            // }
        }
    }
}