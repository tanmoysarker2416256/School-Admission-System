import { useCallback, useEffect, useState } from 'react';
import { cancelHold, confirmHold, getMyHolds } from '../../api/holds';
import type { MyHold } from '../../api/types';

interface Props {
    studentId: string;
    refreshKey: number;
    onChanged: () => void;
}

function formatRemaining(ms: number) {
    const total = Math.max(0, Math.ceil(ms / 1000));
    const minutes = Math.floor(total / 60);
    const seconds = total % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
}

export default function ApplicationPanel({ studentId, refreshKey, onChanged }: Props) {
    const [current, setCurrent] = useState<MyHold | null>(null);
    const [now, setNow] = useState(Date.now());
    const [busy, setBusy] = useState(false);
    const [message, setMessage] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const load = useCallback(async () => {
        try {
            const holds = await getMyHolds(studentId);   // newest first
            setCurrent(holds[0] ?? null);
            setError(null);
        } catch (e) {
            setError((e as Error).message);
        }
    }, [studentId]);

    // load on start, whenever something changed, and every 5 seconds
    useEffect(() => {
        load();
        const timer = setInterval(load, 5000);
        return () => clearInterval(timer);
    }, [load, refreshKey]);

    // the countdown ticks once a second
    useEffect(() => {
        const timer = setInterval(() => setNow(Date.now()), 1000);
        return () => clearInterval(timer);
    }, []);

    const remainingMs = current ? new Date(current.expiresAt).getTime() - now : 0;
    const expiredLocally = current?.status === 'HELD' && remainingMs <= 0;

    // when the countdown reaches zero, ask the server for the real status
    useEffect(() => {
        if (expiredLocally) load();
    }, [expiredLocally, load]);

    async function pay() {
        if (!current) return;
        setBusy(true);
        setError(null);
        setMessage(null);
        try {
            await confirmHold(current.holdId, studentId);
            setMessage('Admission fee paid. Your seat is confirmed.');
        } catch (e) {
            setError((e as Error).message);
        } finally {
            setBusy(false);
            onChanged();
        }
    }

    async function cancel() {
        if (!current) return;
        setBusy(true);
        setError(null);
        setMessage(null);
        try {
            await cancelHold(current.holdId, studentId);
            setMessage('Your application was cancelled.');
        } catch (e) {
            setError((e as Error).message);
        } finally {
            setBusy(false);
            onChanged();
        }
    }

    const status = expiredLocally ? 'EXPIRED' : current?.status;

    return (
        <section>
            <h2>Your application</h2>
            {error && <p role="alert">{error}</p>}
            {message && <p role="status">{message}</p>}

            {!current && <p>You have not applied yet. Choose a class below.</p>}

            {current && (
                <div>
                    <p>
                        Class: <strong>{current.className}</strong>
                    </p>
                    <p>
                        Status: <strong>{status}</strong>
                    </p>

                    {status === 'HELD' && <p>Time left to pay: {formatRemaining(remainingMs)}</p>}
                    {status === 'EXPIRED' && <p>This application has expired. Please apply again.</p>}
                    {status === 'CONFIRMED' && <p>Your seat is confirmed.</p>}
                    {status === 'CANCELLED' && <p>This application was cancelled. You can apply again.</p>}

                    {(status === 'HELD' || status === 'EXPIRED') && (
                        <p>
                            <button disabled={busy || status !== 'HELD'} onClick={pay}>
                                Pay fee
                            </button>{' '}
                            {status === 'HELD' && (
                                <button disabled={busy} onClick={cancel}>
                                    Cancel application
                                </button>
                            )}
                        </p>
                    )}
                </div>
            )}
        </section>
    );
}