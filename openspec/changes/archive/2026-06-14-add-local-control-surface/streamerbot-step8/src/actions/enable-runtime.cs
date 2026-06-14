using System;

public class CPHInline
{
    public bool Execute()
    {
        return CallLocalControl("Enable Runtime", "POST", "/api/v1/runtime/enable", "");
    }

    private bool CallLocalControl(string label, string method, string path, string body)
    {
        string url = CreateUrl(path);
        try
        {
            using (var client = new System.Net.WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                client.Headers[System.Net.HttpRequestHeader.Accept] = "application/json";
                string response = method == "GET"
                    ? client.DownloadString(url)
                    : UploadJson(client, url, method, body ?? "");
                CPH.LogInfo($"[MSLC] {label} {method} {url} -> {response}");
                return true;
            }
        }
        catch (System.Net.WebException ex)
        {
            CPH.LogError($"[MSLC] {label} failed: {ReadError(ex)}");
            return false;
        }
        catch (Exception ex)
        {
            CPH.LogError($"[MSLC] {label} failed: {ex.Message}");
            return false;
        }
    }

    private string CreateUrl(string path)
    {
        string baseUrl = CPH.GetGlobalVar<string>("mouseShenanigans.localControl.baseUrl", true);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "http://127.0.0.1:5178";
        }

        return baseUrl.TrimEnd('/') + path;
    }

    private static string UploadJson(System.Net.WebClient client, string url, string method, string body)
    {
        client.Headers[System.Net.HttpRequestHeader.ContentType] = "application/json";
        return client.UploadString(url, method, body);
    }

    private static string ReadError(System.Net.WebException ex)
    {
        if (ex.Response == null)
        {
            return ex.Message;
        }

        using (var stream = ex.Response.GetResponseStream())
        using (var reader = new System.IO.StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }
}
