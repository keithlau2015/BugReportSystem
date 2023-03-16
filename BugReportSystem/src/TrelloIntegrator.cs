namespace BugReportSystem;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;

internal class TrelloReporter : IReporter
{
    #region Trello config param
    private string API_KEY = "";
    private string API_TOKEN = "";
    private string BOARD_NAME = "";
    private string DEFAULT_LIST_NAME = "";
    #endregion

    #region Trello variable
    private string boardID = "";
    private string listID = "";
    private string lastCardID = "";
    private int cardMaxLimit = -1;
    #endregion

    #region REST API
    private string getBoardInfoApi = "https://api.trello.com/1/boards/{0}/?fields=limits&key={1}&token={2}";
    private string getAllCardsApi = "https://api.trello.com/1/boards/{0}/cards?key={1}&token={2}";
    private string getAllListApi = "https://api.trello.com/1/boards/{0}/lists?&key={1}&token={2}";

    private string createListApi = "https://api.trello.com/1/lists?name={0}&idBoard={1}&key={2}&token={3}";
    private string createCardApi = "https://api.trello.com/1/cards?idList={0}&pos=top&key={1}&token={2}";

    private string delCardApi = "https://api.trello.com/1/cards/{0}?key={1}&token={2}";

    private string updateCardApi = "https://api.trello.com/1/cards/{0}/attachments?key={1}&token={2}";
    #endregion

    private HttpClient client = new HttpClient();

    public TrelloReporter(Dictionary<string, string> parameters) {
        API_KEY             = parameters["API_KEY"];
        API_TOKEN           = parameters["API_TOKEN"];
        BOARD_NAME          = parameters["BOARD_NAME"];
        DEFAULT_LIST_NAME   = parameters["DEFAULT_LIST_NAME"];
    }

