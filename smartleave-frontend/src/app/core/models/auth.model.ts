export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  fullName: string;
  email: string;
  password: string;
  role?: string;
  departmentId?: string | null;
}

export interface AuthResponseDto {
  token: string;
  fullName: string;
  email: string;
  role: string;
  expiresAt: string;
}
