# LDAP
dotnet Core utility to talk to OpenLDAP

# Local Setup

Ensure you have docker setup and then run the following command prior to running tests:

```docker run --rm -it --env LDAP_TLS_VERIFY_CLIENT=allow -p 10389:389 -p 10636:636 osixia/openldap```