import React, { createContext, useContext, useState, useEffect } from 'react';
import ApiService from '../services/ApiService';

const TimerContext = createContext();

export function TimerProvider({ children }) {
  const [activeTimer, setActiveTimer] = useState(() => {
    const saved = localStorage.getItem('devos_active_timer');
    return saved ? JSON.parse(saved) : null;
  });

  const [elapsedSeconds, setElapsedSeconds] = useState(0);
  const [isPaused, setIsPaused] = useState(false);
  const [isMinimized, setIsMinimized] = useState(false);

  useEffect(() => {
    if (activeTimer) {
      localStorage.setItem('devos_active_timer', JSON.stringify(activeTimer));
    } else {
      localStorage.removeItem('devos_active_timer');
    }
  }, [activeTimer]);

  useEffect(() => {
    let interval = null;
    if (activeTimer && !isPaused) {
      interval = setInterval(() => {
        const now = new Date().getTime();
        const start = new Date(activeTimer.startTime).getTime();
        const pausedOffset = activeTimer.pausedOffset || 0;
        setElapsedSeconds(Math.floor((now - start - pausedOffset) / 1000));
      }, 1000);
    }
    return () => clearInterval(interval);
  }, [activeTimer, isPaused]);

  // startTimer теперь принимает entryId, если нужно перезаписать/продолжить существующую сессию
  const startTimer = (description = 'Dev Session', projectId, taskId = null, entryId = null, existingStartedAt = null) => {
    if (!projectId) return;
    const newTimer = {
      startTime: new Date().toISOString(),
      description,
      projectId,
      taskId,
      entryId, // Если передано, таймер обновит существующую запись при Stop
      existingStartedAt, // Исходное время начала сессии
      pausedOffset: 0,
    };
    setActiveTimer(newTimer);
    setIsPaused(false);
    setElapsedSeconds(0);
  };

  const togglePause = () => {
    if (!activeTimer) return;
    if (!isPaused) {
      setActiveTimer((prev) => ({
        ...prev,
        pauseStartedAt: new Date().toISOString(),
      }));
      setIsPaused(true);
    } else {
      const pauseDuration = new Date().getTime() - new Date(activeTimer.pauseStartedAt).getTime();
      setActiveTimer((prev) => ({
        ...prev,
        pausedOffset: (prev.pausedOffset || 0) + pauseDuration,
        pauseStartedAt: null,
      }));
      setIsPaused(false);
    }
  };

  const stopAndSaveTimer = async () => {
    if (!activeTimer) return;
    const endedAt = new Date();
    try {
      if (activeTimer.entryId) {
        // Обновляем существующую запись вместо создания новой
        const startedAt = activeTimer.existingStartedAt || activeTimer.startTime;
        await ApiService.updateTimeEntry(activeTimer.projectId, activeTimer.entryId, {
          description: activeTimer.description,
          taskId: activeTimer.taskId || null,
          startedAt,
          endedAt: endedAt.toISOString(),
        });
      } else {
        // Создаем новую запись, если запускали не из карточки
        await ApiService.createTimeEntry(activeTimer.projectId, {
          description: activeTimer.description,
          taskId: activeTimer.taskId || null,
          startedAt: activeTimer.startTime,
          endedAt: endedAt.toISOString(),
        });
      }
      window.dispatchEvent(new Event('refresh_entries'));
    } catch (e) {
      console.error('Failed to save time entry:', e);
    } finally {
      setActiveTimer(null);
      setElapsedSeconds(0);
      setIsPaused(false);
    }
  };

  return (
    <TimerContext.Provider
      value={{
        activeTimer,
        elapsedSeconds,
        isPaused,
        isMinimized,
        setIsMinimized,
        startTimer,
        togglePause,
        stopAndSaveTimer,
      }}
    >
      {children}
    </TimerContext.Provider>
  );
}

export const useTimer = () => useContext(TimerContext);