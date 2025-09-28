import React from "react";
import type { SvgIconProps } from "@mui/material/SvgIcon";
import Box from "@mui/material/Box";
import { ICONS } from "./Icons";
import HelpOutlineIcon from "@mui/icons-material/HelpOutline";

function getIcon(name?: string | null, props?: SvgIconProps): React.ReactElement {
  if (!name) return <HelpOutlineIcon {...props} />;
  const key = String(name).trim();
  const lower = key.toLowerCase();
  const IconComp = ICONS[key] ?? ICONS[lower] ?? HelpOutlineIcon;
  return <IconComp {...props} />;
}

// simple hex check
function isHexColor(value?: string | null) {
  return typeof value === "string" && /^#([0-9A-F]{3}|[0-9A-F]{6})$/i.test(value.trim());
}

function hexToRgb(hex: string) {
  const clean = hex.replace("#", "");
  const full = clean.length === 3 ? clean.split("").map(c => c + c).join("") : clean;
  const num = parseInt(full, 16);
  return { r: (num >> 16) & 255, g: (num >> 8) & 255, b: num & 255 };
}

function getContrastColor(hex: string) {
  const { r, g, b } = hexToRgb(hex);
  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
  return luminance > 0.6 ? "#000000" : "#ffffff";
}

export function Icon({
  name,
  colorOf = '#1976d2',
  size = "medium",
  sx,
  ...props
}: { name?: string | null; colorOf?: string; size?: "small" | "medium" | "large"; sx?: React.CSSProperties } & SvgIconProps): React.ReactElement {
  if (colorOf) {
    const bg = colorOf;
    const iconColor = isHexColor(bg) ? getContrastColor(bg) : "#ffffff";
    const wrapperSize = size === "small" ? 32 : size === "large" ? 44 : 36;

    const wrapperSx: React.CSSProperties = {
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      backgroundColor: bg,
      borderRadius: "50%",
      padding: 1,
      lineHeight: 0,
      width: wrapperSize,
      height: wrapperSize,
      boxSizing: "border-box",
      ...sx,
    };

    // ensure svg scales and is centered
    const iconStyle = {
      width: Math.round(wrapperSize * 0.6),
      height: Math.round(wrapperSize * 0.6),
      display: "block",
    };

    return (
      <Box sx={wrapperSx}>
        {getIcon(name, { ...props, htmlColor: iconColor, style: iconStyle })}
      </Box>
    );
  }

  return getIcon(name, { ...props });
}

export default Icon;
