import {
  ACCESS_POLICIES,
  AUTHENTICATED_ROUTES,
  USER_ROLES,
  getPostSignInRoute,
  hasAnyRole,
  parseUserRoles,
} from "@/types/authorization";

describe("role authorization", () => {
  it("parses only supported unique roles", () => {
    expect(
      parseUserRoles([
        USER_ROLES.administrator,
        "Unknown",
        USER_ROLES.worker,
        USER_ROLES.worker,
        null,
      ]),
    ).toEqual([USER_ROLES.administrator, USER_ROLES.worker]);
    expect(parseUserRoles(undefined)).toEqual([]);
  });

  it("supports policies shared by multiple roles", () => {
    expect(
      hasAnyRole(
        [USER_ROLES.worker],
        [USER_ROLES.administrator, USER_ROLES.worker],
      ),
    ).toBe(true);
    expect(
      hasAnyRole(
        [USER_ROLES.client],
        [USER_ROLES.administrator, USER_ROLES.worker],
      ),
    ).toBe(false);
  });

  it.each([
    [USER_ROLES.administrator, AUTHENTICATED_ROUTES.currentApp],
    [USER_ROLES.client, AUTHENTICATED_ROUTES.currentApp],
    [USER_ROLES.worker, AUTHENTICATED_ROUTES.currentApp],
  ])("routes %s to the expected authenticated page", (role, expectedRoute) => {
    expect(getPostSignInRoute([role])).toBe(expectedRoute);
  });

  it("allows every supported role into the current app but limits site invoices", () => {
    expect(ACCESS_POLICIES.currentApp).toEqual([
      USER_ROLES.administrator,
      USER_ROLES.worker,
      USER_ROLES.client,
    ]);
    expect(ACCESS_POLICIES.siteInvoices).toEqual([
      USER_ROLES.administrator,
      USER_ROLES.worker,
    ]);
    expect(getPostSignInRoute(undefined)).toBe(AUTHENTICATED_ROUTES.accessDenied);
  });
});
