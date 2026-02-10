using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IslandPostPOS.Services;
using IslandPostPOS.Shared.DTOs;
using System;
using System.Threading.Tasks;

public partial class LoginViewModel : ObservableObject
{
    private readonly ProductService service;

    [ObservableProperty] private string username;
    [ObservableProperty] private string password;
    [ObservableProperty] private bool keepLoggedIn;
    [ObservableProperty] private string statusMessage;
    [ObservableProperty] private bool isBusy;

    public event Action<LoginResponseDTO>? LoggedIn;
    public event Action? ClearPasswordRequested;


    public LoginViewModel(ProductService service)
    {
        this.service = service;
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Please enter both email and password.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Logging in...";

            var loginDTO = new UserLoginDTO
            {
                Email = Username,
                PassWord = Password,
                KeepLoggedIn = KeepLoggedIn,
            };

            var result = await service.LoginAsync(loginDTO);

            if (result != null)
            {
                StatusMessage = $"Welcome {result.User.Name}!";
                LoggedIn?.Invoke(result);
            }
            else
            {
                StatusMessage = "Invalid credentials.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Login failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            Password = string.Empty; // clear sensitive data
            ClearPasswordRequested?.Invoke();
        }
    }

    private bool CanLogin() => !IsBusy;
}