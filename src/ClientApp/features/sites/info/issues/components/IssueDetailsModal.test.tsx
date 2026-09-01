import { render, screen } from '@testing-library/react-native';
import type { ReactNode } from 'react';

import IssueDetailsModal from './IssueDetailsModal';

jest.mock('@expo/vector-icons/Ionicons', () => 'Ionicons');
jest.mock('@/hooks/useColorPalette', () => ({ useColorPalette: () => ({ background: 'white', primary: 'blue', secondary: 'gray', text: 'black' }) }));
jest.mock('react-native-safe-area-context', () => ({ SafeAreaView: ({ children }: { children: ReactNode }) => children }));
jest.mock('../hooks/useGetIssueById', () => ({ useGetIssueById: () => ({
  data: {
    id: 'issue-1', numberId: 42, title: 'Broken gate', description: 'The gate will not close.', status: 'Open',
    startDate: null, endDate: null, assignedWorkers: [{ id: 'worker-1', userName: null, email: 'worker@example.test' }],
    created: '2026-04-02T10:30:00Z', createdBy: 'admin', lastModified: '2026-04-03T10:30:00Z', lastModifiedBy: null
  }, isLoading: false, isError: false, error: null
}) }));

describe('IssueDetailsModal', () => {
  it('renders the issue status, worker fallback name, dates, and activity details', () => {
    render(<IssueDetailsModal issueId="issue-1" onClose={jest.fn()} />);

    expect(screen.getByText('Open')).toBeTruthy();
    expect(screen.getByText('worker@example.test')).toBeTruthy();
    expect(screen.getAllByText('—').length).toBeGreaterThan(0);
    expect(screen.getByText('admin')).toBeTruthy();
  });
});
