import { useState } from 'react';
import ApplicationPanel from './features/application/ApplicationPanel';
import MyApplications from './features/application/MyApplications';
import ClassList from './features/classes/ClassList';
import StudentIdBox, { isValidStudentId } from './features/student/StudentIdBox';

const STORAGE_KEY = 'studentId';

type View = 'classes' | 'applications';

export default function App() {
    const [studentId, setStudentId] = useState<string | null>(() => {
        const saved = localStorage.getItem(STORAGE_KEY);
        return saved && isValidStudentId(saved) ? saved : null;
    });
    const [refreshKey, setRefreshKey] = useState(0);
    const [view, setView] = useState<View>('classes');
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

            <nav>
                <button disabled={view === 'classes'} onClick={() => setView('classes')}>
                    Classes
                </button>{' '}
                <button disabled={view === 'applications'} onClick={() => setView('applications')}>
                    My applications
                </button>
            </nav>

            {view === 'classes' && (
                <>
                    {studentId && (
                        <ApplicationPanel key={studentId} studentId={studentId} refreshKey={refreshKey} onChanged={refresh} />
                    )}
                    <ClassList studentId={studentId} refreshKey={refreshKey} onChanged={refresh} />
                </>
            )}

            {view === 'applications' &&
                (studentId ? (
                    <MyApplications key={studentId} studentId={studentId} refreshKey={refreshKey} />
                ) : (
                    <p>Enter your 17-digit student ID above to see your applications.</p>
                ))}
        </main>
    );
}