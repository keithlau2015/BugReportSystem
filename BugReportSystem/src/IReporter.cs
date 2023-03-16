namespace BugReportSystem;
using System.Threading.Tasks;
internal interface IReporter {
    public Task<bool> SendReport(BugReport bugReport);
}