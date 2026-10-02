import { useMutation } from "@tanstack/react-query";

import {
  login,
  logout,
  register,
} from "../api/authApi";

export function useLogin() {
  return useMutation({
    mutationFn: login,
  });
}

export function useRegister() {
  return useMutation({
    mutationFn: register,
  });
}

export function useLogout() {
  return useMutation({
    mutationFn: logout,
  });
}