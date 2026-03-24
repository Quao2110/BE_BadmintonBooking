using System;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices;

public interface IAiService
{
    /// <summary>
    /// Generate a reply for a user's message. Implementation may use rule-based logic or call external AI provider.
    /// </summary>
    /// <param name="userMessage">Incoming user message</param>
    /// <param name="userId">User id who sent the message</param>
    /// <returns>Reply text to send to user</returns>
    Task<string> GenerateReplyAsync(string userMessage, Guid userId);
}
