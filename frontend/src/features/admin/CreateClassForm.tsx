import { useState } from 'react';
import type { FormEvent } from 'react';
import { createClass } from '../../api/classes';

const GRADES = Array.from({ length: 10 }, (_, i) => String(i + 1));
const SHIFTS = ['Morning', 'Day'];

interface Props {
    onCreated: () => void;
}

export default function CreateClassForm({ onCreated }: Props) {
    const [grade, setGrade] = useState('1');
    const [shift, setShift] = useState('Morning');
    const [seats, setSeats] = useState('');
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    const name = `Class ${grade} - ${shift} Shift`;

    async function submit(e: FormEvent) {
        e.preventDefault();
        setError(null);
        setSuccess(null);

        const totalSeats = Number(seats);
        if (!Number.isInteger(totalSeats) || totalSeats < 1) {
            setError('Total seats must be a whole number greater than zero.');
            return;
        }

        setBusy(true);
        try {
            const created = await createClass(name, totalSeats);
            setSuccess(`Created "${created.name}" with ${created.totalSeats} seats.`);
            setSeats('');
            onCreated();
        } catch (err) {
            setError((err as Error).message);
        } finally {
            setBusy(false);
        }
    }

    return (
        <section>
            <h2>Create a class</h2>
            <form onSubmit={submit}>
                <p>
                    <label>
                        Grade{' '}
                        <select value={grade} onChange={(e) => setGrade(e.target.value)}>
                            {GRADES.map((g) => (
                                <option key={g} value={g}>
                                    Class {g}
                                </option>
                            ))}
                        </select>
                    </label>{' '}
                    <label>
                        Shift{' '}
                        <select value={shift} onChange={(e) => setShift(e.target.value)}>
                            {SHIFTS.map((s) => (
                                <option key={s} value={s}>
                                    {s} Shift
                                </option>
                            ))}
                        </select>
                    </label>{' '}
                    <label>
                        Total seats{' '}
                        <input
                            value={seats}
                            inputMode="numeric"
                            maxLength={6}
                            onChange={(e) => setSeats(e.target.value.replace(/\D/g, ''))}
                        />
                    </label>
                </p>
                <p>
                    Class name: <strong>{name}</strong>
                </p>
                <button type="submit" disabled={busy}>
                    {busy ? 'Creating...' : 'Create class'}
                </button>
            </form>

            {error && <p role="alert">{error}</p>}
            {success && <p role="status">{success}</p>}
        </section>
    );
}