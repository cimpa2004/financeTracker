import { Menu, MenuItem } from "@mui/material";
import { ICONS } from "../Icons/Icons.ts"
import {Icon} from "../Icons/Icons.tsx"
import React from "react";

interface IconSelectorProps {
  onSelect : (iconName: string) => void;
}

export default function IconSelector({ onSelect }: IconSelectorProps) {
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };
  const handleClose = () => {
    setAnchorEl(null);
  };
  return (
    <Menu anchorEl={anchorEl} open={open} onClose={handleClose} onClick={handleClick}>
      {Object.entries(ICONS).map(([name]) => (
        <MenuItem key={name} onClick={() => { handleClose(); onSelect(name); }}>
          <Icon name={name} />
        </MenuItem>
      ))}
    </Menu>
  );
}