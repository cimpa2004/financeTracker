export type Interval = 'weekly' | 'monthly' | 'yearly' | 'All time';

function fmt(d: Date) {
  return d.toISOString().slice(0, 10);
}

// Returns { start, end } where values are YYYY-MM-DD strings or null for open-ended (All time)
export function getRangeForInterval(interval: Interval, refDate?: string | Date): { start: string | null; end: string | null } {
  const now = refDate ? new Date(refDate) : new Date();
  // normalize to UTC midnight
  const nowUtc = new Date(Date.UTC(now.getFullYear(), now.getMonth(), now.getDate()));

  if (interval === 'weekly') {
    const day = nowUtc.getUTCDay();
    const diff = (day + 6) % 7; // days since Monday
    const start = new Date(Date.UTC(nowUtc.getUTCFullYear(), nowUtc.getUTCMonth(), nowUtc.getUTCDate()));
    start.setUTCDate(start.getUTCDate() - diff);
    const end = new Date(start);
    end.setUTCDate(start.getUTCDate() + 6);
    return { start: fmt(start), end: fmt(end) };
  }

  if (interval === 'yearly') {
    const start = new Date(Date.UTC(nowUtc.getUTCFullYear(), 0, 1));
    const end = new Date(Date.UTC(nowUtc.getUTCFullYear(), 11, 31));
    return { start: fmt(start), end: fmt(end) };
  }

  if (interval === 'All time') {
    return { start: null, end: null };
  }

  // default monthly
  const start = new Date(Date.UTC(nowUtc.getUTCFullYear(), nowUtc.getUTCMonth(), 1));
  const end = new Date(Date.UTC(nowUtc.getUTCFullYear(), nowUtc.getUTCMonth() + 1, 0));
  return { start: fmt(start), end: fmt(end) };
}

export default getRangeForInterval;
