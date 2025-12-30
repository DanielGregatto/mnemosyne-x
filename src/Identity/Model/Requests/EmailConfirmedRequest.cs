namespace Identity.Model.Requests
{
    public class EmailConfirmedRequest
    {
        public string Email { get; set; }   
        public string Token { get; set; }
    }
}
