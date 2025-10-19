import { Box, CircularProgress, Paper, Typography, Select, MenuItem, Stack } from "@mui/material";
import ThemedScrollbar from './ThemedScrollbar';
import { useTheme } from "@mui/material/styles";
import type { Interval } from "../types/Statistics";
import { useState } from "react";
import { useGetSpentByCategory } from "../apis/Statistics";
import { ResponsiveContainer, PieChart, Pie, Cell, Tooltip } from "recharts";
import {Icon} from '../Icons/Icons.tsx';

type TooltipPayloadItem = { value?: number; name?: string; payload?: { color?: string } };

function CustomTooltip({ active, payload, label, total, currency }: { active?: boolean; payload?: TooltipPayloadItem[]; label?: string; total: number; currency: Intl.NumberFormat }) {
  if (!active || !payload?.length) return null;
  const item = payload[0];
  const value = item?.value ?? 0;
  const name = item?.name ?? label ?? '';
  const percent = Math.round((value / Math.max(1, total)) * 100);

  return (
    <Box sx={{ bgcolor: (theme) => theme.palette.background.paper, color: (theme) => theme.palette.text.primary, boxShadow: 3, borderLeft: `4px solid ${item?.payload?.color ?? '#1976d2'}`, p: 1, borderRadius: 1 }}>
      <Typography variant="subtitle2">{name}</Typography>
      <Typography variant="body2">{currency.format(value)}</Typography>
      <Typography variant="caption" color="text.secondary">{percent}% of total</Typography>
    </Box>
  );
}

export default function SpentByCategoryCard() {
  const [interval, setInterval] = useState<Interval>("AllTime");
  const { data, isLoading, isError } = useGetSpentByCategory(interval);
  const theme = useTheme();

  const currency = new Intl.NumberFormat('hu-HU', { style: 'currency', currency: 'HUF', maximumFractionDigits: 0 });

  if (isLoading) return (
    <Paper elevation={2} sx={{ p: 2, display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
      <CircularProgress />
    </Paper>
  );

  if (isError) return (
    <Paper elevation={2} sx={{ p: 2 }}>
      <Box>Error loading spending by category</Box>
    </Paper>
  );

  const byCategory = data?.byCategory ?? [];

  const colorsFallback = [theme.palette.primary.main, theme.palette.secondary.main, theme.palette.error.main, theme.palette.warning.main, theme.palette.info.main, theme.palette.success.main];

  const chartData = byCategory.map((b, i) => {
    const name = b.category?.name ?? 'Uncategorized';
    const value = b.spent;
    const color = b.category?.color ?? colorsFallback[i % colorsFallback.length];
    return { name, value, color, raw: b };
  }).filter(d => d.value > 0);

  const total = data?.totalSpent ?? chartData.reduce((s, c) => s + c.value, 0);



  return (
    <Paper elevation={2} sx={{ p: 2 }}>
      <Box display="flex" alignItems="center" justifyContent="space-between" mb={2}>
        <Typography variant="h6">Spendings by category</Typography>
        <Select size="small" value={interval} onChange={(e) => setInterval(e.target.value as Interval)}>
          <MenuItem value="Daily">Daily</MenuItem>
          <MenuItem value="Weekly">Weekly</MenuItem>
          <MenuItem value="Monthly">Monthly</MenuItem>
          <MenuItem value="Yearly">Yearly</MenuItem>
          <MenuItem value="AllTime">All time</MenuItem>
        </Select>
      </Box>

      {chartData.length === 0 ? (
        <Box textAlign="center" py={6}>
          <Typography variant="body2">No spending data for the selected interval.</Typography>
        </Box>
      ) : (
        <Box display={{ xs: 'block', md: 'flex' }} alignItems="center" gap={2}>
          <Box sx={{ width: { xs: '100%', md: '60%' }, height: 220 }}>
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie data={chartData} dataKey="value" nameKey="name" innerRadius={60} outerRadius={90} paddingAngle={0} startAngle={90} endAngle={-270}>
                  {chartData.map((entry) => (
                    <Cell key={entry.name} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip content={<CustomTooltip total={total} currency={currency} />} />
              </PieChart>
            </ResponsiveContainer>
          </Box>

          <Box sx={{ flex: 1 }}>
            <ThemedScrollbar>
              <Stack spacing={1}>
                {chartData.map((c) => (
                  <Box key={c.name} display="flex" alignItems="center" justifyContent="space-between" sx={{ px: 1 }}>
                    <Box display="flex" alignItems="center" gap={1} sx={{ minWidth: 0 }}>
                      <Icon name={c.raw.category?.icon ?? undefined} colorOf={c.color} style={{ width: 14, height: 14 }} />
                      <Typography variant="body2" sx={{ whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{c.name}</Typography>
                    </Box>
                    <Box textAlign="right">
                      <Typography variant="body2">{currency.format(c.value)}</Typography>
                      <Typography variant="caption" color="text.secondary">{Math.round((c.value / Math.max(1, total)) * 100)}%</Typography>
                    </Box>
                  </Box>
                ))}
              </Stack>
            </ThemedScrollbar>
          </Box>
        </Box>
      )}
    </Paper>
  );
}