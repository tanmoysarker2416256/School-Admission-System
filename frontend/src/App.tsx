import { useState } from 'react';
import ClassList from './features/classes/ClassList';
import StudentIdBox, { isValidStudentId } from './features/student/StudentIdBox';

const STORAGE_KEY = 'studentId';

export default function App() {
    const [studentId, setStudentId] = useState<string | null>(() => {
        const saved = localStorage.getItem(STORAGE_KEY);
        return saved && isValidStudentId(saved) ? saved : null;
    });

    function handleStudentId(id: string | null) {
        setStudentId(id);
        if (id) localStorage.setItem(STORAGE_KEY, id);
        else localStorage.removeItem(STORAGE_KEY);
    }

    return (
        <main>
            <h1>School Admission</h1>
            <StudentIdBox studentId={studentId} onChange={handleStudentId} />
            <ClassList studentId={studentId} />
        </main>
    );
}