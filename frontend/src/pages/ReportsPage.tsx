import { useState } from 'react';
import { Button, CircularProgress, Box, Stack, Typography, Paper, Container } from '@mui/material';
import { useDownloadBudgetReport } from '../apis/Reports';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs, { Dayjs } from 'dayjs';

const ReportsPage: React.FC = () => {
  const [from, setFrom] = useState<Dayjs | null>(() => dayjs().subtract(30, 'day'));
  const [to, setTo] = useState<Dayjs | null>(() => dayjs());

  const fromStr = from ? from.format('YYYY-MM-DD') : '';
  const toStr = to ? to.format('YYYY-MM-DD') : '';

  const { mutate: downloadReport, isPending } = useDownloadBudgetReport(fromStr, toStr);

  return (
    <Container maxWidth="md">
      <Paper elevation={2} sx={{ p: { xs: 2, sm: 4 }, mt: 4, borderRadius: 2 }}>
        <Stack spacing={2}>
          <Box>
            <Typography variant="h5" component="h2">
              Generate PDF report
            </Typography>
          </Box>

          <LocalizationProvider dateAdapter={AdapterDayjs}>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems="center">
              <DatePicker
                label="From"
                value={from}
                onChange={(v) => setFrom(v)}
                slotProps={{ textField: { size: 'small', sx: { minWidth: 160 } } }}
              />

              <DatePicker
                label="To"
                value={to}
                onChange={(v) => setTo(v)}
                slotProps={{ textField: { size: 'small', sx: { minWidth: 160 } } }}
              />

              <Box sx={{ flex: 1 }} />

              <Button
                variant="contained"
                color="primary"
                onClick={() => downloadReport()}
                disabled={isPending}
                startIcon={isPending ? <CircularProgress size={16} /> : undefined}
                sx={{ whiteSpace: 'nowrap' }}
              >
                {isPending ? 'Generating...' : 'Download PDF'}
              </Button>
            </Stack>
          </LocalizationProvider>
        </Stack>
      </Paper>
    </Container>
  );
};

export default ReportsPage;
