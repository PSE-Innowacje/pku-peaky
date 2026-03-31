namespace PKU.Domain.Entities;

/// <summary>
/// Declaration statuses per PRD section 11.
/// </summary>
public enum DeclarationStatus
{
    /// <summary>Not submitted - no declaration received for the period</summary>
    NotSubmitted,

    /// <summary>Draft - declaration saved but not yet sent</summary>
    Draft,

    /// <summary>Submitted - declaration submitted and data exported to file</summary>
    Submitted
}