    private async Task<bool> GetBoardInfo() {
        Console.WriteLine("Get Board Info");
        try{
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, string.Format(getBoardInfoApi, BOARD_NAME, API_KEY, API_TOKEN));
            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            Console.WriteLine(responseMessage.StatusCode);
            if(responseMessage.IsSuccessStatusCode) {
                string responseBody = await responseMessage.Content.ReadAsStringAsync();
                JsonNode json = JsonNode.Parse(responseBody);
                if(json == null) return false;
                cardMaxLimit = json["limits"]["cards"]["openPerList"]["warnAt"].AsValue().GetValue<int>();
                boardID = json["id"].AsValue().GetValue<string>();
                Console.WriteLine("cardMaxLimit: " + cardMaxLimit);
                Console.WriteLine("boardID: " + boardID);
                return true;
            }
            return false;
        }catch(Exception e){
            return false;
        }
    }

    private async Task<JsonNode> GetAllCards() {
        Console.WriteLine("Get All Cards");
        try{
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, string.Format(getAllCardsApi, this.listID, API_KEY, API_TOKEN));
            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            if(responseMessage.IsSuccessStatusCode) {
                string responseBody = await responseMessage.Content.ReadAsStringAsync();
                return JsonNode.Parse(responseBody);
            }
            return null;
        }catch(Exception e){
            return null;
        }
    }

    private async Task<JsonNode> GetAllLists() {
        Console.WriteLine("Get All Lists");
        try{
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, string.Format(getAllListApi, this.boardID, API_KEY, API_TOKEN));
            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            if(responseMessage.IsSuccessStatusCode) {
                string responseBody = await responseMessage.Content.ReadAsStringAsync();
                return JsonNode.Parse(responseBody);
            }
            return null;
        }catch(Exception e){
            return null;
        }
    }

    private async Task<bool> CreateBugReportList() {
        Console.WriteLine("Create Bug Report List");
        try
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, string.Format(createListApi, DEFAULT_LIST_NAME, boardID, API_KEY, API_TOKEN));
            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            if (responseMessage.IsSuccessStatusCode) {
                string responseBody = await responseMessage.Content.ReadAsStringAsync();
                JsonNode json = JsonNode.Parse(responseBody);
                if(json == null) return false;
                listID = json["id"].AsValue().GetValue<string>();
                return true;
            }
            else
                return false;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    private async Task<bool> CreateBugReportCard(BugReport bugReport) {
        Console.WriteLine("Create Bug Report Card");
        try
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, string.Format(createCardApi, this.listID, API_KEY, API_TOKEN));
            string data = "{\"name\": \"" + bugReport.title + "\", \"desc\": \"System Info:\\n" + bugReport.sysInfo + "\\nSummary:\\n" + bugReport.summary + "\\nSend Time:\\n" + bugReport.sendTime.ToString("G", CultureInfo.GetCultureInfo("de-DE")) + "\"}";
            requestMessage.Content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            if (responseMessage.IsSuccessStatusCode){
                //Dont have attachment
                if(bugReport.attachment == null || bugReport.attachment.Length == 0)
                    return true;
                
                string responseBody = await responseMessage.Content.ReadAsStringAsync();
                JsonNode json = JsonNode.Parse(responseBody);
                if(json == null) return false;
                lastCardID = json["id"].AsValue().GetValue<string>();

                //update with a screenshot
                HttpRequestMessage updateRequestMessage = new HttpRequestMessage(HttpMethod.Post, string.Format(updateCardApi, this.lastCardID, API_KEY, API_TOKEN));
                MultipartFormDataContent content = new MultipartFormDataContent();
                // Add the attachment file to the content
                ByteArrayContent fileContent = new ByteArrayContent(bugReport.attachment);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg"); // Replace "image/jpeg" with the appropriate MIME type for your image file
                content.Add(fileContent, "file", "attachment-name.jpg"); // Replace "attachment-name.jpg" with the name of your attachment file

                // Add the other parameters to the content
                StringContent mimeTypeContent = new StringContent("image/jpeg"); // Replace "image/jpeg" with the appropriate MIME type for your image file
                content.Add(mimeTypeContent, "mimeType");

                // Set the content of the request message
                updateRequestMessage.Content = content;

                HttpResponseMessage updateResponseMessage = await client.SendAsync(updateRequestMessage);
                if(updateResponseMessage.IsSuccessStatusCode)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    private async Task<bool> DeleteBugReportCard()
    {
        Console.WriteLine("Delete Bug Report Card");
        if(lastCardID == "")
            return false;

        try
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Delete, string.Format(delCardApi, lastCardID, API_KEY, API_TOKEN));
            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            if (responseMessage.IsSuccessStatusCode)
                return true;
            else
                return false;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<bool> SendReport(BugReport bugReport)
    {
        Console.WriteLine("Send Report");
        if (!await GetBoardInfo()){
            Console.WriteLine("GetBoardInfo error");
            return false;
        }

        JsonNode listsJson = await GetAllLists();
        foreach (JsonNode json in listsJson.AsArray())
        {
            if (json["name"].AsValue().GetValue<string>().Equals(DEFAULT_LIST_NAME))
            {
                listID = json["id"].AsValue().GetValue<string>();
                break;
            }
        }

        if (listID == "") {
            if (!await CreateBugReportList()) {
                Console.WriteLine("CreateBugReportList error");
                return false;
            }
        }

        JsonNode cardsJson = await GetAllCards();
        if (cardMaxLimit == -1){
            Console.WriteLine("cardMaxLimit == -1");
            return false;
        }

        if(cardsJson != null && cardMaxLimit <= cardsJson.AsArray().Count)
        {
            lastCardID = cardsJson.AsArray()[cardsJson.AsArray().Count - 1]["id"].AsValue().GetValue<string>();
            if(!await DeleteBugReportCard()){
                Console.WriteLine("DeleteBugReportCard error");
                return false;
            }
        }

        return await CreateBugReportCard(bugReport);
    }
}