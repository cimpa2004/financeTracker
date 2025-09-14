import { Box, Button, Typography, CircularProgress } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { z } from 'zod';
import { httpService } from '../services/httpService';

function useHealthCheck() {
    return useQuery<string, unknown>({
        queryKey: ['health'],
        queryFn: async () => httpService.get('/', z.string()),
        retry: false,
    });
}

export default function HealthCheck() {
  const { data, isLoading, isError, refetch } = useHealthCheck();

  return (
    <Box display="flex" alignItems="center" gap={2}>
      {isLoading ? (
        <CircularProgress size={20} />
      ) : null}
      {(() => {
        if (isLoading) {
          return <CircularProgress size={20} />;
        } else if (isError) {
          return <Typography color="error">Connection failed</Typography>;
        } else {
          return <Typography color="textPrimary">Backend: {data}</Typography>;
        }
      })()}
      <Button size="small" onClick={() => refetch()}>
        Check
      </Button>
    </Box>
  );
}
