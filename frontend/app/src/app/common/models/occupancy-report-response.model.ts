export interface OccupancyReportResponse {
  eventId: number;
  eventName: string;
  ticketsSold: number;
  ticketsAvailable: number;
  occupancyPercentage: number;
  totalRevenue: number;
  eventStatusName: string;
}
