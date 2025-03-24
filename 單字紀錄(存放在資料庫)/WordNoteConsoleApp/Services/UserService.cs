using WordNoteConsoleApp.DataAccess;
using WordNoteConsoleApp.Models;

namespace WordNoteConsoleApp.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepo;

        public UserService(UserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public User Login(string account, string password)
        {
            return _userRepo.Login(account, password);
        }

        public bool Register(string name, string account, string password)
        {
            var newUser = new User
            {
                Name = name,
                Account = account
            };
            return _userRepo.Register(newUser, password);
        }
    }
}
