namespace BugReportSystem;
using System;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;

internal class TrelloReporter : IReporter
{
    #region Trello config param
    private const string API_KEY = "cff4038ef745ff7294d7038b339e6ff5";
    private const string API_TOKEN = "ATTA4647c7f6dd95ddd6726e5163d389a36014b7f83231b7c6c2b37f82557549253f67616F87";
    private const string BOARD_NAME = "l5mphxTR";
    private const string DEFAULT_LIST_NAME = "Player Bug Report";
    #endregion

    #region Trello variable
    private static string boardID = "";
    private static string listID = "";
    private static string lastCardID = "";
    private static int cardMaxLimit = -1;
    #endregion

    #region REST API
    private string getBoardInfoApi = $"https://api.trello.com/1/boards/{BOARD_NAME}/?fields=limits&key={API_KEY}&token={API_TOKEN}";
    private string getAllCardsApi = $"https://api.trello.com/1/boards/{listID}/cards?key={API_KEY}&token={API_TOKEN}";
    private string getAllListApi = $"https://api.trello.com/1/boards/{boardID}/lists?&key={API_KEY}&token={API_TOKEN}";

    private string createListApi = $"https://api.trello.com/1/lists?name={DEFAULT_LIST_NAME}&idBoard={boardID}&key={API_KEY}&token={API_TOKEN}";
    private string createCardApi = $"https://api.trello.com/1/cards?idList={listID}&key={API_KEY}&token={API_TOKEN}";

    private string delCardApi = $"https://api.trello.com/1/cards/{lastCardID}?key={API_KEY}&token={API_TOKEN}";

    private string updateCardApi = $"";
    #endregion

    private BugReport bugReport;

    private HttpClient client = new HttpClient();

    public TrelloReporter(BugReport bugReport) {
        this.bugReport = bugReport;
    }

    private async Task<bool> GetBoardInfo() {
        try{
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, getBoardInfoApi);
            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            if(responseMessage.IsSuccessStatusCode) {
                string responseBody = await responseMessage.Content.ReadAsStringAsync();
                JsonNode json = JsonNode.Parse(responseBody);
                if(json == null) return false;
                cardMaxLimit = json["limits"]["cards"]["openPerList"]["warnAt"].AsValue().GetValue<int>();
                return json["id"].AsValue().TryGetValue(out boardID);
            }
            return false;
        }catch(Exception e){
            return false;
        }
    }

    private async Task<JsonNode> GetAllCards() {
        try{
            var json = JsonSerializer.Serialize(this.bugReport);
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, getAllCardsApi);
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
        try{
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, getAllListApi);
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
        try
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, createListApi);
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

    private async Task<bool> CreateBugReportCard() {
        try
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, createCardApi);
            requestMessage.Content.
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

    private async Task<bool> DeleteBugReportCard()
    {
        if(lastCardID == "")
            return false;

        try
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Delete, delCardApi);
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

    public async Task<bool> SendReport()
    {
        if (await GetBoardInfo())
            return false;

        JsonNode listsJson = await GetAllLists();
        foreach (JsonNode json in listsJson.AsArray())
        {
            if (json["name"].AsValue().GetValue<string>().Equals(DEFAULT_LIST_NAME))
            {
                listID = json["id"].AsValue().GetValue<string>();
                break;
            }
        }

        if (listID == "")
            if (!await CreateBugReportList()) return false;

        JsonNode cardsJson = await GetAllCards();
        if (cardMaxLimit == -1)
            return false;

        if(cardMaxLimit <= cardsJson.AsArray().Count)
        {
            lastCardID = cardsJson.AsArray()[cardsJson.AsArray().Count - 1]["id"].AsValue().GetValue<string>();
            if(!await DeleteBugReportCard())
                return false;
        }

        return await CreateBugReportCard();
    }
}