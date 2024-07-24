using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

public class APIHandler : MonoBehaviour
{
    //public static string Token { get; private set; } = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjY2OTgxZTA3NTgzMDlmNzZhYTBlNTQ0YSIsImlhdCI6MTcyMTI1MzU4N30.DWkaOhjHFTD789srS4DNI5tbV51DKQmLf3WRP2G6Lgg";
    public static string Token { get; private set; } = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjY2OTgxZTU3NTgzMDlmNzZhYTBlNTQ0ZSIsImlhdCI6MTcyMTY4NjUxNH0.JDgURNMHICwdmCBS0GjaAwxbLaIqSZmVhrTgu0XRlfk";
    
    public static IEnumerator SendRequest(string uri, string method = "GET", string body = "", System.Action<JSONNode> result = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(uri, method))
        {
            byte[] bodyRaw = System.Text.Encoding.ASCII.GetBytes(body);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + User.Token);
            yield return request.SendWebRequest();
            JSONNode response = JSON.Parse(request.downloadHandler.text);
            Debug.Log(response?.ToString());
            result?.Invoke(response);
        }
    }
}
