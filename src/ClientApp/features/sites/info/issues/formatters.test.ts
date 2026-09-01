import { formatIssueDate, formatIssueDateTime, formatIssueText } from './formatters';

describe('issue formatters', () => {
  it('renders missing values as a consistent placeholder', () => {
    expect(formatIssueDate(null)).toBe('—');
    expect(formatIssueDateTime(null)).toBe('—');
    expect(formatIssueText('   ')).toBe('—');
  });

  it('keeps invalid dates visible and formats meaningful text', () => {
    expect(formatIssueDate('not-a-date')).toBe('not-a-date');
    expect(formatIssueDateTime('not-a-date')).toBe('not-a-date');
    expect(formatIssueText(' Gate repair ')).toBe(' Gate repair ');
  });
});
