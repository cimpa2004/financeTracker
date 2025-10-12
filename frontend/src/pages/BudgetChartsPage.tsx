import { Box, Typography, CircularProgress, Card, CardContent } from '@mui/material';
import { useTheme } from '@mui/material/styles';
import { useAllBudgetsStatus } from '../apis/Budget';
import { PieChart, Pie, Cell, ResponsiveContainer } from 'recharts';

function SmallBar({ percent, color }: { percent: number; color: string }) {
  return (
    <Box sx={{ width: '100%', height: 10, backgroundColor: '#eee', borderRadius: 1 }}>
      <Box sx={{ width: `${percent}%`, height: '100%', background: color, borderRadius: 1 }} />
    </Box>
  );
}

export default function BudgetChartsPage() {
  const theme = useTheme();
  const { data, isLoading, isError } = useAllBudgetsStatus();

  if (isLoading) return <CircularProgress />;
  if (isError) return <Box>Error loading budgets</Box>;

  const budgets = data ?? [];

  if (budgets.length === 0) {
    return (
      <Box p={2} textAlign="center">
        <Typography variant="h5">No budgets found</Typography>
      </Box>
    );
  }

  return (
    <Box p={2}>
      <Typography variant="h4" gutterBottom>
        Budgets Overview
      </Typography>
      <Box
        display="grid"
        gridTemplateColumns={{ xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr 1fr' }}
        gap={2}
      >
        {budgets.map((b) => {
          const percent = b.amount > 0 ? Math.min(100, Math.round((b.spent / b.amount) * 100)) : 0;
          const color = b.category?.color ?? theme.palette.primary.main;
          return (
            <Card key={b.budgetId}>
              <CardContent sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
                  <Box sx={{ width: 80, height: 80, position: 'relative' }}>
                    <ResponsiveContainer width="100%" height="100%">
                      <PieChart>
                        <Pie data={[{ name: 'spent', value: b.spent }, { name: 'remaining', value: Math.max(0, b.amount - b.spent) }]} dataKey="value" innerRadius={22} outerRadius={36} startAngle={90} endAngle={-270}>
                          <Cell key="spent" fill={color} />
                          <Cell key="remaining" fill={theme.palette.grey[200]} />
                        </Pie>
                      </PieChart>
                    </ResponsiveContainer>
                  </Box>
                <Box sx={{ flex: 1 }}>
                  <Box display="flex" alignItems="center" justifyContent="space-between" mb={1}>
                    <Box>
                      <Typography variant="h6">{b.name ?? 'Untitled Budget'}</Typography>
                      <Typography variant="body2" color="text.secondary">
                        {b.category ? b.category.name : 'All categories'}
                      </Typography>
                    </Box>
                    <Box textAlign="right">
                      <Typography variant="subtitle1">{new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(b.spent)} / {new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(b.amount)}</Typography>
                      <Typography variant="caption">Remaining: {new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(b.remaining)}</Typography>
                    </Box>
                  </Box>
                  <SmallBar percent={percent} color={color} />
                  <Box display="flex" justifyContent="space-between" mt={1}>
                    <Typography variant="caption">{percent}% used</Typography>
                    <Typography variant="caption">{b.startDate ? new Date(b.startDate).toLocaleDateString() : ''}
                      {b.endDate ? `- ${new Date(b.endDate).toLocaleDateString()}` : ''}</Typography>
                    {b.category && (
                      <Typography variant="caption" align="center" sx={{ fontSize: 11 }}>{b.category.name}</Typography>
                    )}
                  </Box>

                </Box>
              </CardContent>
            </Card>
          );
        })}
      </Box>
    </Box>
  );
}
