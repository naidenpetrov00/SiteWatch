import { routes } from './app.routes';

describe('dashboard routes', () => {
  it('owns catalog pages as lazy dashboard routes', () => {
    const dashboard = routes.find((route) => route.path === '')!;
    const manageProducts = dashboard.children?.find((route) => route.path === 'manage-products');
    const manageRetailers = dashboard.children?.find((route) => route.path === 'manage-retailers');
    const manageActivities = dashboard.children?.find((route) => route.path === 'manage-activities');

    expect(manageProducts?.title).toBe('Manage Products');
    expect(manageProducts?.loadComponent).toBeTypeOf('function');
    expect(manageRetailers?.title).toBe('Manage Retailers');
    expect(manageRetailers?.loadComponent).toBeTypeOf('function');
    expect(manageActivities?.title).toBe('Manage Activities');
    expect(manageActivities?.loadComponent).toBeTypeOf('function');
  });
});
