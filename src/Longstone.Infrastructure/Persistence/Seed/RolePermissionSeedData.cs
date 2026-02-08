using Longstone.Domain.Auth;

namespace Longstone.Infrastructure.Persistence.Seed;

public static class RolePermissionSeedData
{
    public static IReadOnlyList<RolePermission> CreateSeededRolePermissions()
    {
        var permissions = new List<RolePermission>();

        // SystemAdmin — all permissions, all scope
        foreach (var permission in Enum.GetValues<Permission>())
        {
            permissions.Add(RolePermission.Create(Role.SystemAdmin, permission, PermissionScope.All));
        }

        // Fund Manager
        permissions.Add(RolePermission.Create(Role.FundManager, Permission.ViewPortfolios, PermissionScope.Own));
        permissions.Add(RolePermission.Create(Role.FundManager, Permission.ManageFunds, PermissionScope.Own));
        permissions.Add(RolePermission.Create(Role.FundManager, Permission.CreateOrders, PermissionScope.Own));
        permissions.Add(RolePermission.Create(Role.FundManager, Permission.ViewRiskDashboards, PermissionScope.Own));

        // Dealer
        permissions.Add(RolePermission.Create(Role.Dealer, Permission.ViewPortfolios, PermissionScope.All));
        permissions.Add(RolePermission.Create(Role.Dealer, Permission.ExecuteOrders, PermissionScope.All));
        permissions.Add(RolePermission.Create(Role.Dealer, Permission.ViewRiskDashboards, PermissionScope.All));

        // Compliance Officer
        permissions.Add(RolePermission.Create(Role.ComplianceOfficer, Permission.ViewPortfolios, PermissionScope.All));
        permissions.Add(RolePermission.Create(Role.ComplianceOfficer, Permission.ConfigureCompliance, PermissionScope.All));
        permissions.Add(RolePermission.Create(Role.ComplianceOfficer, Permission.OverrideComplianceBreach, PermissionScope.All));
        permissions.Add(RolePermission.Create(Role.ComplianceOfficer, Permission.ViewRiskDashboards, PermissionScope.All));
        permissions.Add(RolePermission.Create(Role.ComplianceOfficer, Permission.ViewAuditLogs, PermissionScope.All));

        // Operations
        permissions.Add(RolePermission.Create(Role.Operations, Permission.ViewPortfolios, PermissionScope.All));
        permissions.Add(RolePermission.Create(Role.Operations, Permission.ProcessCorporateActions, PermissionScope.All));
        permissions.Add(RolePermission.Create(Role.Operations, Permission.RunNavCalculation, PermissionScope.All));
        permissions.Add(RolePermission.Create(Role.Operations, Permission.ViewAuditLogs, PermissionScope.All));

        // Risk Manager
        permissions.Add(RolePermission.Create(Role.RiskManager, Permission.ViewPortfolios, PermissionScope.All));
        permissions.Add(RolePermission.Create(Role.RiskManager, Permission.ViewRiskDashboards, PermissionScope.All));
        permissions.Add(RolePermission.Create(Role.RiskManager, Permission.ViewAuditLogs, PermissionScope.All));

        // ReadOnly — view portfolios only
        permissions.Add(RolePermission.Create(Role.ReadOnly, Permission.ViewPortfolios, PermissionScope.All));

        return permissions;
    }
}
