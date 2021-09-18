﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TaskManagerTLA.BLL.DTO;
using TaskManagerTLA.BLL.Interfaces;
using TaskManagerTLA.DAL.Identity;
using TaskManagerTLA.DAL.Identity.Interfaces;

namespace TaskManagerTLA.BLL.Services
{
    public class IdentityServices : IIdentityServices
    {
        IUnitOfWorkIdentity Db { get; }
        IMapper mapper;
        SignInManager<IdentityUser> signInManager;
        public IdentityServices(IUnitOfWorkIdentity identityRepositories, IMapper mapper, SignInManager<IdentityUser> signInManager)
        {
            Db = identityRepositories;
            this.mapper = mapper;
            this.signInManager = signInManager;
        }

        public async Task Login(LoginDTO loginUser)
        {
            var result = await signInManager.PasswordSignInAsync(loginUser.UserName, loginUser.Password, loginUser.RememberMe, false);
            if (!result.Succeeded)
            {
                throw new Exception("Невірний логін або пароль");
            }
        }
        public async Task Logout()
        {
            await signInManager.SignOutAsync();
        }

        public void RegisterNewUser(UserDTO newUser)
        {
            if (GetUserByName(newUser.UserName) != null)
            {
                throw new Exception("Користувач з таким іменем вже існує!");
            }
            if (!CreateUserAndRole(newUser))
            {
                throw new Exception("Не можливо створити користувача, введено не коректні данні ");
            }


        }


        //в данному методі не використовую автомапер, тому що в данній реалізації для більшості полів IdentityUser буде присвоєно значення за замовчуванням 
        public bool CreateUserAndRole(UserDTO newUser)
        {
            //створення User
            PasswordHasher<IdentityUser> hasher = new PasswordHasher<IdentityUser>();
            IdentityUser newUserDb = new IdentityUser()
            {
                UserName = newUser.UserName,
                NormalizedUserName = newUser.UserName.ToUpper(),
                Email = newUser.Email,
                NormalizedEmail = newUser.Email.ToUpper(),
            };
            newUserDb.PasswordHash = hasher.HashPassword(newUserDb, newUser.Password);

            //присвоюєм юзеру роль "Developer"
            if (Db.UsersRepositories.CreateItem(newUserDb))
            {
                string developerRoleId = "";
                foreach (var item in Db.RolesRepositories.GetAllItems())
                {
                    if (item.Name == "Developer")
                    {
                        developerRoleId = item.Id;
                    }
                }

                //зміни в Базі Данних зберігаются лише після успішного додавання юзера і ролі в БД, таким чином ми уникаєм можливості створення юзера без ролі 
                return CreateUserRole(newUserDb.Id, developerRoleId);
            }
            return false;
        }

        public bool CreateUserRole(string userId, string roleId)
        {
            if (Db.UserRolesRepositories.CreateItem(new IdentityUserRole<string>() { RoleId = roleId, UserId = userId }))
            {
                Db.Save();
                return true;
            }
            return false;
        }

        public void DeleteUser(string userId)
        {
            //видаляєм користувача і всі звязані з ним ролі
            if (Db.UsersRepositories.DeleteItem(Db.UsersRepositories.GetItem(userId)))
            {
                DeleteAllUserRoles(userId);
                Db.Save();
            }
        }

        public void ChangeUserRole(string userId, string newRoleId)
        {
            DeleteAllUserRoles(userId);
            CreateUserRole(userId, newRoleId);
        }

        //видаляєм всі звязані з користувачем ролі
        public void DeleteAllUserRoles(string userId)
        {
            foreach (var item in Db.UserRolesRepositories.GetAllItems())
            {
                if (item.UserId == userId)
                {
                    Db.UserRolesRepositories.DeleteItem(item);

                }
            }
        }

        public void DeleteRole(string roleId)
        {
            foreach (var item in Db.RolesRepositories.GetAllItems())
            {
                if (item.Id == roleId)
                {
                    Db.RolesRepositories.DeleteItem(item);
                    Db.Save();
                }
            }
        }

        public void CreateRole(string newRoleName)
        {
            Db.RolesRepositories.CreateItem(new IdentityRole(newRoleName));
            Db.Save();
        }
        public string GetUserRole(string userId)
        {
            string roleId = "";
            foreach (var item in Db.UserRolesRepositories.GetAllItems())
            {
                if (item.UserId == userId)
                {
                    roleId = item.RoleId;
                }
            }
            return Db.RolesRepositories.GetItem(roleId).Name;
        }

        public IEnumerable<RoleDTO> GetRoles()
        {
            IEnumerable<IdentityRole> rolesDb = Db.RolesRepositories.GetAllItems();
            var roles = mapper.Map<IEnumerable<IdentityRole>, List<RoleDTO>>(rolesDb);
            return roles;
        }

        public UserDTO GetUserById(string userId)
        {
            var user = mapper.Map<UserDTO>(Db.UsersRepositories.GetItem(userId));
            return user;
        }

        public UserDTO GetUserByName(string userName)
        {
            IdentityUser returnedUser = null;
            foreach (var item in Db.UsersRepositories.GetAllItems())
            {
                if (userName == item.UserName)
                {
                    returnedUser = item;
                }
            }
            var user = mapper.Map<UserDTO>(returnedUser);
            return user;
        }

        public IEnumerable<UserDTO> GetUsers()
        {
            var users = mapper.Map<IEnumerable<IdentityUser>, List<UserDTO>>(Db.UsersRepositories.GetAllItems());
            foreach (var item in users)
            {
                item.UserRole = GetUserRole(item.id);
            }
            return users;
        }
    }
}
