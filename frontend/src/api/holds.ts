import { request } from './client';
import type { HoldCreated, MyHold } from './types';

export const applyForSeat = (classId: string, studentId: string) =>
    request<HoldCreated>(`/classes/${classId}/holds`, { method: 'POST', studentId });

export const getMyHolds = (studentId: string) =>
    request<MyHold[]>('/me/holds', { studentId });

export const confirmHold = (holdId: string, studentId: string) =>
    request<{ holdId: string; status: string }>(`/holds/${holdId}/confirm`, { method: 'POST', studentId });

export const cancelHold = (holdId: string, studentId: string) =>
    request<void>(`/holds/${holdId}`, { method: 'DELETE', studentId });