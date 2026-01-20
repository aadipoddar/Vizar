using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Operations;

namespace VizarLibrary.Exporting;

public static class Mailing
{
    public static async Task SendEmail(string toName, string toEmail, string subject, string htmlBody)
    {
        // Deprecated - kept for backward compatibility
        // This method should no longer be used directly
        throw new NotImplementedException("Please use MailingUtil.SendLoginCodeEmail instead");
    }

    public static async Task SendMailCodeToUser(UserModel user, string code, string redirectLink, int codeExpiryMinutes)
    {
        await MailingUtil.SendLoginCodeEmail(user, code, redirectLink, codeExpiryMinutes);
    }
}