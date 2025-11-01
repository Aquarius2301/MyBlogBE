using System;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace WebApi.Helpers;

public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int EmailTokenTimeoutMinute { get; set; }
    public int EmailTokenLength { get; set; }
}

public class EmailHelper
{
    private readonly EmailSettings _settings;
    public int TokenTimeOut => _settings.EmailTokenTimeoutMinute;
    public int TokenLength => _settings.EmailTokenLength;

    public EmailHelper(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

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

