import { useCallback, useEffect, useState } from 'react';
import { getClasses } from '../../api/classes';
import { applyForSeat } from '../../api/holds';
import type { ClassSeats } from '../../api/types';

const REFRESH_MS = 5000;

interface Props {
    studentId: string | null;
    refreshKey: number;
    onChanged: () => void;
    showApply?: boolean;
}

export default function ClassList({ studentId, refreshKey, onChanged, showApply = true }: Props) {
    const [classes, setClasses] = useState<ClassSeats[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);
    const [applyingId, setApplyingId] = useState<string | null>(null);
    const [message, setMessage] = useState<{ ok: boolean; text: string } | null>(null);

    const load = useCallback(async () => {
        try {
            setClasses(await getClasses());
            setError(null);
        } catch (e) {
            setError((e as Error).message);
        } finally {
            setLoading(false);
        }
    }, []);



    useEffect(() => {
        load();
        const timer = setInterval(load, REFRESH_MS);
        return () => clearInterval(timer);
    }, [load]);

    useEffect(() => {
        load();
        const timer = setInterval(load, REFRESH_MS);
        return () => clearInterval(timer);
    }, [load, refreshKey]);



    async function handleApply(classId: string) {
        if (!studentId) return;
        setApplyingId(classId);
        setMessage(null);
        try {
            await applyForSeat(classId, studentId);
            onChanged();   // the application panel and the class list both reload
        } catch (e) {
            setMessage({ ok: false, text: (e as Error).message });
        } finally {
            setApplyingId(null);
        }
    }

    if (loading) return <p>Loading classes...</p>;

    return (
        <section>
            {showApply && !studentId && <p>Enter your 17-digit student ID above to apply.</p>}
            {error && <p role="alert">{error}</p>}
            {message && <p role={message.ok ? 'status' : 'alert'}>{message.text}</p>}

            {classes.length === 0 && !error ? (
                <p>No classes have been created yet.</p>
            ) : (
                <table>
                    <thead>
                        <tr>
                            <th>Class</th>
                            <th>Total</th>
                            <th>Held</th>
                            <th>Confirmed</th>
                            <th>Available</th>
                            {showApply && <th></th>}
                        </tr>
                    </thead>
                    <tbody>
                        {classes.map((c) => (
                            <tr key={c.id}>
                                <td>{c.name}</td>
                                <td>{c.totalSeats}</td>
                                <td>{c.heldSeats}</td>
                                <td>{c.confirmedSeats}</td>
                                <td>{c.availableSeats}</td>
                                {showApply && (
                                    <td>
                                        <button
                                            disabled={!studentId || applyingId !== null || c.availableSeats === 0}
                                            onClick={() => handleApply(c.id)}
                                        >
                                            {c.availableSeats === 0 ? 'Full' : applyingId === c.id ? 'Applying...' : 'Apply'}
                                        </button>
                                    </td>
                                )}
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </section>
    );
}