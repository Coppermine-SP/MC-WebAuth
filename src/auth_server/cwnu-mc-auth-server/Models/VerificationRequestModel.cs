namespace cwnu_mc_auth_server.Models
{
    public class VerificationRequestModel
    {
        public string VerificationToken;
        public string AuthCode;
        public string PlayerUuid;
        public string? StudentId;
        public int? DeptCode;
        static string _generateAuthCode(int length)
        {
            string guid = Guid.NewGuid().ToString("N").ToUpper();
            string alphanumeric = new string(guid.Where(c => char.IsLetterOrDigit(c)).ToArray());
            string validCharacters = new string(alphanumeric.Where(c => char.IsDigit(c) || (char.IsLetter(c) && char.IsUpper(c))).ToArray());
            string randomString = validCharacters.Substring(0, length);
            return randomString;
        }

        public VerificationRequestModel(string playerUuid)
        {
            VerificationToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            AuthCode = _generateAuthCode(6);
            PlayerUuid = playerUuid;
        }
    }
}
