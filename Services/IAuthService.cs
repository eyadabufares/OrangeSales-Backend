using Orange.Training.SecondTask.Models;

namespace Orange.Training.SecondTask.Services
{
    public interface IAuthService
    {
        bool Login(LoginRequest request);
        bool Register(RegisterRequest request);
    }
}