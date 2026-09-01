import { render, screen } from '@testing-library/react-native';
import type { ReactNode } from 'react';

import Details from './Details';

jest.mock('@/hooks/useGetSearchParams', () => () => ({ siteId: 'site-1' }));
jest.mock('@/hooks/useColorPalette', () => ({ useColorPalette: () => ({ text: 'black', secondary: 'gray' }) }));
jest.mock('@/store/auth_context', () => ({ useAuth: () => ({ hasAnyRole: () => true }) }));
jest.mock('@/features/sites/info/images/hooks/useGetSiteImageIdsBySiteId', () => ({ useGetSiteImageIdsBySiteId: () => ({ isSuccess: true, data: [] }) }));
jest.mock('@/features/sites/info/videos/hooks/useGetSiteVideoIdsBySiteId', () => ({ useGetSiteVideoIdsBySiteId: () => ({ isSuccess: true, data: [] }) }));
jest.mock('@/features/sites/info/files/hooks/useGetSiteFileIdsBySiteId', () => ({ useGetSiteFileIdsBySiteId: () => ({ isSuccess: true, data: [] }) }));
jest.mock('@/features/sites/info/invoices/hooks/useGetSiteInvoices', () => ({ useGetSiteInvoices: () => ({ isSuccess: true, data: [] }) }));
jest.mock('@/features/sites/info/issues/hooks/useGetSiteIssues', () => ({ useGetSiteIssues: () => ({ isSuccess: true, data: [{ id: 'issue-1' }, { id: 'issue-2' }] }) }));
jest.mock('../../ui/DetailCard/DetailCard', () => {
  const React = require('react');
  const { View } = require('react-native');

  return ({ path, children }: { path: string; children: ReactNode }) =>
    React.createElement(View, { accessibilityLabel: `detail-${path}` }, children);
});

describe('site details', () => {
  it('links to Issues and shows the retrieved issue count', () => {
    render(<Details />);

    expect(screen.getByLabelText('detail-Issues')).toBeTruthy();
    expect(screen.getByText('Issues')).toBeTruthy();
    expect(screen.getByText('2')).toBeTruthy();
  });
});
