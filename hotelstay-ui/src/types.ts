export type RoomType = 'Standard' | 'Deluxe' | 'Suite';
export type DocumentType = 'Passport' | 'NationalId';
export type CancellationPolicy = 'FreeCancellation' | 'Flexible' | 'NonRefundable';

export interface Hotel {
  provider: string;
  hotelId: string;
  name: string;
  roomType: RoomType;
  ratePerNight: number;
  totalPrice: number;
  cancellationPolicy: CancellationPolicy;
  amenities?: string[];
  starRating?: number;
}

export interface ReservationRequest {
  destination: string;
  checkIn: string;
  checkOut: string;
  hotelId: string;
  roomType: RoomType;
  guestName: string;
  documentType: DocumentType;
  documentNumber: string;
}

export interface ReservationResponse {
  reference: string;
  provider: string;
  hotelId: string;
  hotelName: string;
  destination: string;
  checkIn: string;
  checkOut: string;
  guestName: string;
  documentType: DocumentType;
  documentNumber: string;
  roomType: RoomType;
  totalPrice: number;
  cancellationPolicy: CancellationPolicy;
}
