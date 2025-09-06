namespace woopmotd.Config;

public class ModConfig
{
    // Enable/disable MOTD popup on login
    public bool EnableMotd { get; set; } = true;

    // VTML contents sent by server to clients (multiple messages supported).  This is largely for demonstration; in practice, server admins will edit the file.  This also acts as the fallback if the serverside formatting is invalid.
    public string[] MotdVtmls { get; set; } =
    {
        "<strong><font color=\"gold\">Welcome!</color></strong><br>Enjoy your stay on our server.",
        "Ruh Roh Raggie, It looks like you found a second message!<br>Contact an admin if you see this."
    };

}