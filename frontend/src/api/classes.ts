import { request } from './client';
import type { ClassSeats } from './types';

export const getClasses = () => request<ClassSeats[]>('/classes');