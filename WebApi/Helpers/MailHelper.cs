using System;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace WebApi.Helpers;

/// <summary>
/// Configuration settings for sending emails.
/// </summary>
public class EmailSettings
{

    /// <summary>
    /// SMTP server address.
    /// </summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Display name of the email sender.
    /// </summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the sender.
    /// </summary>
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>
    /// Username for SMTP authentication.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for SMTP authentication.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Expiration time (in minutes) for email verification tokens.
    /// </summary>
    public int EmailTokenTimeoutMinute { get; set; }

    /// <summary>
    /// Length of the email verification token.
    /// </summary>
    public int EmailTokenLength { get; set; }
}

/// <summary>
/// Helper class for sending emails such as registration and password reset emails.
/// </summary>
public class EmailHelper
{
    private readonly EmailSettings _settings;

    /// <summary>
    /// Gets the token timeout duration in minutes.
    /// </summary>
    public int TokenTimeOut => _settings.EmailTokenTimeoutMinute;

    /// <summary>
    /// Gets the length of email verification tokens.
    /// </summary>
    public int TokenLength => _settings.EmailTokenLength;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailHelper"/> class.
    /// </summary>
    /// <param name="options">The configured email settings.</param>
    public EmailHelper(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    /// <summary>
    /// Sends an email to the specified recipient.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="subject">Subject of the email.</param>
    /// <param name="body">Body content of the email.</param>
    /// <param name="isHtml">Indicates whether the body is HTML. Defaults to true.</param>
    /// <returns>A task representing the asynchronous email sending operation.</returns>
    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var builder = new BodyBuilder();
        if (isHtml)
            builder.HtmlBody = body;
        else
            builder.TextBody = body;

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpServer, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.Username, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    /// <summary>
    /// Sends a registration confirmation email to a new user.
    /// </summary>
    /// <param name="email">Recipient email address.</param>
    /// <param name="username">Username of the new account.</param>
    /// <param name="confirmCode">Confirmation code for email verification.</param>
    /// <returns>A task representing the asynchronous email sending operation.</returns>
    /// <remarks>
    /// The email includes a link to confirm the account.  
    /// The confirmation link expires after <see cref="TokenTimeOut"/> minutes.
    /// </remarks>
    public async Task SendRegisterEmailAsync(string email, string username, string confirmCode)
    {
        var emailTokenTimeout = _settings.EmailTokenTimeoutMinute;

        await SendEmailAsync(
                     to: email,
                     subject: "Xác nhận đăng ký tài khoản MyBlog",
                     body: $"<h2>Xin chào {username}</h2><br/>" +
                             "<p>Bạn đã đăng ký tài khoản thành công trên MyBlog.</p>" +
                             "<p>Vui lòng hãy nhấn vào liên kết này để xác thực email:</p>" +
                             $"<a href='https://myblog.example.com/confirm?type=register&token={confirmCode}'>Xác nhận tài khoản</a><br/><br/>" +
                             $"<p>Đường link này chỉ tồn tại trong {emailTokenTimeout} phút.</p><br/>" +
                             "<p>Nếu bạn không đăng ký tài khoản này, vui lòng bỏ qua email này.</p>" +
                             "<br/><p>Trân trọng,<br/>Đội ngũ MyBlog</p>"
                 );
    }

    /// <summary>
    /// Sends a password reset email to the user.
    /// </summary>
    /// <param name="email">Recipient email address.</param>
    /// <param name="username">Username of the account.</param>
    /// <param name="confirmCode">Confirmation code for password reset.</param>
    /// <returns>A task representing the asynchronous email sending operation.</returns>
    /// <remarks>
    /// The email includes a link to reset the password.  
    /// The confirmation link expires after <see cref="TokenTimeOut"/> minutes.
    /// </remarks>
    public async Task SendForgotPasswordEmailAsync(string email, string username, string confirmCode)
    {
        var emailTokenTimeout = _settings.EmailTokenTimeoutMinute;

        await SendEmailAsync(
                     to: email,
                     subject: "Xác nhận quên mật khẩu tài khoản MyBlog",
                     body: $"<h2>Xin chào {username}</h2><br/>" +
                             "<p>Bạn đã quên mật khẩu trên MyBlog.</p>" +
                             "<p>Vui lòng hãy nhấn vào liên kết này để xác thực email:</p>" +
                             $"<a href='https://myblog.example.com/confirm?type=forgotPassword&token={confirmCode}'>Xác nhận tài khoản</a><br/><br/>" +
                             $"<p>Đường link này chỉ tồn tại trong {emailTokenTimeout} phút.</p><br/>" +
                             "<p>Nếu bạn không sử dụng quên mật khẩu, vui lòng bỏ qua email này.</p>" +
                             "<br/><p>Trân trọng,<br/>Đội ngũ MyBlog</p>"
                 );
    }

}

