import { Box } from '@mui/material';
import AddBudgetForm from '../components/AddBudgetForm';

export default function AddBudgetPage() {
	return (
		<Box justifyContent={"center"} alignItems="center" width='100vw'>
			<AddBudgetForm />
		</Box>
	);
}