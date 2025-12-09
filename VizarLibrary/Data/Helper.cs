using System.Globalization;

namespace VizarLibrary.Data;

public static class Helper
{
    public static string RemoveSpace(this string str) =>
        str.Replace(" ", "");

    public static string FormatIndianCurrency(this decimal rate) =>
        string.Format(new CultureInfo("hi-IN"), "{0:C}", rate);

    public static string FormatIndianCurrency(this decimal? rate)
    {
        rate ??= 0;
        return string.Format(new CultureInfo("hi-IN"), "{0:C}", rate);
    }

    public static string FormatIndianCurrency(this int rate) =>
        string.Format(new CultureInfo("hi-IN"), "{0:C}", rate);

    public static string FormatDecimalWithTwoDigits(this decimal value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture);

    public static bool ValidatePhoneNumber(this string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;
        if (phoneNumber.Length != 10)
            return false;
        return long.TryParse(phoneNumber, out _);
	}

    public static bool ValidateEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
		}
	}
}
