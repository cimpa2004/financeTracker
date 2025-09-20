import { Box } from "@mui/material";
import SpentThisMonth from "../components/SpentThisMonth";

export default function Home() {
    return (
        <Box 
            textAlign="center" 
            display="flex" 
            flexDirection="column" 
            justifyContent="center" 
            alignItems={"flex-start"}
            >
            <SpentThisMonth />
        </Box>
    )
}