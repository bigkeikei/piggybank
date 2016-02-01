﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PiggyBank.Models
{
    public class UserManager : IUserManager
    {
        private IPiggyBankDbContext _dbContext;

        public UserManager(IPiggyBankDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public User CreateUser(User user)
        {
            if (user == null) throw new PiggyBankDataException("User object is missing");
            PiggyBankUtility.CheckMandatory(user);
            user.Authentication = new UserAuthentication();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            return user;
        }

        public User FindUser(int userId)
        {
            User user = _dbContext.Users.Find(userId);
            if ( user == null ) throw new PiggyBankDataNotFoundException("User [" + userId + "] cannot be found");
            return user;
        }

        public User FindUserByName(string userName)
        {
            var q = _dbContext.Users.Where(b => b.Name == userName);
            if (!q.Any()) throw new PiggyBankDataNotFoundException("User [" + userName + "] cannot be found");
            return q.First();
        }

        public User FindUserByToken(string accessToken)
        {
            var q = _dbContext.Users.Where(b => b.Authentication.AccessToken == accessToken);
            if (!q.Any()) throw new PiggyBankDataNotFoundException("User with token [" + accessToken + "] cannot be found");
            return q.First();
        }

        public User UpdateUser(User user)
        {
            if (user == null) throw new PiggyBankDataException("User object is missing");
            User userToUpdate = FindUser(user.Id);
            
            if (userToUpdate.Name != user.Name) throw new PiggyBankDataException("Editing User.Name is not supported");
            PiggyBankUtility.CheckMandatory(user);
            PiggyBankUtility.UpdateModel(userToUpdate, user);
            _dbContext.SaveChanges();
            return userToUpdate;
        }

        public IEnumerable<User> ListUsers()
        {
            return _dbContext.Users;
        }

        public UserAuthentication GenerateChallenge(int userId)
        {
            User userToUpdate = FindUser(userId);
            userToUpdate.Authentication.Challenge = userToUpdate.Name + System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            userToUpdate.Authentication.ChallengeTimeout = System.DateTime.Now.AddSeconds(60);

            _dbContext.SaveChanges();
            return userToUpdate.Authentication;
        }

        public UserAuthentication GenerateToken(int userId, string signature)
        {
            User userToUpdate = FindUser(userId);
            UserAuthentication auth = userToUpdate.Authentication;
            MD5 md5 = MD5.Create();
            string authSign = Convert.ToBase64String(md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(auth.Challenge + auth.Secret)));
            if (authSign != signature) { throw new PiggyBankDataException("Invalid signature [" + signature + "]"); }
            if (DateTime.Now >= auth.ChallengeTimeout) { throw new PiggyBankAuthenticationTimeoutException("Challenge expired"); }

            userToUpdate.Authentication.AccessToken = Hash(System.Guid.NewGuid().ToString() + userToUpdate.Name);
            userToUpdate.Authentication.RefreshToken = Hash(System.Guid.NewGuid().ToString() + userToUpdate.Name);
            userToUpdate.Authentication.TokenTimeout = System.DateTime.Now.AddMinutes(30);
            _dbContext.SaveChanges();
            return userToUpdate.Authentication;
        }

        public User CheckAccessToken(int userId, string accessToken)
        {
            User user = FindUser(userId);
            if (user.Authentication.AccessToken != accessToken) { throw new PiggyBankDataException("Invalid token [" + accessToken + "]"); }
            if (DateTime.Now >= user.Authentication.TokenTimeout) { throw new PiggyBankAuthenticationTimeoutException("Token expired"); }
            return user;
        }

        private string Hash(string content)
        {
            MD5 md5 = MD5.Create();
            return Convert.ToBase64String(md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(content)));
        }
    }
}
