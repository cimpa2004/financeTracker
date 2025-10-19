import { styled } from '@mui/material/styles';
import Box from '@mui/material/Box';

// A wrapper that applies a themed scrollbar. Works on WebKit and Firefox.
const ThemedScrollbar = styled(Box)(({ theme }) => ({
  maxHeight: 220,
  overflowY: 'auto',
  paddingRight: 8,
  /* WebKit */
  '&::-webkit-scrollbar': {
    width: 10,
    height: 10,
  },
  '&::-webkit-scrollbar-track': {
    background: theme.palette.background.paper,
    borderRadius: 8,
  },
  '&::-webkit-scrollbar-thumb': {
    background: theme.palette.action.selected,
    borderRadius: 8,
    border: `2px solid ${theme.palette.background.paper}`,
  },
  /* Firefox */
  scrollbarWidth: 'thin',
  scrollbarColor: `${theme.palette.action.selected} ${theme.palette.background.paper}`,
}));

export default ThemedScrollbar;
