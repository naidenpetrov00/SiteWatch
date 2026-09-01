import { fireEvent, render, screen } from '@testing-library/react-native';
import type { ReactNode } from 'react';

import IssuesCard from './IssuesCard';

const mockUseGetSiteIssues = jest.fn();

jest.mock('@expo/vector-icons/Ionicons', () => 'Ionicons');
jest.mock('@/hooks/useColorPalette', () => ({ useColorPalette: () => ({ background: 'white', primary: 'blue', secondary: 'gray', text: 'black' }) }));
jest.mock('@/hooks/useGetSearchParams', () => () => ({ siteId: 'site-1' }));
jest.mock('@/features/auth/components/RoleGate/RoleGate', () => ({ children }: { children: ReactNode }) => children);
jest.mock('@/store/auth_context', () => ({ useAuth: () => ({ hasAnyRole: () => true }) }));
jest.mock('react-native-safe-area-context', () => ({ useSafeAreaInsets: () => ({ bottom: 0 }) }));
jest.mock('../hooks/useGetSiteIssues', () => ({ useGetSiteIssues: (...args: unknown[]) => mockUseGetSiteIssues(...args) }));
jest.mock('./AddIssueModal', () => () => null);
jest.mock('./IssueDetailsModal', () => {
  const React = require('react');
  const { Text } = require('react-native');

  return ({ issueId }: { issueId: string | null }) =>
    issueId ? React.createElement(Text, null, `selected:${issueId}`) : null;
});

describe('IssuesCard', () => {
  it('renders a loaded issue and opens its details when selected', () => {
    mockUseGetSiteIssues.mockReturnValue({
      data: [{ id: 'issue-1', numberId: 42, title: 'Broken gate', status: 'Open' }],
      isLoading: false, isError: false, isRefetching: false, refetch: jest.fn()
    });

    render(<IssuesCard />);
    fireEvent.press(screen.getByRole('button', { name: 'View issue Broken gate' }));

    expect(screen.getByText('Issue #42')).toBeTruthy();
    expect(screen.getByText('selected:issue-1')).toBeTruthy();
  });

  it('shows an observable error state when the issue list fails', () => {
    mockUseGetSiteIssues.mockReturnValue({
      data: [], error: new Error('Unavailable'), isLoading: false, isError: true, isRefetching: false, refetch: jest.fn()
    });

    render(<IssuesCard />);

    expect(screen.getByText('Issues unavailable')).toBeTruthy();
    expect(screen.getByText('Unavailable')).toBeTruthy();
  });
});
