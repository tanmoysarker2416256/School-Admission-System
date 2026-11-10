export interface ClassSeats {
    id: string;
    name: string;
    totalSeats: number;
    heldSeats: number;
    confirmedSeats: number;
    availableSeats: number;
}
export interface HoldCreated {
    holdId: string;
    classId: string;
    studentId: string;
    status: string;
    expiresAt: string;
}
export interface MyHold {
    holdId: string;
    classId: string;
    className: string;
    status: 'HELD' | 'CONFIRMED' | 'EXPIRED' | 'CANCELLED';
    expiresAt: string;
}