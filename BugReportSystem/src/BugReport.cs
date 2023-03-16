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
    }

    public struct BugReportParam
    {
        public SupportItemIndex index;
        public bool isActive;
        public Dictionary<string, string> parameters;
        public BugReportParam(){
            this.parameters = new Dictionary<string, string>();
        }
    }
    
    public string title;
    public string summary;
    public string sysInfo;
    public byte[] attachment;
    public DateTime sendTime;
    
    /// <summary>
    /// contain all supported ways to send bug report
    /// </summary>
    public Dictionary<SupportItemIndex, BugReportParam> allItems { get; private set; }= new Dictionary<SupportItemIndex, BugReportParam>();

    public BugReport(Dictionary<SupportItemIndex, bool> activation){        
        #region Trello
        BugReportParam trelloParam = new BugReportParam();
        trelloParam.index = SupportItemIndex.Trello;
        trelloParam.isActive = true;
        trelloParam.parameters.Add("API_KEY", "YOUR API_KEY");
        trelloParam.parameters.Add("API_TOKEN", "YOUR API_TOKEN");
        trelloParam.parameters.Add("BOARD_NAME", "YOUR BOARD ID");
        trelloParam.parameters.Add("DEFAULT_LIST_NAME", "YOUR LIST NAME");
        allItems.Add(trelloParam.index, trelloParam);
        #endregion

        foreach (KeyValuePair<SupportItemIndex, bool> item in activation)
        {
            BugReportParam bugReportParam;
            if(allItems.TryGetValue(item.Key, out bugReportParam))
                bugReportParam.isActive = item.Value;
        }
    }

    /// <summary>
    /// Send Report to all supported ways
    /// </summary>
    public async Task<bool> SendReport(){
        bool result = true;
        foreach (var item in allItems.Values)
        {
            if(!item.isActive) continue;
            IReporter reporter = new TrelloReporter(item.parameters);
            result = await reporter.SendReport(this);
        }
        return result;
    }
}