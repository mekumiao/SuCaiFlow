using Microsoft.EntityFrameworkCore;

namespace SuCaiFlow.EntityFramework.Infrastructure;

/// <summary>
/// Provides options for configuring SuCaiFlow Entity Framework Core integration.
/// </summary>
public class SuCaiFlowEntityFrameworkCoreOptions
{
    /// <summary>
    /// Gets or sets the type of the database context used by SuCaiFlow.
    /// </summary>
    public Type? ContextType { get; set; }
}
