import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { LeaveRequestService } from '../../../core/services/leave-request.service';
import { AuthService } from '../../../core/services/auth.service';
import { LeaveRequestDto } from '../../../core/models/leave-request.model';

@Component({
  selector: 'app-pending-requests',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatCardModule],
  templateUrl: './pending-requests.component.html'
})
export class PendingRequestsComponent implements OnInit {
  private service = inject(LeaveRequestService);
  private auth = inject(AuthService);

  requests: LeaveRequestDto[] = [];
  loading = false;
  error = '';
  displayedColumns = ['employeeName', 'leaveTypeName', 'startDate', 'endDate', 'totalDays', 'reason', 'actions'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service.getPending().subscribe({
      next: (data) => { this.requests = data; this.loading = false; },
      error: () => { this.error = 'Failed to load pending requests.'; this.loading = false; }
    });
  }

  review(id: string, isApproved: boolean): void {
    this.service.review(id, { isApproved }).subscribe({ next: () => this.load() });
  }

  logout(): void { this.auth.logout(); }
}
