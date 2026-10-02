export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
}

export interface CurrentUser {
  id: string;
  fullName: string | null;
  email: string;
  phoneNumber: string | null;
  profileComplete: boolean;
}

export interface UpdateProfileRequest {
  fullName: string;
  phoneNumber: string;
}