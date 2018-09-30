namespace Telefrek.LDAP
{
    /// <summary>
    /// Current LDAP Result Codes
    /// </summary>
    public enum LDAPResultCode
    {
        /// <value></value>
        Success = 0,
        /// <value></value>
        OperationError = 1,
        /// <value></value>
        ProtocolError = 2,
        /// <value></value>
        TimeLimitExceeded = 3,
        /// <value></value>
        SizeLimitExceeded = 4,
        /// <value></value>
        CompareFalse = 5,
        /// <value></value>
        CompareTrue = 6,
        /// <value></value>
        AuthMethodNotSupported = 7,
        /// <value></value>
        StrongerAuthRequired = 8,
        /// <value></value>
        Referral = 10,
        /// <value></value>
        AdminLimitExceeded = 11,
        /// <value></value>
        UnavailableCriticalExtension = 12,
        /// <value></value>
        ConfidentialityRequired = 13,
        /// <value></value>
        SASLBindInProgress = 14,
        /// <value></value>
        NoSuchAttribute = 16,
        /// <value></value>
        UndefinedAttributeType = 17,
        /// <value></value>
        InappropriateMatching = 18,
        /// <value></value>
        ConstraintViolation = 19,
        /// <value></value>
        AttributeOrValueExists = 20,
        /// <value></value>
        InvalidAttributeSyntax = 21,
        /// <value></value>
        NoSuchObject = 32,
        /// <value></value>
        AliasProblem = 33,
        /// <value></value>
        InvalidDNSyntax = 34,
        /// <value></value>
        AliasDereferencingProblem = 36,
        /// <value></value>
        InappropriateAuthentication = 48,
        /// <value></value>
        InvalidCredentials = 49,
        /// <value></value>
        InsufficientAccessRights = 50,
        /// <value></value>
        Busy = 51,
        /// <value></value>
        Unavailable = 52,
        /// <value></value>
        UnwillingToPerform = 53,
        /// <value></value>
        LoopDetect = 54,
        /// <value></value>
        NamingViolation = 64,
        /// <value></value>
        ObjectClassViolation = 65,
        /// <value></value>
        NotAllowedOnNonLeaf = 66,
        /// <value></value>
        NotAllowedOnRDN = 67,
        /// <value></value>
        EntryAlreadyExists = 68,
        /// <value></value>
        ObjectClassModsProhibited = 69,
        /// <value></value>
        AffectsMultipleDSAs = 71,
        /// <value></value>
        Other = 80,
    }
}