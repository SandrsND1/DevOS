import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Auth from './components/Auth';
import Dashboard from './components/Dashboard';
import Projects from './components/Projects';
import Tasks from './components/Tasks';
import TimeTracking from './components/TimeTracking';
import Navbar from './components/Navbar';
import ApiService from './services/ApiService';

// Импорты глобального таймера и виджета
import { TimerProvider } from './context/TimerContext';
import TimerWidget from './components/TimerWidget';

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem('devos_token');
    if (token) {
      ApiService.setToken(token);
      setIsAuthenticated(true);
    }
    setLoading(false);
  }, []);

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-devos-dark">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-devos-primary"></div>
      </div>
    );
  }

  return (
    <TimerProvider>
      <Router>
        {isAuthenticated ? (
          <>
            <Navbar setIsAuthenticated={setIsAuthenticated} />
            <div className="min-h-screen bg-devos-dark">
              <Routes>
                <Route path="/" element={<Dashboard />} />
                <Route path="/projects" element={<Projects />} />
                <Route path="/tasks" element={<Tasks />} />
                <Route path="/time-tracking" element={<TimeTracking />} />
                <Route path="*" element={<Navigate to="/" />} />
              </Routes>
            </div>
            {/* Виджет плавающего таймера, доступен на всех страницах */}
            <TimerWidget />
          </>
        ) : (
          <Routes>
            <Route path="*" element={<Auth setIsAuthenticated={setIsAuthenticated} />} />
          </Routes>
        )}
      </Router>
    </TimerProvider>
  );
}

export default App;