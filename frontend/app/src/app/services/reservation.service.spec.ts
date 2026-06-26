import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../environments/environment';
import { CreateReservationRequest } from '../common/models/create-reservation-request.model';
import { ReservationResponse } from '../common/models/reservation-response.model';
import { ReservationService } from './reservation.service';

describe('ReservationService', () => {
  let reservationService: ReservationService;
  let httpTesting: HttpTestingController;

  const sampleReservation: ReservationResponse = {
    id: 1,
    eventId: 1,
    eventName: 'Sample Conference',
    customerName: 'John Doe',
    ticketQuantity: 2,
    totalPrice: 100,
    reservationStatusName: 'PendingPayment',
    reservationCode: '',
    reservationDate: '2026-06-26T10:00:00',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    reservationService = TestBed.inject(ReservationService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('createReservation_WhenCalled_PostsToReservationsUrlWithRequestBody', () => {
    const request: CreateReservationRequest = {
      eventId: 1,
      ticketQuantity: 2,
      customerName: 'John Doe',
      customerEmail: 'john.doe@example.com',
    };

    let result: ReservationResponse | undefined;
    reservationService.createReservation(request).subscribe((response) => (result = response));

    const httpRequest = httpTesting.expectOne(`${environment.apiUrl}/reservations`);
    expect(httpRequest.request.method).toBe('POST');
    expect(httpRequest.request.body).toEqual(request);

    httpRequest.flush({ success: true, data: sampleReservation, statusCode: 201, message: null });

    expect(result).toEqual(sampleReservation);
  });

  it('confirmPayment_WhenCalled_PatchesExactConfirmUrl', () => {
    const confirmedReservation: ReservationResponse = { ...sampleReservation, reservationStatusName: 'Confirmed', reservationCode: 'EV-123456' };

    let result: ReservationResponse | undefined;
    reservationService.confirmPayment(1).subscribe((response) => (result = response));

    const httpRequest = httpTesting.expectOne(`${environment.apiUrl}/reservations/1/confirm`);
    expect(httpRequest.request.method).toBe('PATCH');

    httpRequest.flush({ success: true, data: confirmedReservation, statusCode: 200, message: null });

    expect(result).toEqual(confirmedReservation);
  });

  it('cancelReservation_WhenCalled_PatchesExactCancelUrl', () => {
    const cancelledReservation: ReservationResponse = { ...sampleReservation, reservationStatusName: 'Cancelled' };

    let result: ReservationResponse | undefined;
    reservationService.cancelReservation(1).subscribe((response) => (result = response));

    const httpRequest = httpTesting.expectOne(`${environment.apiUrl}/reservations/1/cancel`);
    expect(httpRequest.request.method).toBe('PATCH');

    httpRequest.flush({ success: true, data: cancelledReservation, statusCode: 200, message: null });

    expect(result).toEqual(cancelledReservation);
  });

  it('releaseReservation_WhenCalled_PatchesExactReleaseUrl', () => {
    const releasedReservation: ReservationResponse = { ...sampleReservation, reservationStatusName: 'Cancelled' };

    let result: ReservationResponse | undefined;
    reservationService.releaseReservation(1).subscribe((response) => (result = response));

    const httpRequest = httpTesting.expectOne(`${environment.apiUrl}/reservations/1/release`);
    expect(httpRequest.request.method).toBe('PATCH');

    httpRequest.flush({ success: true, data: releasedReservation, statusCode: 200, message: null });

    expect(result).toEqual(releasedReservation);
  });

  it('getReservationsByEventId_WhenCalled_SendsEventIdQueryParam', () => {
    let result: ReservationResponse[] | undefined;
    reservationService.getReservationsByEventId(1).subscribe((response) => (result = response));

    const httpRequest = httpTesting.expectOne(
      (req) => req.url === `${environment.apiUrl}/reservations` && req.method === 'GET'
    );

    expect(httpRequest.request.params.keys()).toEqual(['eventId']);
    expect(httpRequest.request.params.get('eventId')).toBe('1');

    httpRequest.flush({ success: true, data: [sampleReservation], statusCode: 200, message: null });

    expect(result).toEqual([sampleReservation]);
  });

  it('getReservationsByEmail_WhenCalled_SendsCustomerEmailQueryParam', () => {
    let result: ReservationResponse[] | undefined;
    reservationService.getReservationsByEmail('john.doe@example.com').subscribe((response) => (result = response));

    const httpRequest = httpTesting.expectOne(
      (req) => req.url === `${environment.apiUrl}/reservations` && req.method === 'GET'
    );

    expect(httpRequest.request.params.keys()).toEqual(['customerEmail']);
    expect(httpRequest.request.params.get('customerEmail')).toBe('john.doe@example.com');

    httpRequest.flush({ success: true, data: [sampleReservation], statusCode: 200, message: null });

    expect(result).toEqual([sampleReservation]);
  });
});
