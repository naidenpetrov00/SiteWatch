import { formatSiteDate, formatSiteDuration } from './site-info-formatters';

describe('site info formatters', () => {
  it('formats valid site dates and labels an absent end date as ongoing', () => {
    expect(formatSiteDate('2026-03-03')).toMatch(/Mar.*3.*2026|3.*Mar.*2026/);
    expect(formatSiteDate(null)).toBe('Ongoing');
  });

  it('calculates one-day and multi-day schedule durations at their boundaries', () => {
    expect(formatSiteDuration('2026-03-03', '2026-03-04')).toBe('1 day');
    expect(formatSiteDuration('2026-03-03', '2026-03-06')).toBe('3 days');
  });

  it('keeps invalid dates visible and returns a placeholder for an invalid duration', () => {
    expect(formatSiteDate('not-a-date')).toBe('not-a-date');
    expect(formatSiteDuration('not-a-date', '2026-03-06')).toBe('—');
  });
});
