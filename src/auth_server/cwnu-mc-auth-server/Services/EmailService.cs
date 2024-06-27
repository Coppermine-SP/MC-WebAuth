using System.Configuration;
using Azure;
using Azure.Communication.Email;

namespace cwnu_mc_auth_server.Services
{
    public class EmailTemplate
    {
        public string Subject { get; set; }
        public string HtmlContent { get; set; }

        public EmailTemplate(string subject, string htmlContent)
        {
            Subject = subject;
            HtmlContent = htmlContent;
        }

        public EmailTemplate()
        {
            Subject = "";
            HtmlContent = "";
        }
    }

    public class VerificationEmailTemplate : EmailTemplate
    {
        public VerificationEmailTemplate(string authUrl, string name)
        {
            Subject = "재학생 인증 완료하기";
            HtmlContent = $"""
                <h2>{name}님, 안녕하세요!</h2>
                <p>아래 링크를 클릭하여 창원대학교 재학생 인증을 완료하세요:<p>
                <a href={authUrl}>{authUrl}</a>
                """;
        }
    }

    public interface IEmailService
    {
        public bool Send(string recipient, EmailTemplate template);
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<VerificationService> _logger;
        private readonly IConfiguration _config;
        private readonly string? _connectionString;
        private readonly string? _senderAddress;

        public EmailService(ILogger<VerificationService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _connectionString = _config.GetValue<string>("azureCommConnectionString");
            _senderAddress = _config.GetValue<string>("azureCommSenderAddress");

            if (_connectionString is null || String.IsNullOrWhiteSpace(_connectionString))
            {
                _logger.LogCritical("azureCommConnectionString is empty or not configured! Check appsettings.json.");
                throw new ConfigurationErrorsException();
            }

            if (_senderAddress is null || String.IsNullOrWhiteSpace(_senderAddress))
            {
                _logger.LogCritical("azureCommSenderAddress is empty or not configured! Check appsettings.json.");
                throw new ConfigurationErrorsException();
            }
        }

        public bool Send(string recipient, EmailTemplate template)
        {
            _logger.LogInformation($"Send email to {recipient} via {_senderAddress}.");

            try
            {
                var client = new EmailClient(_connectionString);
                EmailSendOperation operation = client.Send(
                    WaitUntil.Completed,
                    senderAddress: _senderAddress,
                    recipientAddress: recipient,
                    subject: template.Subject,
                    htmlContent: template.HtmlContent
                );

                if (operation.Value.Status == EmailSendStatus.Failed)
                {
                    _logger.LogWarning($"Send email to {recipient} was failed. (#{operation.Id})");
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Send email to {recipient} was failed due exception:\n" + e);
                return false;
            }

            return true;
        }
    }
}
