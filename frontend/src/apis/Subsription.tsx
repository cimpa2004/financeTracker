import { SubscriptionArraySchema } from "../types/Subscription";
import { httpService } from "../services/httpService";
import { useQuery } from "@tanstack/react-query";

async function getAllSubscriptions() {
    const response = await httpService.get("subscriptions", SubscriptionArraySchema);
    return response;
}

export function useSubscriptionApi() {
    return useQuery({
        queryKey: ["subscriptions"],
        queryFn: getAllSubscriptions,
    });
}

async function getLast3Subscriptions() {
    const response = await httpService.get("subscriptions/last3", SubscriptionArraySchema);
    return response;
}

export function useLast3Subscriptions() {
    return useQuery({
        queryKey: ["subscriptions", "last3"],
        queryFn: getLast3Subscriptions,
    });
}