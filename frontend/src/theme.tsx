import React, { useEffect, useMemo, useState } from 'react';
import { createTheme, CssBaseline, ThemeProvider as MUIThemeProvider } from '@mui/material';
import type { PaletteMode } from '@mui/material';
import { ColorModeContext } from './contexts/colorModeContext';

function getDesignTokens(mode: PaletteMode) {
  return {
    palette: {
      mode,
      ...(mode === 'light'
        ? {
            primary: {
              main: '#0b5e54',
              contrastText: '#ffffff',
            },
            secondary: {
              main: '#ffb300',
              contrastText: '#0b0b0b',
            },
            success: {
              main: '#2e7d32',
              contrastText: '#ffffff',
            },
            error: {
              main: '#d32f2f',
              contrastText: '#ffffff',
            },
            warning: {
              main: '#f57c00',
              contrastText: '#0b0b0b',
            },
            info: {
              main: '#1976d2',
              contrastText: '#ffffff',
            },
            background: {
              default: '#f4f7f6',
              paper: '#ffffff',
            },
            text: {
              primary: '#0b1b1a',
              secondary: '#425051',
              disabled: '#8a9596',
            },
            divider: 'rgba(11,27,26,0.08)',
          }
        : {
            primary: {
              main: '#55c7a3',
              contrastText: '#012017',
            },
            secondary: {
              main: '#ffb74d',
              contrastText: '#071011',
            },
            success: {
              main: '#66bb6a',
              contrastText: '#012017',
            },
            error: {
              main: '#ef5350',
              contrastText: '#071011',
            },
            warning: {
              main: '#ffb74d',
              contrastText: '#071011',
            },
            info: {
              main: '#64b5f6',
              contrastText: '#071011',
            },
            background: {
              default: '#071217',
              paper: '#0b1b1a',
            },
            text: {
              primary: '#e6f5ef',
              secondary: '#bfe6d6',
              disabled: 'rgba(230,245,239,0.6)',
            },
            divider: 'rgba(255,255,255,0.06)',
          }),
    },
    typography: {
      fontFamily: ['Inter', 'Roboto', 'Helvetica', 'Arial', 'sans-serif'].join(','),
      h1: { fontWeight: 700 },
      h2: { fontWeight: 600 },
      h3: { fontWeight: 600 },
      button: { textTransform: 'none', fontWeight: 600 },
    },
    shape: {
      borderRadius: 8,
    },
    components: {
      MuiButton: {
        styleOverrides: {
          containedPrimary: {
            boxShadow: 'none',
          },
        },
      },
      MuiPaper: {
        styleOverrides: {
          root: {},
        },
      },
    },
  };
}

export const AppThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [mode, setMode] = useState<PaletteMode>(() => {
    const fallback: PaletteMode = 'light';
    try {
      const stored = localStorage.getItem('color-mode');
      if (stored === 'light' || stored === 'dark') return stored as PaletteMode;
      if (typeof window !== 'undefined' && window.matchMedia) {
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
      }
    } catch (err) {
      console.warn('Unable to read color-mode preference, defaulting to', fallback, err);
    }
    return fallback;
  });

  useEffect(() => {
    try {
      localStorage.setItem('color-mode', mode);
    } catch (err) {
      console.warn('Unable to persist color-mode preference', err);
    }
  }, [mode]);

  const colorMode = useMemo(
    () => ({
      mode,
      toggleMode: () => setMode((prev) => (prev === 'light' ? 'dark' : 'light')),
    }),
    [mode],
  );

  const theme = useMemo(() => createTheme(getDesignTokens(mode as PaletteMode)), [mode]);

  return (
    <ColorModeContext.Provider value={colorMode}>
      <MUIThemeProvider theme={theme}>
        <CssBaseline />
        {children}
      </MUIThemeProvider>
    </ColorModeContext.Provider>
  );
};
