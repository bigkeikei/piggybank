﻿using System;
using System.Collections.Generic;

using SimpleIdentity.Models;

namespace SimpleIdentity.UnitTesting.Mocks
{
    public class MockData
    {
        public MockData()
        {
            Users = new List<User>();
            Tokens = new List<Token>();
            Authorizations = new List<Authorization>();
            Clients = new List<Client>();
        }

        public List<User> Users { get; set; }
        public List<Token> Tokens { get; set; }
        public List<Authorization> Authorizations { get; set; }
        public List<Client> Clients { get; set; }

        public static MockData Seed()
        {
            MockData data = new MockData();
            int userId = 0;
            int tokenId = 0;
            data.Clients.Add(new Client { Id = 1, Secret = "123" });
            data.Users.Add(new User { Id = ++userId, Name = "Happy Cat", Email = "cat@happy.com", IsActive = true });
            data.Users.Add(new User { Id = ++userId, Name = "Skiny Pig", Email = "pig@skiny.com", IsActive = true });
            data.Users.Add(new User { Id = ++userId, Name = "Silly Dog", Email = "dog@silly.com", IsActive = true });
            foreach (User user in data.Users)
            {
                user.Authentication = new UserAuthentication { Id = user.Id, User = user, Secret = "secret" };
                data.Tokens.Add(new Token { Id = ++tokenId, User = user, AccessToken = "token" + tokenId.ToString(), TokenTimeout = DateTime.Now.AddSeconds(60), Client = data.Clients[0] });
            }

            return data;
        }
    }
}
