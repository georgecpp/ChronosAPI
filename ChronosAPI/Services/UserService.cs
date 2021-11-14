using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChronosAPI.Helpers;
using ChronosAPI.Models;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace ChronosAPI.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        RegisterResponse Register(User user);
        User GetUserById(int id);
    }
    public class UserService : IUserService
    {
        //private List<User> _users = new List<User>(); // populate users from db.
        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }
        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            if(string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return null; // CREDENTIALS EMPTY
            }

            // check if email exists in db, get user.
            string query = @" SELECT * from dbo.Users WHERE Email = @Email";

            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@Email", model.Email);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            if(table.Rows.Count == 0)
            {
                return null; // USER WITH THAT EMAIL WAS NOT FOUND
            }

            var userRow = table.AsEnumerable().SingleOrDefault(row => model.Email == row.Field<string>("Email"));
            User user = new User();
            user.UserId = Convert.ToInt32(userRow.Field<int>("UserID"));
            user.FirstName = userRow.Field<string>("FirstName");
            user.LastName = userRow.Field<string>("LastName");
            user.Email = userRow.Field<string>("Email").ToString();
            user.DateOfBirth = userRow.Field<DateTime>("DateOfBirth");
            user.CreatedAt = userRow.Field<DateTime>("CreatedAt");
            user.Password = userRow.Field<string>("Password");

            byte[] passwordHash;
            CreatePasswordHash(model.Password, out passwordHash);
            if(passwordHash.ToString() != user.Password)
            {
                return null; // INVALID PASSWORD
            }

            // authentication successful so generate jwt token
            var token = GenerateJwtToken(user);

            return new AuthenticateResponse(user, token);
        }

        public RegisterResponse Register(User user)
        {
            // Validate user first

            // hash password
            byte[] passwordHash;
            CreatePasswordHash(user.Password, out passwordHash);
            user.Password = passwordHash.ToString();
            

            string query = @" INSERT INTO [dbo].[Users]
           ([FirstName]
           ,[LastName]
           ,[Email]
           ,[Password]
           ,[DateOfBirth]
           ,[CreatedAt])
     VALUES
           (@FirstName,
           @LastName,
           @Email,
           @Password,
           @DateOfBirth,
           @CreatedAt)";

            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@FirstName", user.FirstName);
                    myCommand.Parameters.AddWithValue("@LastName", user.LastName);
                    myCommand.Parameters.AddWithValue("@Email", user.Email);
                    myCommand.Parameters.AddWithValue("@Password", user.Password);
                    myCommand.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth.Date);
                    myCommand.Parameters.AddWithValue("@CreatedAt", ((object)user.CreatedAt) ?? DateTime.Now);

                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new RegisterResponse(user);
        }

        // util methods

        private string GenerateJwtToken(User user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.UserId.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash)
        {
            if(password == null)
            {
                throw new ArgumentNullException("password");
            }

            if(string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Value cannot be empty or whitespace.", "password");
            }

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public User GetUserById(int id)
        {
            string query = @" SELECT * from dbo.Users WHERE UserID = @UserId";

            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@UserId", id);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            if (table.Rows.Count == 0)
            {
                return null; // USER WITH THAT id WAS NOT FOUND
            }

            var userRow = table.AsEnumerable().SingleOrDefault(row => id == row.Field<int>("UserID"));
            User user = new User();
            user.UserId = Convert.ToInt32(userRow.Field<int>("UserID"));
            user.FirstName = userRow.Field<string>("FirstName");
            user.LastName = userRow.Field<string>("LastName");
            user.Email = userRow.Field<string>("Email").ToString();
            user.DateOfBirth = userRow.Field<DateTime>("DateOfBirth");
            user.CreatedAt = userRow.Field<DateTime>("CreatedAt");
            user.Password = userRow.Field<string>("Password");

            return user;           
        
        }
    }
}
