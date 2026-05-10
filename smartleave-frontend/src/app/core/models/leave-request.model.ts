export interface LeaveRequestDto {
  id: string;
  employeeName: string;
  leaveTypeName: string;
  startDate: string;
  endDate: string;
  totalDays: number;
  status: string;
  reason?: string | null;
  managerNote?: string | null;
  createdAt: string;
}

export interface CreateLeaveRequestDto {
  leaveTypeId: string;
  startDate: string;
  endDate: string;
  reason?: string | null;
}

export interface ReviewLeaveRequestDto {
  isApproved: boolean;
  managerNote?: string | null;
}
