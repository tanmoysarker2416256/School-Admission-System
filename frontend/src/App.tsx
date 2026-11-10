import { useState } from 'react';
import ApplicationPanel from './features/application/ApplicationPanel';
import ClassList from './features/classes/ClassList';
import StudentIdBox, { isValidStudentId } from './features/student/StudentIdBox';

const STORAGE_KEY = 'studentId';

export default function App() {
    const [studentId, setStudentId] = useState<string | null>(() => {
        const saved = localStorage.getItem(STORAGE_KEY);
        return saved && isValidStudentId(saved) ? saved : null;
    });
    const [refreshKey, setRefreshKey] = useState(0);
    const refresh = () => setRefreshKey((k) => k + 1);

    function handleStudentId(id: string | null) {
        setStudentId(id);
        if (id) localStorage.setItem(STORAGE_KEY, id);
        else localStorage.removeItem(STORAGE_KEY);
    }

    return (
        <main>
            <h1>School Admission</h1>
            <StudentIdBox studentId={studentId} onChange={handleStudentId} />
            {studentId && (
                <ApplicationPanel key={studentId} studentId={studentId} refreshKey={refreshKey} onChanged={refresh} />
            )}
            <ClassList studentId={studentId} refreshKey={refreshKey} onChanged={refresh} />
        </main>
    );
}