import React from "react";
import { Button, Popover, IconButton, Box } from "@mui/material";
import { ICONS } from "../Icons/Icons.ts";
import { Icon } from "../Icons/Icons.tsx";

interface IconSelectorProps {
  onSelect: (iconName: string) => void;
  value?: string | null;
}

export default function IconSelector({ onSelect, value }: IconSelectorProps) {
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };
  const handleClose = () => {
    setAnchorEl(null);
  };
  const handleSelect = (name: string) => {
    onSelect(name);
    handleClose();
  };

  return (
    <>
      <Button
        variant="outlined"
        onClick={handleClick}
        startIcon={
          <Box
            sx={{
              width: 36,
              height: 36,
              bgcolor: "transparent",
              overflow: "hidden",
            }}
          >
            {value ? <Icon name={value} /> : null}
          </Box>
        }
      >
        Select icon
      </Button>

      <Popover
        open={open}
        anchorEl={anchorEl}
        onClose={handleClose}
        anchorOrigin={{ vertical: "bottom", horizontal: "left" }}
        transformOrigin={{ vertical: "top", horizontal: "left" }}
        slotProps={{
          paper: {
            sx: {
              mt: 1,
              p: 1,
              maxHeight: 400,
              width: 320,
              overflowY: "auto",
            },
          },
        }}
      >
        <ul
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(6, 1fr)",
            gap: 8,
            alignItems: "center",
            justifyItems: "center",
            padding: 0,
            margin: 0,
            listStyle: "none",
          }}
        >
          {Object.keys(ICONS).map((name) => (
            <li key={name} style={{ textAlign: "center" }}>
              <IconButton
                onClick={() => handleSelect(name)}
                size="small"
                sx={{
                  width: 44,
                  height: 44,
                  borderRadius: 1,
                  p: 0,
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  border: name === value ? "2px solid" : "1px solid transparent",
                  borderColor: name === value ? "primary.main" : "transparent",
                  bgcolor: "background.paper",
                }}
              >
                <Box sx={{ width: 28, height: 28, display: "flex", alignItems: "center", justifyContent: "center" }}>
                  <Icon name={name} />
                </Box>
              </IconButton>
            </li>
          ))}
        </ul>
      </Popover>
    </>
  );
}