import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from '@core/api-base-url';

@Injectable({ providedIn: 'root' })
export class ReportsStore {
  private readonly http = inject(HttpClient);
  private readonly apiBase = inject(API_BASE_URL);

  /** Command: download the occupancy report as a PDF (admin). */
  downloadPdf() {
    return this.http.get(`${this.apiBase}/reports/occupancy/pdf`, { responseType: 'blob' });
  }
}
