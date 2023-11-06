using CalenderAPITask.Helper;
using CalenderAPITask.Models;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace CalenderAPITask.Service
{
    public class UserService : IUserService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly IConfiguration _config;

        public UserService(FirestoreDb firestoreDb, IConfiguration config)
        {
            _firestoreDb = firestoreDb;
            _config = config;
        }
        public async Task<string> SignIn(string gmail,string password)
        {
            var usersCollection = _firestoreDb.Collection("users");
            var userQuery = usersCollection.WhereEqualTo("gmail", gmail);
            var userSnapshot = await userQuery.GetSnapshotAsync();

            if (!userSnapshot.Documents.Any()) 
            {
                throw new Exception("User with this email does not exist. Redirect to sign up Instead.");
            }
            var userDocument = userSnapshot.Documents.First();
            var hashedPassword = userDocument.GetValue<string>("password");
            var salt = userDocument.GetValue<string>("salt");

            var saltAndHashedEneterdPassword = PasswordHasher.HashPassword(password,Convert.FromBase64String(salt));

            if (saltAndHashedEneterdPassword.hash == hashedPassword )
            {
                var token = GenerateJwt(gmail, userDocument.Id);
                return token;
            }
            else
            {
                throw new Exception("Invalid Email or Password");
            }
        }

        public async Task<string> SignUp(string gmail, string password)
        {
            var usersCollection = _firestoreDb.Collection("users");
            var existingUserQuery = usersCollection.WhereEqualTo("gmail", gmail);
            var existingUserSnapshot = await existingUserQuery.GetSnapshotAsync();

            if (existingUserSnapshot.Documents.Any())
            {
                throw new Exception("User with this email already exists. Redirect To Sign In Instead");
            }

            var saltAndHashedPassword = PasswordHasher.HashPassword(password);
            string salt = saltAndHashedPassword.salt;
            string hashedPassword = saltAndHashedPassword.hash;

            var newUserRecord = new Dictionary<string, object>
            {
                 {"gmail", gmail},
                 {"password", hashedPassword},
                 {"salt", salt},
                 {"expiryDate",0 },
                 {"refreshToken",null },
                 {"issuedAt",null }
            };

            var userRef = await usersCollection.AddAsync(newUserRecord);
            var token = GenerateJwt(gmail, userRef.Id);
            return token;
        }

        private string GenerateJwt(string gmail,string documentId)
        {
            string secretKey = _config.GetValue<string>("JWT_SECRET_KEY");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, gmail),
                new Claim(ClaimTypes.Sid,documentId)
            };

            var token = new JwtSecurityToken(
                issuer: "calendarApp",
                audience: "gmail-calendar-users",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
