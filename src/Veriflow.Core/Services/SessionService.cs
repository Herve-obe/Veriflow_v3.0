using System.Text.Json;
using Veriflow.Core.Interfaces;
using Veriflow.Core.Models;

namespace Veriflow.Core.Services;

/// <summary>
/// Session management service
/// </summary>
public class SessionService : ISessionService
{
    private Session _currentSession;
    
    public Session CurrentSession => _currentSession;
    
    public event EventHandler? SessionChanged;
    
    public SessionService()
    {
        _currentSession = new Session();
    }
    
    public void NewSession()
    {
        _currentSession = new Session();
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }
    
    public async Task LoadSessionAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        var session = JsonSerializer.Deserialize<Session>(json);
        
        if (session != null)
        {
            _currentSession = session;
            _currentSession.FilePath = filePath;
            _currentSession.IsModified = false;
            SessionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public async Task SaveSessionAsync(string? filePath = null, CancellationToken cancellationToken = default)
    {
        filePath ??= _currentSession.FilePath;
        
        if (string.IsNullOrEmpty(filePath))
        {
            throw new InvalidOperationException("No file path specified for session save");
        }
        
        _currentSession.ModifiedDate = DateTime.Now;
        _currentSession.FilePath = filePath;
        
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(_currentSession, options);
        
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
        
        _currentSession.IsModified = false;
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }
    
    public void MarkAsModified()
    {
        _currentSession.IsModified = true;
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }
}
