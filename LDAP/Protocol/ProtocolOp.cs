namespace Telefrek.Security.LDAP.Protocol
{
    public enum ProtocolOp
    {
        BIND_REQUEST = 0,
        BIND_RESPONSE = 1,
        UNBIND_REQUEST = 2,
        SEARCH_REQUEST = 3,
        SEARCH_RESPONSE = 4,
        SEARCH_RESULT = 5,
        MODIFY_REQUEST = 6,
        MODIFY_RESPONSE = 7,
        ADD_REQUEST = 8,
        ADD_RESPONSE = 9,
        DEL_REQUEST = 10,
        DEL_RESPONSE = 11,
        MODIFY_RDN_REQUEST = 12,
        MODIFY_RDN_RESPONSE = 13,
        COMPARE_REQUEST = 14,
        COMPARE_RESPONSE = 15,
        ABANDON_REQUEST = 16,
        SEARCH_RESULT_REFERENCE = 19,
        EXTENDED_REQUEST = 23,
        EXTENDED_RESPONSE = 24,
        INTERMEDIATE_RESPONSE = 25,
    }
}