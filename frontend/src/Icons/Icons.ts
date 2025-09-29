import MonetizationOnIcon from "@mui/icons-material/MonetizationOn";
import AccountBalanceIcon from "@mui/icons-material/AccountBalance";
import SavingsIcon from "@mui/icons-material/Savings";
import AttachMoneyIcon from "@mui/icons-material/AttachMoney";
import CardGiftcardIcon from "@mui/icons-material/CardGiftcard";
import WorkIcon from "@mui/icons-material/Work";

// expense-related icons
import ShoppingCartIcon from "@mui/icons-material/ShoppingCart";
import LocalGroceryStoreIcon from "@mui/icons-material/LocalGroceryStore";
import RestaurantIcon from "@mui/icons-material/Restaurant";
import LocalGasStationIcon from "@mui/icons-material/LocalGasStation";
import DirectionsCarIcon from "@mui/icons-material/DirectionsCar";
import HomeWorkIcon from "@mui/icons-material/HomeWork"; // rent / mortgage
import ReceiptIcon from "@mui/icons-material/Receipt";
import CreditCardIcon from "@mui/icons-material/CreditCard";
import LocalAtmIcon from "@mui/icons-material/LocalAtm";
import LocalCafeIcon from "@mui/icons-material/LocalCafe";
import FlightIcon from "@mui/icons-material/Flight";
import LocalHospitalIcon from "@mui/icons-material/LocalHospital";
import SchoolIcon from "@mui/icons-material/School";
import SubscriptionsIcon from "@mui/icons-material/Subscriptions";
import MovieIcon from "@mui/icons-material/Movie";
import LocalLaundryServiceIcon from "@mui/icons-material/LocalLaundryService";
import ReceiptLongIcon from "@mui/icons-material/ReceiptLong";
import LocalOfferIcon from "@mui/icons-material/LocalOffer";
import HelpOutlineIcon from "@mui/icons-material/HelpOutline";
import type { SvgIconProps } from "@mui/material";

export const ICONS: Record<string, React.ComponentType<SvgIconProps>> = {
  // income / primary lookups
  income: MonetizationOnIcon,

  // bank / salary / savings
  salary: AccountBalanceIcon,
  savings: SavingsIcon,
  money: AttachMoneyIcon,
  gift: CardGiftcardIcon,
  work: WorkIcon,

  // expenses
  expense: ReceiptIcon,
  receiptLong: ReceiptLongIcon,
  creditCard: CreditCardIcon,
  atm: LocalAtmIcon,
  shoppingCart: ShoppingCartIcon,
  groceries: LocalGroceryStoreIcon,
  restaurant: RestaurantIcon,
  cafe: LocalCafeIcon,
  fuel: LocalGasStationIcon,
  transport: DirectionsCarIcon,
  rent: HomeWorkIcon,
  shoppingOffer: LocalOfferIcon,
  travel: FlightIcon,
  health: LocalHospitalIcon,
  education: SchoolIcon,
  subscription: SubscriptionsIcon,
  entertainment: MovieIcon,
  laundry: LocalLaundryServiceIcon,

  // fallback
  unknown: HelpOutlineIcon,
};