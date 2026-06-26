export interface CreateEventRequest {
  name: string;
  description: string;
  venueId: number;
  maxCapacity: number;
  startDate: string;
  endDate: string;
  price: number;
  eventTypeId: number;
}
