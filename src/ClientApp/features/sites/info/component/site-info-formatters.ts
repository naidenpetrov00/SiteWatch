const dateFormatter = new Intl.DateTimeFormat(undefined, {
  year: "numeric",
  month: "short",
  day: "numeric",
});

const parseDateOnly = (value: string) => new Date(`${value}T00:00:00`);

export const formatSiteDate = (value: string | null) => {
  if (!value) return "Ongoing";

  const date = parseDateOnly(value);
  return Number.isNaN(date.getTime()) ? value : dateFormatter.format(date);
};

export const formatSiteDuration = (startDate: string, endDate: string | null) => {
  const start = parseDateOnly(startDate);
  const end = endDate ? parseDateOnly(endDate) : new Date();

  if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) return "—";

  const durationInDays = Math.max(
    0,
    Math.floor((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24)),
  );
  return `${durationInDays} ${durationInDays === 1 ? "day" : "days"}`;
};
