namespace Calcpad.WebApi.Configs
{
    public class UsersConfig : List<UserInfo>
    {
        /// <summary>
        /// is valid user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public UserInfo? GetUserInfo(string userName, string password)
        {
            var userInfo = this.FirstOrDefault(u =>
                u.Username == userName && u.Password == password
            );
            return userInfo;
        }
    }

    public class UserInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public List<string> Roles { get; set; } = [];
    }
}
