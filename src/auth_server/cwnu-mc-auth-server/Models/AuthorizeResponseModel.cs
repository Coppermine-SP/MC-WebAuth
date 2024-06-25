namespace cwnu_mc_auth_server.Models
{
    public class AuthorizeResponseModel
    {
        public bool IsAuthorized { get; set; }
        public string? AuthCode { get; set; }
        public int? DeptId { get; set; }
    }
}
