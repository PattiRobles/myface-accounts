using System.Collections.Generic;
using System.Linq;
using MyFace.Models.Database;
using MyFace.Models.Request;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System;
using Microsoft.AspNetCore.Http;
using System.Web;
using System.Text;

namespace MyFace.Repositories
{
    public interface IUsersRepo
    {
        IEnumerable<User> Search(UserSearchRequest search);
        int Count(UserSearchRequest search);
        User GetById(int id);
        User Create(CreateUserRequest newUser);
        User Update(int id, UpdateUserRequest update);
        void Delete(int id);
        Boolean HasAccess(string httpContext);
    }
    
    public class UsersRepo : IUsersRepo
    {
        private readonly MyFaceDbContext _context;
		private object authHeader;

		public UsersRepo(MyFaceDbContext context)
        {
            _context = context;
        }
        
        public IEnumerable<User> Search(UserSearchRequest search)
        {
            return _context.Users
                .Where(p => search.Search == null || 
                            (
                                p.FirstName.ToLower().Contains(search.Search) ||
                                p.LastName.ToLower().Contains(search.Search) ||
                                p.Email.ToLower().Contains(search.Search) ||
                                p.Username.ToLower().Contains(search.Search)
                            ))
                .OrderBy(u => u.Username)
                .Skip((search.Page - 1) * search.PageSize)
                .Take(search.PageSize);
        }

        public int Count(UserSearchRequest search)
        {
            return _context.Users
                .Count(p => search.Search == null || 
                            (
                                p.FirstName.ToLower().Contains(search.Search) ||
                                p.LastName.ToLower().Contains(search.Search) ||
                                p.Email.ToLower().Contains(search.Search) ||
                                p.Username.ToLower().Contains(search.Search)
                            ));
        }

        public User GetById(int id)
        {
           
            return _context.Users
                .Single(user => user.Id == id);
        }

        public User Create(CreateUserRequest newUser)
        {
            var salt = GenerateSalt();
            var insertResponse = _context.Users.Add(new User
            {
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Email = newUser.Email,
                Username = newUser.Username,
                ProfileImageUrl = newUser.ProfileImageUrl,
                CoverImageUrl = newUser.CoverImageUrl,
                Hashed_password = GenerateHashPassword(newUser.Hashed_password , salt),
                Salt = salt,
            });
            _context.SaveChanges();

            return insertResponse.Entity;
        }

        public User Update(int id, UpdateUserRequest update)
        {
            var user = GetById(id);

            user.FirstName = update.FirstName;
            user.LastName = update.LastName;
            user.Username = update.Username;
            user.Email = update.Email;
            user.ProfileImageUrl = update.ProfileImageUrl;
            user.CoverImageUrl = update.CoverImageUrl;

            _context.Users.Update(user);
            _context.SaveChanges();

            return user;
        }

        public void Delete(int id)
        {
            var user = GetById(id);
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

         public static byte[] GenerateSalt()
        {
          return RandomNumberGenerator.GetBytes(128 / 8); 
            
        }

        public static string GenerateHashPassword(string password, byte[] salt )
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password!,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
            return hashed;
        }
        public Boolean HasAccess(string authHeader) {
            
            string password = "";
            string username = "";

            if (authHeader != null && authHeader.StartsWith("Basic")) {
    //Extract credentials
            string encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();

            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
            int seperatorIndex = usernamePassword.IndexOf(':');

            username = usernamePassword.Substring(0, seperatorIndex);
            password = usernamePassword.Substring(seperatorIndex + 1);

            } else { return false;}
            var user = GetUserByUserName(username);
            if (user == null) return false;
            if (user.Hashed_password != UsersRepo.GenerateHashPassword(password, user.Salt)) 
            {return false;}
            return true;
        }
        public User GetUserByUserName(string username) {
                      //accessing the 'Users' tables in DB   
            var resultUser = _context.Users.SingleOrDefault(c => c.Username == username);
            return resultUser;
        }
    }
}
