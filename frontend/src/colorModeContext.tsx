import { createContext, useContext } from 'react';
import type { PaletteMode } from '@mui/material';

export type ColorModeContextType = {
  mode: PaletteMode;
  toggleMode: () => void;
};

export const ColorModeContext = createContext<ColorModeContextType | undefined>(undefined);

export function useColorMode() {
  const ctx = useContext(ColorModeContext);
  if (!ctx) throw new Error('useColorMode must be used within AppThemeProvider');
  return ctx;
}
