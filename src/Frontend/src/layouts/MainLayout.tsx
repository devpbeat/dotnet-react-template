import React from 'react';
import { useAuth } from '../context/AuthContext';
import { Outlet, Navigate } from 'react-router-dom';

const MainLayout: React.FC = () => {
    const { user, logout, isLoading } = useAuth();

    if (isLoading) {
        return <div>Loading...</div>;
    }

    if (!user) {
        return <Navigate to="/login" />;
    }

    return (
        <div style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
            <header style={{ padding: '10px 20px', background: '#f0f0f0', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <h1>My SaaS App</h1>
                <div>
                    <span style={{ marginRight: '10px' }}>Welcome, {user.email} ({user.role})</span>
                    <button onClick={logout}>Logout</button>
                </div>
            </header>
            <main style={{ flex: 1, padding: '20px' }}>
                <Outlet />
            </main>
            <footer style={{ padding: '10px 20px', background: '#f0f0f0', textAlign: 'center' }}>
                &copy; 2025 My SaaS App
            </footer>
        </div>
    );
};

export default MainLayout;
