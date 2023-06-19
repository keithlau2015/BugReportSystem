using System.Threading.Tasks;
using System;
namespace BugReportSystem;
public class BugReport
{
    public enum SupportItemIndex {
        /// <summary>
        /// BugReportParam parameters:
        /// API_KEY
        /// API_TOKEN
        /// BOARD_NAME
        /// DEFAULT_LIST_NAME
        /// </summary>
        Trello = 0,
        Notion = 1,
    }

    public struct BugReportParam
    {
        public SupportItemIndex index;
        public bool isActive;
        public Dictionary<string, string> parameters;
    }
    
    public string title;
    public string summary;
    public string sysInfo;
    public byte[] attachment;
    public DateTime sendTime;
    
    /// <summary>
    /// contain all supported ways to send bug report
    /// </summary>
    public Dictionary<SupportItemIndex, BugReportParam> allItems { get; private set; } = new Dictionary<SupportItemIndex, BugReportParam>(){
        {
            SupportItemIndex.Trello, 
            new BugReportParam(){ 
                index = SupportItemIndex.Trello,
                isActive = true,
                parameters = new Dictionary<string, string>(){
                    {"API_KEY", ""},
                    {"API_TOKEN", ""},
                    {"BOARD_NAME", ""},
                    {"DEFAULT_LIST_NAME", ""}
                }
            }
        }
    };

    /// <summary>
    /// Send Report to all supported ways
    /// </summary>
    public async Task<bool> SendReport(){
        bool result = true;
        foreach (var item in allItems.Values)
        {
            if(!item.isActive) continue;
            
            IReporter reporter = null;
            if(item.index == SupportItemIndex.Trello)
                reporter = new TrelloReporter(item.parameters);                
            if(reporter == null)
                return false;
            
            result = await reporter.SendReport(this);
        }
        return result;
    }
}