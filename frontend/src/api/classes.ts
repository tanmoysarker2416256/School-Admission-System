import { request } from './client';
import type { ClassCreated, ClassSeats } from './types';

export const getClasses = () => request<ClassSeats[]>('/classes');
export const createClass = (name: string, totalSeats: number) =>
    request<ClassCreated>('/classes', { method: 'POST', body: { name, totalSeats } });