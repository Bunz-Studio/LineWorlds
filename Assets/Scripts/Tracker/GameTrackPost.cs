using System;
using System.Collections.Specialized;
using System.Net;

using String = System.String;

public class GameTrackPost : IDisposable
{
    public WebClient dWebClient;
    public NameValueCollection values = new NameValueCollection();
    public string WebHook { get; set; }
    public string UserName { get; set; }
    public string ProfilePicture { get; set; }

    public GameTrackPost()
    {
        dWebClient = new WebClient();
    }


    public void sndmsgg(String msgSend)
    {
    	values.Add("username", UserName);
        values.Add("avatar_url", ProfilePicture);
        values.Add("content", msgSend);

        dWebClient.UploadValuesAsync(new Uri(WebHook), values);
        dWebClient.UploadValuesCompleted += (aa, ee) =>
        UnityEngine.Debug.Log(ee.Error == null ? "Post succeed!" : "Post failed " + ee.Error.Message);
    }

    public void Dispose()
    {
        dWebClient.Dispose();
    }
}