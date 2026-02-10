namespace IslandPostPOS.Shared.DTOs
{
    public class LoginResponseDTO
    {
        public string Message { get; set; }
        public string Token { get; set; }
        public UserDTO User { get; set; }
    }

}
