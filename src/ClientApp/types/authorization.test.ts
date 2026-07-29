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
    [USER_ROLES.client, AUTHENTICATED_ROUTES.accessDenied],
    [USER_ROLES.worker, AUTHENTICATED_ROUTES.accessDenied],
  ])("routes %s to the expected authenticated page", (role, expectedRoute) => {
    expect(getPostSignInRoute([role])).toBe(expectedRoute);
  });

  it("keeps the current app policy Administrator-only", () => {
    expect(ACCESS_POLICIES.currentApp).toEqual([USER_ROLES.administrator]);
    expect(getPostSignInRoute(undefined)).toBe(AUTHENTICATED_ROUTES.accessDenied);
  });
});
