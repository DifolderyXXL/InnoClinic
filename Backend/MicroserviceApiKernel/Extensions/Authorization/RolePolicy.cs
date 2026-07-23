namespace MicroserviceApiKernel;

public static class RolePolicy
{
    public const string Client = "ClientPolicyName";
    public const string Doctor = "DoctorPolicyName";
    public const string Receptionist = "ReceptionistPolicyName";
    public const string DoctorOrReceptionist = "DoctorOrReceptionist";
    public const string IdentityServer = "IdentityServerPolicyName";
}

public static class StaticRoleConvention
{
    public const string Client = "client";
    public const string Doctor = "doctor";
    public const string Receptionist = "receptionist";
}