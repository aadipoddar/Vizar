using MailKit.Net.Smtp;

using MimeKit;

using VizarLibrary.DataAccess;
using VizarLibrary.Models.Common;

namespace VizarLibrary.Exporting;

public static class Mailing
{
	public static async Task SendEmail(string toName, string toEmail, string subject, string htmlBody)
	{
		var message = new MimeMessage();
		message.From.Add(new MailboxAddress("AadiSoft", Secrets.Email));
		message.To.Add(new MailboxAddress(toName, toEmail));
		message.Subject = subject;

		var bodyBuilder = new BodyBuilder
		{
			HtmlBody = htmlBody
		};
		message.Body = bodyBuilder.ToMessageBody();

		using var client = new SmtpClient();
		await client.ConnectAsync("smtp.gmail.com", 465, true);
		await client.AuthenticateAsync(Secrets.Email, Secrets.EmailPassword);
		await client.SendAsync(message);
		await client.DisconnectAsync(true);
	}

	public static async Task SendMailCodeToUser(UserModel user, string code, int codeExpiryMinutes)
	{
		var subject = "Your Login Code for Vizar";
		var htmlBody = GenerateLoginCodeEmailHtml(user, code, codeExpiryMinutes);
		await SendEmail(user.Name, user.Email, subject, htmlBody);
	}

	private static string GenerateLoginCodeEmailHtml(UserModel user, string code, int codeExpiryMinutes)
	{
		return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Your Login Code</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f5f5f5;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse; background-color: #f5f5f5;"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table role=""presentation"" style=""width: 600px; max-width: 100%; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">
                    <!-- Header -->
                    <tr>
                        <td style=""padding: 40px 40px 30px 40px; text-align: center; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border-radius: 8px 8px 0 0;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 600; letter-spacing: -0.5px;"">Vizar</h1>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px;"">
                            <h2 style=""margin: 0 0 20px 0; color: #333333; font-size: 24px; font-weight: 600;"">Hello {user.Name},</h2>
                            <p style=""margin: 0 0 30px 0; color: #666666; font-size: 16px; line-height: 1.6;"">
                                You've requested a login code for your Vizar account. Use the code below to complete your sign-in:
                            </p>
                            
                            <!-- Code Box -->
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 30px 0;"">
                                <tr>
                                    <td align=""center"" style=""padding: 30px; background-color: #f8f9fa; border-radius: 8px; border: 2px dashed #e0e0e0;"">
                                        <div style=""font-size: 36px; font-weight: 700; color: #667eea; letter-spacing: 8px; font-family: 'Courier New', monospace;"">{code}</div>
                                    </td>
                                </tr>
                            </table>
                            
                            <p style=""margin: 30px 0 20px 0; color: #666666; font-size: 16px; line-height: 1.6;"">
                                This code will expire in <strong>{codeExpiryMinutes} minutes</strong> for your security.
                            </p>
                            
                            <!-- CTA Button -->
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 20px 0;"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""https://vizar.azurewebsites.net/login-with-code-redirect/{user.Id}/{code}"" style=""display: inline-block; padding: 14px 40px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);"">Go to Vizar Login</a>
                                    </td>
                                </tr>
                            </table>
                            
                            <!-- Warning Box -->
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse; margin: 30px 0;"">
                                <tr>
                                    <td style=""padding: 20px; background-color: #fff3cd; border-left: 4px solid #ffc107; border-radius: 4px;"">
                                        <p style=""margin: 0; color: #856404; font-size: 14px; line-height: 1.6;"">
                                            <strong>⚠️ Security Notice:</strong> If you didn't request this code, please ignore this email and ensure your account is secure.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style=""padding: 30px 40px; background-color: #f8f9fa; border-radius: 0 0 8px 8px; border-top: 1px solid #e0e0e0;"">
                            <p style=""margin: 0 0 10px 0; color: #999999; font-size: 14px; line-height: 1.6;"">
                                Best regards,<br>
                                <strong style=""color: #667eea;"">The AadiSoft Team</strong>
                            </p>
                            <p style=""margin: 20px 0 0 0; color: #999999; font-size: 12px; line-height: 1.6;"">
                                This is an automated message, please do not reply to this email.<br>
                                <a href=""https://aadisoft.vercel.app/"" style=""color: #667eea; text-decoration: none;"">Visit our website</a>
                            </p>
                        </td>
                    </tr>
                </table>
                
                <!-- Footer Text -->
                <table role=""presentation"" style=""width: 600px; max-width: 100%; border-collapse: collapse; margin-top: 20px;"">
                    <tr>
                        <td align=""center"" style=""padding: 0 40px;"">
                            <p style=""margin: 0; color: #999999; font-size: 12px; line-height: 1.6;"">
                                © 2025 AadiSoft. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
	}
}