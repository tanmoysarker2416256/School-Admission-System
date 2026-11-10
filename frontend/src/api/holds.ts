import { request } from './client';
import type { HoldCreated } from './types';

export const applyForSeat = (classId: string, studentId: string) =>
    request<HoldCreated>(`/classes/${classId}/holds`, { method: 'POST', studentId });