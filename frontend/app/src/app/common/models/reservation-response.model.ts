export interface ReservationResponse {
  id: number;
  eventId: number;
  eventName: string;
  customerName: string;
  ticketQuantity: number;
  totalPrice: number;
  reservationStatusName: string;
  reservationCode: string;
  reservationDate: string;
  cancelledDate?: string;
}
