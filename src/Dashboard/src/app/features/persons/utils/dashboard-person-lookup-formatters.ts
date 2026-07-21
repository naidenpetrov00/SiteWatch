import { DashboardPersonLookup } from '../models/dashboard-person-lookup.model';

function formatOptionalValue(label: string, value: string | null): string | null {
  const normalizedValue = value?.trim() ?? '';

  return normalizedValue.length > 0 ? `${label}: ${normalizedValue}` : null;
}

export function formatDashboardPersonLookupLabel(person: DashboardPersonLookup): string {
  return person.displayName.trim();
}

export function formatDashboardPersonLookupSubtitle(person: DashboardPersonLookup): string {
  const parts = [
    formatOptionalValue('First', person.firstName),
    formatOptionalValue('Middle', person.middleName),
    formatOptionalValue('Last', person.lastName),
    formatOptionalValue('Company', person.companyName),
    `Id: ${person.id}`
  ].filter((part): part is string => part !== null);

  return parts.join(' • ');
}
