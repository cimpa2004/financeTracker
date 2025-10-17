export type IntervalOption = 'weekly' | 'monthly' | 'yearly' | 'All time';

function toDate(d?: string | null) {
  if (!d) return null;
  const dt = new Date(d);
  if (isNaN(dt.getTime())) return null;
  // normalize to UTC midnight for stable date-only comparisons
  return new Date(Date.UTC(dt.getFullYear(), dt.getMonth(), dt.getDate()));
}

// Return true if date is first day of month
function isFirstOfMonth(d: Date) {
  return d.getUTCDate() === 1;
}

// Return true if date is last day of month
function isLastOfMonth(d: Date) {
  const year = d.getUTCFullYear();
  const month = d.getUTCMonth();
  const nextMonthFirst = new Date(Date.UTC(year, month + 1, 1));
  const lastDay = new Date(nextMonthFirst);
  lastDay.setUTCDate(lastDay.getUTCDate() - 1);
  return d.getUTCDate() === lastDay.getUTCDate();
}

// Return true if date is Jan 1
function isFirstOfYear(d: Date) {
  return d.getUTCMonth() === 0 && d.getUTCDate() === 1;
}

// Return true if date is Dec 31
function isLastOfYear(d: Date) {
  return d.getUTCMonth() === 11 && d.getUTCDate() === 31;
}

// Return true if start is Monday of the week and end is Sunday
function isFullWeekRange(start: Date, end: Date) {
  const startDay = start.getUTCDay();
  // in JS, Sunday=0, Monday=1; we accept Monday start
  const isMonday = startDay === 1;
  if (!isMonday) return false;
  // compute expected end by adding 6 days to start (UTC)
  const expected = new Date(Date.UTC(start.getUTCFullYear(), start.getUTCMonth(), start.getUTCDate()));
  expected.setUTCDate(expected.getUTCDate() + 6);
  return expected.getUTCFullYear() === end.getUTCFullYear() && expected.getUTCMonth() === end.getUTCMonth() && expected.getUTCDate() === end.getUTCDate();
}

export function getIntervalFromDates(start?: string | null, end?: string | null): IntervalOption {
  const s = toDate(start);
  const e = toDate(end);
  if (!s && !e) return 'All time';
  if (s && e) {
    // If the range starts on Jan 1 and ends on Dec 31 consider it yearly (even across multiple years)
    if (isFirstOfYear(s) && isLastOfYear(e)) return 'yearly';
    // monthly: same month & year and full month
    if (s.getUTCFullYear() === e.getUTCFullYear() && s.getUTCMonth() === e.getUTCMonth() && isFirstOfMonth(s) && isLastOfMonth(e)) return 'monthly';
    if (isFullWeekRange(s, e)) return 'weekly';
  }
  return 'monthly';
}

export default getIntervalFromDates;
