import { useState } from 'react';

export const isValidStudentId = (value: string) => /^\d{17}$/.test(value);

interface Props {
    studentId: string | null;
    onChange: (id: string | null) => void;
}

export default function StudentIdBox({ studentId, onChange }: Props) {
    const [draft, setDraft] = useState('');
    const [error, setError] = useState<string | null>(null);

    if (studentId) {
        return (
            <p>
                Student ID: <strong>{studentId}</strong>{' '}
                <button onClick={() => onChange(null)}>Change</button>
            </p>
        );
    }

    function save() {
        if (!isValidStudentId(draft)) {
            setError('Student ID must be exactly 17 digits.');
            return;
        }
        setError(null);
        onChange(draft);
    }

    return (
        <div>
            <label>
                Student ID (17 digits){' '}
                <input
                    value={draft}
                    inputMode="numeric"
                    maxLength={17}
                    onChange={(e) => setDraft(e.target.value.replace(/\D/g, ''))}
                />
            </label>{' '}
            <button onClick={save}>Continue</button>
            {error && <p role="alert">{error}</p>}
        </div>
    );
}