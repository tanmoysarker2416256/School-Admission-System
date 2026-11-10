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