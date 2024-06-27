namespace MC_WebAuth.Models
{
    public class AuthorizeResponseModel
    {
        public bool IsAuthorized { get; set; }
        public string? AuthCode { get; set; }
        public int? DeptId { get; set; }
    }
}
