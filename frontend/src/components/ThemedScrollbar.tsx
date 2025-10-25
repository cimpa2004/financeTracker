import { styled } from '@mui/material/styles';
import Box from '@mui/material/Box';
import type { BoxProps } from '@mui/material/Box';
import React from 'react';

const Root = styled(Box)(({ theme }) => ({
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

type ThemedScrollbarProps = BoxProps & {
  fullVh?: boolean;
};

const ThemedScrollbar: React.FC<ThemedScrollbarProps> = ({ fullVh = false, sx, children, ...rest }) => {
  const sizeStyle: BoxProps['sx'] = fullVh
    ? { height: '100vh', maxHeight: '100vh' }
    : { maxHeight: 220 };

  const mergedSx = Array.isArray(sx) ? [...sx, sizeStyle] : { ...(sx as object), ...sizeStyle };

  return (
    <Root sx={mergedSx} {...(rest as BoxProps)}>
      {children}
    </Root>
  );
};

export default ThemedScrollbar;
