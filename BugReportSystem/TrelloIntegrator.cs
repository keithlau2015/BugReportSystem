namespace BugReportSystem;
using System;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;

internal class TrelloReporter : IReporter
{
    private HttpClient client = new HttpClient();
    private const string API_KEY = "cff4038ef745ff7294d7038b339e6ff5";
    private const string API_TOKEN = "ATTA4647c7f6dd95ddd6726e5163d389a36014b7f83231b7c6c2b37f82557549253f67616F87";
    private const string BOARD_ID = "l5mphxTR";
    private const string DEFAULT_BOARD_NAME = "Player Bug Report";

    private static string boardID = "";

    #region REST API
    private string getBoardApi = $"https://api.trello.com/1/boards/{BOARD_ID}?key={API_KEY}&token={API_TOKEN}";
    private string getBoardLimitApi = $"";
    private string createListApi = $"https://api.trello.com/1/lists?name={DEFAULT_BOARD_NAME}&idBoard={boardID}&key={API_KEY}&token={API_TOKEN}";
    private string createCardApi = "";
    #endregion

    private BugReport bugReport;
    public bool IsConnected {
        get{
            return false;
        }
    }
    public TrelloReporter(BugReport bugReport) {
        this.bugReport = bugReport;
    }

    public override async Task<bool> SendReport(){
        //if board not exits
        if(!await IsBoardExists()) {
            //create new board
            
        }
        
        //if card is over limit
        if(await IsCardOverLimit()){

        }

        return await CreateBugReportCard();
    }

    private async Task<bool> IsBoardExists() {
        try{
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, getBoardApi);
            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            if(responseMessage.IsSuccessStatusCode) {
                string responseBody = await responseMessage.Content.ReadAsStringAsync();
                JsonNode json = JsonNode.Parse(responseBody);                
                return json["id"].AsValue().TryGetValue(out boardID);
            }
            return false;
        }catch(Exception e){
            return false;
        }
    }

    private async Task<bool> IsCardOverLimit() {
        try{
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, getBoardLimitApi);
            
            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            if(responseMessage.IsSuccessStatusCode) {
                return true;
            }
            return false;
        }catch(Exception e){
            return false;
        }
    }

    private async Task<bool> CreateBugReportCard() {
        try{
            var json = JsonSerializer.Serialize(this.bugReport);
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, createCardApi);
            await client.GetAsync(createCardApi);
            return true;
        }catch(Exception e){
            return false;
        }
    }
}