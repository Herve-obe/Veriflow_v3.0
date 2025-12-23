using Veriflow.Core.Models;

namespace Veriflow.Core.Interfaces;

/// <summary>
/// Interface for session management
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Get the current session
    /// </summary>
    Session CurrentSession { get; }
    
    /// <summary>
    /// Create a new session
    /// </summary>
    void NewSession();
    
    /// <summary>
    /// Load a session from file
    /// </summary>
    Task LoadSessionAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Save the current session
    /// </summary>
    Task SaveSessionAsync(string? filePath = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Mark session as modified
    /// </summary>
    void MarkAsModified();
    
    /// <summary>
    /// Event raised when session changes
    /// </summary>
    event EventHandler? SessionChanged;
}
