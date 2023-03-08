using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace MyFace.Controllers;

    public class LoginController : ControllerBase
    {
        [HttpPost("get")]
        public IActionResult GetUserLoginData()
        {
             string authHeader = HttpContext.Request.Headers["Authorization"];

             if (authHeader != null && authHeader.)
        }

        public User GetUserByUserName(string username) {
                      //accessing the 'Users' tables in DB   
        var resultUser = _context.Users.SingleOrDefault(c => c.Username == username);
        return resultUser;
        }
    }
