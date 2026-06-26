export interface CreateReservationRequest {
  eventId: number;
  ticketQuantity: number;
  customerName: string;
  customerEmail: string;
  customerPhone?: string;
}
