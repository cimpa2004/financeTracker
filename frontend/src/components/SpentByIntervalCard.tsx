import { useMemo, useState, useEffect } from 'react';
import { Box, Paper, Typography, Select, MenuItem, TextField } from '@mui/material';
import { useTheme } from '@mui/material/styles';
import { ResponsiveContainer, LineChart, Line, XAxis, YAxis, Tooltip, CartesianGrid } from 'recharts';
import type { Interval } from '../types/Statistics';
import { useGetSpentByInterval } from '../apis/Statistics';

type TooltipPayload = { value?: number }[] | undefined;

function ChartTooltip({ active, payload, label }: { active?: boolean; payload?: TooltipPayload; label?: string }) {
  if (!active || !payload?.length) return null;
  const item = payload[0];
  return (
    <Box sx={{ bgcolor: (t) => t.palette.background.paper, color: (t) => t.palette.text.primary, p: 1, boxShadow: 3, borderRadius: 1 }}>
      <Typography variant="subtitle2">{label}</Typography>
      <Typography variant="body2">{new Intl.NumberFormat('hu-HU', { style: 'currency', currency: 'HUF', maximumFractionDigits: 0 }).format(item?.value ?? 0)}</Typography>
    </Box>
  );
}

export default function SpentByIntervalCard() {
  const theme = useTheme();
  const [interval, setInterval] = useState<Interval>('Monthly');
  const [startDate, setStartDate] = useState<string | undefined>(undefined);
  const [endDate, setEndDate] = useState<string | undefined>(undefined);
  const [startRaw, setStartRaw] = useState<string>('');
  const [endRaw, setEndRaw] = useState<string>('');

  // convert date (yyyy-mm-dd) to ISO UTC strings expected by backend
  const startIso = useMemo(() => (startDate ? `${startDate}T00:00:00Z` : undefined), [startDate]);
  const endIso = useMemo(() => (endDate ? `${endDate}T23:59:59Z` : undefined), [endDate]);

  // parse display value like 'yyyy.mm.dd' or 'yyyy-mm-dd' into normalized 'yyyy-mm-dd'
  const parseDisplayToIso = (val: string) => {
    if (!val) return undefined;
    const trimmed = val.trim();
    if (/^\d{4}\.\d{2}\.\d{2}$/.test(trimmed)) return trimmed.replaceAll('.', '-');
    if (/^\d{4}-\d{2}-\d{2}$/.test(trimmed)) return trimmed;
    return undefined;
  };

  const formatIsoToDisplay = (iso?: string) => {
    if (!iso) return '';
    // expect yyyy-mm-dd
  const re = /^(\d{4})-(\d{2})-(\d{2})$/;
  const m = re.exec(iso);
  if (!m) return iso;
  return `${m[1]}.${m[2]}.${m[3]}`;
  };

  useEffect(() => {
    setStartRaw(formatIsoToDisplay(startDate));
  }, [startDate]);

  useEffect(() => {
    setEndRaw(formatIsoToDisplay(endDate));
  }, [endDate]);

  const { data, isLoading, isError } = useGetSpentByInterval(interval, startIso, endIso);

  const chartData = useMemo(() => {
    const arr: { PeriodStart: string; Spent: number }[] = data?.byPeriod.map(({ periodStart, spent }) => ({ PeriodStart: periodStart, Spent: spent })) ?? [];
    return arr.map((p) => ({ x: p.PeriodStart, y: Number(p.Spent ?? 0) }));
  }, [data]);

  if (isLoading) return (
    <Paper elevation={2} sx={{ p: 2, display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
      <Typography>Loading...</Typography>
    </Paper>
  );

  if (isError) return (
    <Paper elevation={2} sx={{ p: 2 }}>
      <Box>Failed to load data</Box>
    </Paper>
  );

  const formatX = (iso: string) => {
    try {
      const d = new Date(iso);
      if (interval === 'Daily') return d.toLocaleDateString();
      if (interval === 'Weekly') return `W ${d.toLocaleDateString()}`;
      if (interval === 'Monthly') return d.toLocaleDateString(undefined, { month: 'short', year: 'numeric' });
      if (interval === 'Yearly') return d.getFullYear().toString();
      return d.toLocaleDateString();
    } catch {
      return iso;
    }
  };

  return (
    <Paper elevation={2} sx={{ p: 2 }}>
      <Box display="flex" alignItems="center" justifyContent="space-between" mb={2}>
        <Typography variant="h6">Spending over time</Typography>
        <Box display="flex" gap={1} alignItems="center">
          <Select size="small" value={interval} onChange={(e) => setInterval(e.target.value as Interval)}>
            <MenuItem value="Daily">Daily</MenuItem>
            <MenuItem value="Weekly">Weekly</MenuItem>
            <MenuItem value="Monthly">Monthly</MenuItem>
            <MenuItem value="Yearly">Yearly</MenuItem>
          </Select>
        </Box>
      </Box>

        <Box display="flex" gap={2} mb={2} alignItems="center">
        <TextField
          label="Start date"
          type="text"
          size="small"
          placeholder="yyyy.mm.dd"
          value={startRaw}
          onChange={(e) => {
            const v = e.target.value;
            setStartRaw(v);
            setStartDate(parseDisplayToIso(v));
          }}
          aria-label="start date"
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <TextField
          label="End date"
          type="text"
          size="small"
          placeholder="yyyy.mm.dd"
          value={endRaw}
          onChange={(e) => {
            const v = e.target.value;
            setEndRaw(v);
            setEndDate(parseDisplayToIso(v));
          }}
          aria-label="end date"
          slotProps={{ inputLabel: { shrink: true } }}
        />
      </Box>

      {(!chartData || chartData.length === 0) ? (
        <Box textAlign="center" py={6}>
          <Typography variant="body2">No data for the selected interval / range.</Typography>
        </Box>
      ) : (
        <Box sx={{ width: '100%', height: 220 }}>
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={chartData} margin={{ top: 10, right: 20, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="x" tickFormatter={formatX} stroke={theme.palette.text.secondary} />
              <YAxis tickFormatter={(v) => new Intl.NumberFormat('hu-HU', { style: 'currency', currency: 'HUF', maximumFractionDigits: 0 }).format(Number(v))} stroke={theme.palette.text.secondary} />
              <Tooltip content={<ChartTooltip />} />
              <Line type="monotone" dataKey="y" stroke={theme.palette.primary.main} strokeWidth={2} dot={{ r: 2 }} />
            </LineChart>
          </ResponsiveContainer>
        </Box>
      )}
    </Paper>
  );
}
