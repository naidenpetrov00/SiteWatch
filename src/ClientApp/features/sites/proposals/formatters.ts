const PLACEHOLDER = "—";

const dateFormatter = new Intl.DateTimeFormat(undefined, {
  dateStyle: "medium",
  timeZone: "UTC",
});

const dateTimeFormatter = new Intl.DateTimeFormat(undefined, {
  dateStyle: "medium",
  timeStyle: "short",
});

const parseTimestamp = (value: string | null | undefined) => {
  if (typeof value !== "string" || !value.trim()) return null;

  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? null : parsed;
};

const parseDateOnly = (value: string | null | undefined) => {
  if (typeof value !== "string" || !value.trim()) return null;

  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  if (!match) return null;

  const [, year, month, day] = match;
  const parsed = new Date(Date.UTC(Number(year), Number(month) - 1, Number(day)));
  return parsed.getUTCFullYear() === Number(year) &&
    parsed.getUTCMonth() === Number(month) - 1 &&
    parsed.getUTCDate() === Number(day)
    ? parsed
    : null;
};

export const formatProposalTimestamp = (value: string | null | undefined) => {
  const parsed = parseTimestamp(value);
  return parsed ? dateTimeFormatter.format(parsed) : PLACEHOLDER;
};

export const formatProposalDateOnly = (value: string | null | undefined) => {
  const parsed = parseDateOnly(value);
  return parsed ? dateFormatter.format(parsed) : PLACEHOLDER;
};
