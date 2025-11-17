import { Card, CardContent, Box, Typography, IconButton } from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import { PieChart, Pie, Cell, ResponsiveContainer } from 'recharts';
import { useTheme } from '@mui/material/styles';
import type { BudgetStatus } from '../types/Budget';

function SmallBar({ percent, color }: { percent: number; color: string }) {
  return (
    <Box sx={{ width: '100%', height: 10, backgroundColor: '#eee', borderRadius: 1 }}>
      <Box sx={{ width: `${percent}%`, height: '100%', background: color, borderRadius: 1 }} />
    </Box>
  );
}

interface BudgetCardProps {
  budget: BudgetStatus;
  onEdit: (budget: BudgetStatus) => void;
  onDelete: (id: string) => void;
  onDetails: (id: string) => void;
}

export default function BudgetCard({ budget, onEdit, onDelete, onDetails }: BudgetCardProps) {
  const theme = useTheme();
  const percent = budget.amount > 0 ? Math.min(100, Math.round((budget.spent / budget.amount) * 100)) : 0;
  const color = budget.category?.color ?? theme.palette.primary.main;

  return (
    <Card key={budget.budgetId} onClick={() => onDetails(budget.budgetId)} sx={{ cursor: 'pointer' }}>
      <CardContent sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
        <Box sx={{ width: 80, height: 80, position: 'relative' }}>
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie data={[{ name: 'spent', value: budget.spent }, { name: 'remaining', value: Math.max(0, budget.amount - budget.spent) }]} dataKey="value" innerRadius={22} outerRadius={36} startAngle={90} endAngle={-270}>
                <Cell key="spent" fill={color} />
                <Cell key="remaining" fill={theme.palette.grey[200]} />
              </Pie>
            </PieChart>
          </ResponsiveContainer>
          {percent >= 90 && (
            <Box sx={{ position: 'absolute', top: 4, right: 4, width: 18, height: 18, borderRadius: '50%', bgcolor: theme.palette.error.main, color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: 12, fontWeight: 'bold', pointerEvents: 'none' }}>
              !
            </Box>
          )}
        </Box>
        <Box sx={{ flex: 1 }}>
          <Box display="flex" alignItems="center" justifyContent="space-between" mb={1}>
            <Box>
              <Typography variant="h6">{budget.name ?? 'Untitled Budget'}</Typography>
              <Typography variant="body2" color="text.secondary">
                {budget.category ? budget.category.name : 'All categories'}
              </Typography>
            </Box>
            <Box textAlign="right">
              <Typography variant="subtitle1">{new Intl.NumberFormat('hu-HU', { style: 'currency', currency: 'HUF', maximumFractionDigits: 0 }).format(budget.spent)} / {new Intl.NumberFormat('hu-HU', { style: 'currency', currency: 'HUF', maximumFractionDigits: 0 }).format(budget.amount)}</Typography>
              <Typography variant="caption">Remaining: {new Intl.NumberFormat('hu-HU', { style: 'currency', currency: 'HUF', maximumFractionDigits: 0 }).format(budget.remaining)}</Typography>
            </Box>
          </Box>
          <SmallBar percent={percent} color={color} />
          <Box display="flex" justifyContent="space-between" mt={1}>
            <Typography variant="caption">{percent}% used{percent > 90 ? ' !' : ''}</Typography>
            <Typography variant="caption">{budget.startDate ? new Date(budget.startDate).toLocaleDateString() : ''}
              {budget.endDate ? `- ${new Date(budget.endDate).toLocaleDateString()}` : ''}</Typography>
            {budget.category && (
              <Typography variant="caption" align="center" sx={{ fontSize: 11 }}>{budget.category.name}</Typography>
            )}
          </Box>
          <Box display={'flex'} justifyContent={'flex-end'}>
            <IconButton color="primary" onClick={(e) => { e.stopPropagation(); onEdit(budget); }} aria-label={`edit-${budget.budgetId}`}>
              <EditIcon />
            </IconButton>
            <IconButton color="error" onClick={(e) => { e.stopPropagation(); onDelete(budget.budgetId); }} aria-label={`delete-${budget.budgetId}`}>
              <DeleteIcon />
            </IconButton>
          </Box>
        </Box>
      </CardContent>
    </Card>
  );
}
