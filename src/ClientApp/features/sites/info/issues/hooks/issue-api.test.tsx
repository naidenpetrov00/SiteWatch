jest.mock('@/store/auth_context', () => ({ useAuth: jest.fn() }));
jest.mock('@/lib/api-client', () => ({ api: { get: jest.fn(), post: jest.fn() } }));

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { act, renderHook, waitFor } from '@testing-library/react-native';
import type { ReactNode } from 'react';

import { api } from '@/lib/api-client';
import { useAuth } from '@/store/auth_context';
import { useCreateIssue } from './useCreateIssue';
import { getIssueById } from './useGetIssueById';
import { getSiteIssues } from './useGetSiteIssues';

const accessToken = 'header.payload.signature';
const siteId = '11111111-1111-1111-1111-111111111111';
const issueId = '22222222-2222-2222-2222-222222222222';

describe('issue API hooks', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    jest.clearAllMocks();
    queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });
    jest.mocked(useAuth).mockReturnValue({ accessToken } as ReturnType<typeof useAuth>);
  });

  afterEach(() => queryClient.clear());

  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );

  it('requests site lists and details with the authenticated bearer token', async () => {
    jest.mocked(api.get).mockResolvedValueOnce([{ id: issueId }]).mockResolvedValueOnce({ id: issueId });

    await getSiteIssues({ siteId, accessToken });
    await getIssueById({ issueId, accessToken });

    expect(api.get).toHaveBeenNthCalledWith(1, `/issues/site/${siteId}`, { headers: { Authorization: `Bearer ${accessToken}` } });
    expect(api.get).toHaveBeenNthCalledWith(2, `/issues/${issueId}`, { headers: { Authorization: `Bearer ${accessToken}` } });
  });

  it('creates with the client contract and refreshes the owning site issue list', async () => {
    jest.mocked(api.post).mockResolvedValue({ id: issueId });
    const invalidate = jest.spyOn(queryClient, 'invalidateQueries');
    const { result } = renderHook(() => useCreateIssue(), { wrapper });
    const request = { siteId, title: 'Broken gate', description: 'The gate will not close.' };

    await act(async () => { await result.current.mutateAsync(request); });

    expect(api.post).toHaveBeenCalledWith('/issues', request, {
      headers: { Authorization: `Bearer ${accessToken}`, 'Content-Type': 'application/json' }
    });
    await waitFor(() => expect(invalidate).toHaveBeenCalledWith({ queryKey: ['site-issues', siteId] }));
  });
});
