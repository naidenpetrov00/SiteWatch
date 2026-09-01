import { fireEvent, render, screen, waitFor } from '@testing-library/react-native';

import AddIssueForm from './AddIssueForm';

const mutation = { isPending: false, mutateAsync: jest.fn() };

jest.mock('@/hooks/useColorPalette', () => ({ useColorPalette: () => ({ background: 'white', primary: 'blue', secondary: 'gray', text: 'black' }) }));
jest.mock('../hooks/useCreateIssue', () => ({ useCreateIssue: () => mutation }));

describe('AddIssueForm', () => {
  beforeEach(() => {
    mutation.mutateAsync.mockReset();
    mutation.mutateAsync.mockResolvedValue({ id: 'issue-1' });
  });

  it('submits trimmed valid values and closes after creation', async () => {
    const onClose = jest.fn();
    render(<AddIssueForm siteId="site-1" visible onClose={onClose} />);
    fireEvent.changeText(screen.getByLabelText('Issue title'), '  Broken gate  ');
    fireEvent.changeText(screen.getByLabelText('Issue description'), '  The gate will not close.  ');
    fireEvent.press(screen.getByRole('button', { name: 'Add Issue' }));

    await waitFor(() => expect(mutation.mutateAsync).toHaveBeenCalledWith({ siteId: 'site-1', title: 'Broken gate', description: 'The gate will not close.' }));
    await waitFor(() => expect(onClose).toHaveBeenCalledTimes(1));
  });

  it('shows local validation errors and surfaces failed submissions', async () => {
    mutation.mutateAsync.mockRejectedValue(new Error('Network unavailable'));
    render(<AddIssueForm siteId="site-1" visible onClose={jest.fn()} />);

    fireEvent.press(screen.getByRole('button', { name: 'Add Issue' }));
    expect(screen.getByRole('alert').props.children).toBe('Enter an issue title of up to 200 characters.');

    fireEvent.changeText(screen.getByLabelText('Issue title'), 'Broken gate');
    fireEvent.changeText(screen.getByLabelText('Issue description'), 'Details');
    fireEvent.press(screen.getByRole('button', { name: 'Add Issue' }));

    await waitFor(() => expect(screen.getByRole('alert').props.children).toBe('Network unavailable'));
  });
});
