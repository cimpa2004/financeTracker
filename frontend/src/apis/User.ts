import { httpService } from "../services/httpService";
import { UserNestedSchema, UserSchema } from "../types/User";
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useAuth } from '../contexts/AuthContext';
import type { User } from '../types/User';

export async function getProfile() {
  try {
    const resp = await httpService.get('user', UserSchema);
    return resp;
  } catch {
    return null;
  }
}

export function useProfile() {
  return useQuery({ queryKey: ['profile'], queryFn: getProfile, retry: false, refetchOnWindowFocus: false });
}

export async function updateProfile(data: { username?: string; email?: string; password?: string }) {
  const response = await httpService.put('user', UserNestedSchema, data, { operation: 'updateUser' }).catch(() => null);
  return response;
}

export function useUpdateProfile() {
  const queryClient = useQueryClient();
  const { setUser } = useAuth();
  return useMutation({
    mutationFn: updateProfile,
    onSuccess: (resp) => {
      // update local storage so AuthProvider can pick it up
      try {
        if (resp) {
          // resp may contain a subset of user fields
          const stored = localStorage.getItem('user');
          const current = stored ? JSON.parse(stored) : {};
          const next = { ...current, ...resp };
          localStorage.setItem('user', JSON.stringify(next));
          try {
            setUser(next as User);
          } catch {
            // swallow errors from setUser to avoid breaking the mutation flow
          }
        }
      } catch {
        // ignore localStorage errors
      }
      queryClient.invalidateQueries({ queryKey: ['profile'] });
    },
  });
}
