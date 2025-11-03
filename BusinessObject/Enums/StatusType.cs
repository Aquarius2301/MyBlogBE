using System;

namespace BusinessObject.Enums;

public enum StatusType
{
    /// <summary>
    /// The account is inactive and cannot log in or perform any actions.
    /// </summary>
    InActive,
    /// <summary>
    /// The account is active and can log in, post content, and comment.
    /// </summary>
    Active,
    /// <summary>
    /// The account is suspended (can log in and view content but cannot post and comment).
    /// </summary>
    Suspended
}

