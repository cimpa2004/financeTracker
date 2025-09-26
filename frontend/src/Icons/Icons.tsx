import React, { type JSX } from "react";
import type { SvgIconProps } from "@mui/material/SvgIcon";
import {ICONS} from "./Icons";
import HelpOutlineIcon from "@mui/icons-material/HelpOutline";

function getIcon(name?: string | null, props?: SvgIconProps): JSX.Element {
  if (!name) return <HelpOutlineIcon {...props} />;
  const key = String(name).trim();
  const lower = key.toLowerCase();
  const IconComp = ICONS[key] ?? ICONS[lower] ?? HelpOutlineIcon;
  return <IconComp {...props} />;
}

export function Icon({ name, ...props }: { name?: string | null } & SvgIconProps): JSX.Element {
  return getIcon(name, props);
}
