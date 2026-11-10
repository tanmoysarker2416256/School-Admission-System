import { useCallback, useEffect, useState } from 'react';
import { getMyHolds } from '../../api/holds';
import type { MyHold } from '../../api/types';

interface Props {
    studentId: string;
    refreshKey: number;
}

export default function MyApplications({ studentId, refreshKey }: Props) {
    const [holds, setHolds] = useState<MyHold[] | null>(null);
    const [error, setError] = useState<string | null>(null);

    const load = useCallback(async () => {
        try {
            setHolds(await getMyHolds(studentId));
            setError(null);
        } catch (e) {
            setError((e as Error).message);
        }
    }, [studentId]);

    useEffect(() => {
        load();
        const timer = setInterval(load, 5000);
        return () => clearInterval(timer);
    }, [load, refreshKey]);

    if (holds === null && !error) return <p>Loading your applications...</p>;

    return (
        <section>
            <h2>My applications</h2>
            {error && <p role="alert">{error}</p>}

            {holds && holds.length === 0 && <p>You have no applications yet.</p>}

            {holds && holds.length > 0 && (
                <table>
                    <thead>
                        <tr>
                            <th>Class</th>
                            <th>Status</th>
                            <th>Deadline</th>
                        </tr>
                    </thead>
                    <tbody>
                        {holds.map((h) => (
                            <tr key={h.holdId}>
                                <td>{h.className}</td>
                                <td>{h.status}</td>
                                <td>{new Date(h.expiresAt).toLocaleString()}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </section>
    );
}