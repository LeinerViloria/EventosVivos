import { EventStatus } from '../enums/event-status';

/** A row of the occupancy report (GET /api/v1/reports/occupancy, admin). */
export interface OccupancyReportItem {
  eventId: string;
  eventTitle: string;
  capacity: number;
  soldTickets: number;
  availableTickets: number;
  occupancyPercent: number;
  revenue: number;
  status: EventStatus;
}
