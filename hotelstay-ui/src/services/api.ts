import type { Hotel, ReservationRequest, ReservationResponse, RoomType } from '../types';

const API_BASE = 'http://localhost:5042';

function buildSearchUrl(destination: string, checkIn: string, checkOut: string, roomType?: RoomType) {
  const params = new URLSearchParams({
    destination,
    checkIn,
    checkOut,
  });
  if (roomType) {
    params.set('roomType', roomType);
  }
  return `${API_BASE}/hotels/search?${params.toString()}`;
}

async function fetchJson<T>(url: string, options?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });

  const payload = await response.json().catch(() => null);
  if (!response.ok) {
    throw new Error(payload?.error || payload?.Error || 'API request failed');
  }
  return payload as T;
}

export async function searchHotels(destination: string, checkIn: string, checkOut: string, roomType?: RoomType) {
  return fetchJson<Hotel[]>(buildSearchUrl(destination, checkIn, checkOut, roomType));
}

export async function reserveHotel(request: ReservationRequest) {
  return fetchJson<ReservationResponse>(`${API_BASE}/hotels/reserve`, {
    method: 'POST',
    body: JSON.stringify(request),
  });
}

export async function getReservation(reference: string) {
  return fetchJson<ReservationResponse>(`${API_BASE}/hotels/reservation/${reference}`);
}
