import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { LeaveRequestService } from '../../../core/services/leave-request.service';
import { AuthService } from '../../../core/services/auth.service';
import { LeaveRequestDto } from '../../../core/models/leave-request.model';

@Component({
  selector: 'app-my-requests',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatCardModule],
  templateUrl: './my-requests.component.html'
})
export class MyRequestsComponent implements OnInit {
  private service = inject(LeaveRequestService);
  private auth = inject(AuthService);

  requests: LeaveRequestDto[] = [];
  loading = false;
  error = '';
  displayedColumns = ['leaveTypeName', 'startDate', 'endDate', 'totalDays', 'status', 'actions'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service.getMy().subscribe({
      next: (data) => { this.requests = data; this.loading = false; },
      error: () => { this.error = 'Failed to load requests.'; this.loading = false; }
    });
  }

  cancel(id: string): void {
    this.service.cancel(id).subscribe({ next: () => this.load() });
  }

  logout(): void { this.auth.logout(); }
}
