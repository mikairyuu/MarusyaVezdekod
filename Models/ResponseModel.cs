namespace MarusyaVezdekod.Models;

public class ResponseModel
{
    public ResponseResponse response { get; set; }
    public SessionResponse session { get; set; }
    public string version { get; set; } = "1.0";
}

public class SessionResponse
{
    public string session_id { get; set; }
    public string user_id { get; set; }
    public int message_id { get; set; } 
}

public class ResponseResponse
{
    public string tts { get; set; } = "";
    public string[] text { get; set; }
    public Button[] buttons { get; set; } = { };
    public bool end_session { get; set; }
    public CardCommon card { get; set; } = new();
}

public class Button
{
    public string title { get; set; }
    public string url { get; set; }
}

public class CardCommon
{
    public string type { get; set; }
    public int image_id { get; set; }
}
