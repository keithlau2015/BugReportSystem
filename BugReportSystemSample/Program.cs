using BugReportSystem;

public class Program{
    public static async Task Main(string[] args)
    {
        //Create Bug report instance
        BugReport bugReport = new BugReport();

        //Every plateform may have different parameters
        //In this case, using trello, there are few parameters you must to filled in, and able to use the trello api
        bugReport.allItems[BugReport.SupportItemIndex.Trello].parameters["API_KEY"] = "YOUR_API_KEY";
        bugReport.allItems[BugReport.SupportItemIndex.Trello].parameters["API_TOKEN"] = "YOUR_API_TOKEN";
        bugReport.allItems[BugReport.SupportItemIndex.Trello].parameters["BOARD_NAME"] = "YOUR_BOARD_ID";
        bugReport.allItems[BugReport.SupportItemIndex.Trello].parameters["DEFAULT_LIST_NAME"] = "YOUR_CARD_LIST_DEFAULT_NAME";

        //The actual report context
        bugReport.title = "Obiwan Bug Report";
        bugReport.summary = "Hello There!";
        bugReport.sendTime = DateTime.Now;
        //attachment is optional
        //bugReport.attachment = File.ReadAllBytes("image.jpg");
    
        //If return true, that's mean all the operation are successful executed
        bool success = await bugReport.SendReport();
        
        Console.WriteLine(success);
    }
}