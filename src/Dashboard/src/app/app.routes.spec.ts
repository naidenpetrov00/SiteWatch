import { routes } from './app.routes';

describe('dashboard routes', () => {
  it('owns Manage Products as a lazy dashboard route', () => {
    const dashboard = routes.find((route) => route.path === '')!;
    const manageProducts = dashboard.children?.find((route) => route.path === 'manage-products');

    expect(manageProducts?.title).toBe('Manage Products');
    expect(manageProducts?.loadComponent).toBeTypeOf('function');
  });
});
