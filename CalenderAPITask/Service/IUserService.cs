namespace CalenderAPITask.Service
{
    public interface IUserService
    {

        Task<string> SignUp(string gmail, string password);
        Task<string> SignIn(string gmail, string password);
    }
}
