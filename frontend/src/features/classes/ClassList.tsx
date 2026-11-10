import { useEffect, useState } from 'react';
import { getClasses } from '../../api/classes';
import type { ClassSeats } from '../../api/types';

export default function ClassList() {
    const [classes, setClasses] = useState<ClassSeats[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let cancelled = false;

        getClasses()
            .then((data) => {
                if (cancelled) return;
                setClasses(data);
                setError(null);
            })
            .catch((e: Error) => {
                if (!cancelled) setError(e.message);
            })
            .finally(() => {
                if (!cancelled) setLoading(false);
            });

        return () => {
            cancelled = true;
        };
    }, []);

    if (loading) return <p>Loading classes...</p>;
    if (error) return <p role="alert">{error}</p>;
    if (classes.length === 0) return <p>No classes have been created yet.</p>;

    return (
        <table>
            <thead>
                <tr>
                    <th>Class</th>
                    <th>Total</th>
                    <th>Held</th>
                    <th>Confirmed</th>
                    <th>Available</th>
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
                    </tr>
                ))}
            </tbody>
        </table>
    );
}