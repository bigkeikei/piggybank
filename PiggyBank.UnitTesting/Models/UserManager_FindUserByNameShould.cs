﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using PiggyBank.Models;
using PiggyBank.UnitTesting.Mocks;

namespace PiggyBank.UnitTesting.Models
{
    public class UserManager_FindUserByNameShould
    {
        [Fact]
        public void ThrowException_WhenUserNotFound()
        {
            Exception ex = null;
            try
            {
                var mockDbContext = new MockPiggyBankDbContext();
                UserManager userManager = new UserManager(mockDbContext);
                userManager.FindUserByName("ABC");
            }
            catch (PiggyBankDataNotFoundException e) { ex = e; }

            Assert.True(ex != null);
        }

        [Fact]
        public void ReturnUser_WhenUserIdMatch()
        {
            MockData data = new MockData();
            data.Users.Add(new User { Id = 1, Name = "Happy Cat" });
            data.Users.Add(new User { Id = 2, Name = "Skiny Pig" });
            data.Users.Add(new User { Id = 3, Name = "Silly Dog" });
            User user = data.Users[2];
            var mockDbContext = new MockPiggyBankDbContext(data);
            UserManager userManager = new UserManager(mockDbContext);
            User found = userManager.FindUserByName(user.Name);

            Assert.True(found.Id == user.Id);
            Assert.True(found.Name == user.Name);
        }
    }
}
