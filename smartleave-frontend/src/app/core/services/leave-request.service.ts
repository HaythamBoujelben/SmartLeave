import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateLeaveRequestDto,
  LeaveRequestDto,
  ReviewLeaveRequestDto
} from '../models/leave-request.model';
import { API_BASE_URL } from '../config';

@Injectable({ providedIn: 'root' })
export class LeaveRequestService {
  private http = inject(HttpClient);
  private base = `${API_BASE_URL}/api/LeaveRequest`;

  create(dto: CreateLeaveRequestDto): Observable<LeaveRequestDto> {
    return this.http.post<LeaveRequestDto>(this.base, dto);
  }

  getMy(): Observable<LeaveRequestDto[]> {
    return this.http.get<LeaveRequestDto[]>(`${this.base}/my`);
  }

  getPending(): Observable<LeaveRequestDto[]> {
    return this.http.get<LeaveRequestDto[]>(`${this.base}/pending`);
  }

  review(id: string, dto: ReviewLeaveRequestDto): Observable<unknown> {
    return this.http.put(`${this.base}/${id}/review`, dto);
  }

  cancel(id: string): Observable<unknown> {
    return this.http.put(`${this.base}/${id}/cancel`, {});
  }
}
