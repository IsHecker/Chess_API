using SimpleJSON;

public class User
{
    public static string Id { get; private set; }
    public static string Token { get; private set; }
    public static string Username { get; private set; }
    public static string Role { get; private set; }

    public static void CreateUser(JSONObject user)
    {
        Token = user["token"];

        Id = user["data"]["user"]["_id"];
        Username = user["data"]["user"]["username"];
        Role = user["data"]["user"]["role"];
    }
}
