using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SemanticKernelPlayground;

public class TodayPlugin
{
    [KernelFunction("get_today")]
    [Description("Gets the current date and time")]
    [return:Description("The current date and time")]
    public DateTime GetToday(Kernel kernel)
    {
        return DateTime.Now;
    }
}