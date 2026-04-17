using System.Text.RegularExpressions;

public static class NicknameValidator
{
    private static readonly Regex NicknameRegex =
        new Regex(@"^[a-zA-Z°¡-ÆR0-9]{2,8}$");

    public static bool Validate(string nickname, out string error)
    {
        nickname = nickname?.Trim();

        if (string.IsNullOrEmpty(nickname))
        {
            error = "´Ğ³×ÀÓÀ» ÀÔ·ÂÇØÁÖ¼¼¿ä.";
            return false;
        }

        if (nickname.Contains(" "))
        {
            error = "°ø¹éÀº »ç¿ëÇÒ ¼ö ¾ø½À´Ï´Ù.";
            return false;
        }

        if (!NicknameRegex.IsMatch(nickname))
        {
            error = "2~8ÀÚÀÇ ÇÑ±Û, ¿µ¾î, ¼ıÀÚ¸¸ °¡´ÉÇÕ´Ï´Ù.";
            return false;
        }

        error = "";
        return true;
    }
}