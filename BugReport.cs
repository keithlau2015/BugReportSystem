using System.Threading.Tasks;
using System;
namespace BugReportSystem;
public class BugReport
{
    private string title;
    private string summary;
    private string sysInfo;
    private DateTime sendTime;

    public Task<bool> SendReport(){
        IReporter reporter = new TrelloReporter(this);
        return reporter.SendReport();
    }
}