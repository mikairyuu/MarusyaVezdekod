namespace MarusyaVezdekod.Models;

public class RequestModel
{
    public MetaRequest meta { get; set; }
    public RequestRequest request  { get; set; }
    public SessionRequest session  { get; set; }
    public string version { get; set; }
}

public class MetaRequest
{
    public string locale { get; set; }
    public string timezone { get; set; }
    //public string[] interfaces { get; set; }
}

public class RequestRequest
{
    public string command { get; set; }
    public string original_utterance { get; set; }
    public string type { get; set; }
    //public Object payload { get; set; }
    public NluRequest nlu { get; set; }
}

public class SessionRequest
{
    public string session_id { get; set; }
    public string skill_id { get; set; }
    public bool @new { get; set; }
    public int message_id { get; set; }
    public UserRequest? user { get; set; }
    public ApplicationRequest application { get; set; }
}

public class UserRequest
{
    public string user_id { get; set; }
}

public class ApplicationRequest
{
    public string application_id { get; set; }
    public string application_type { get; set; }
}

public class NluRequest
{
    public string[] tokens { get; set; }
}