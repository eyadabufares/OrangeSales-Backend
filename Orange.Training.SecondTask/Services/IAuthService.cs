using System.Threading.Tasks; // ضروري للـ Task
using Orange.Training.SecondTask.Models;

namespace Orange.Training.SecondTask.Services
{
    public interface IAuthService
    {
        Task<bool> Login(LoginRequest request);
        Task<bool> Register(RegisterRequest request);
    }
}