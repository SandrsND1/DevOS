import React, { useState, useEffect, useRef } from 'react';
import { Play, Pause, Square, Minimize2, Maximize2, ExternalLink, Move } from 'lucide-react';
import { useTimer } from '../context/TimerContext';

export default function TimerWidget() {
  const {
    activeTimer,
    elapsedSeconds,
    isPaused,
    isMinimized,
    setIsMinimized,
    togglePause,
    stopAndSaveTimer,
  } = useTimer();

  const [position, setPosition] = useState({ 
    x: typeof window !== 'undefined' ? window.innerWidth - 340 : 20, 
    y: typeof window !== 'undefined' ? window.innerHeight - 240 : 20 
  });
  const [isDragging, setIsDragging] = useState(false);
  const dragRef = useRef({ startX: 0, startY: 0, initialX: 0, initialY: 0 });
  const pipWindowRef = useRef(null);

  const formatTime = (totalSec) => {
    const hrs = Math.floor(totalSec / 3600);
    const mins = Math.floor((totalSec % 3600) / 60);
    const secs = totalSec % 60;
    return `${hrs > 0 ? hrs + ':' : ''}${mins < 10 ? '0' : ''}${mins}:${secs < 10 ? '0' : ''}${secs}`;
  };

  // Drag logic
  const handleMouseDown = (e) => {
    setIsDragging(true);
    dragRef.current = {
      startX: e.clientX,
      startY: e.clientY,
      initialX: position.x,
      initialY: position.y,
    };
  };

  useEffect(() => {
    const handleMouseMove = (e) => {
      if (!isDragging) return;
      const dx = e.clientX - dragRef.current.startX;
      const dy = e.clientY - dragRef.current.startY;
      setPosition({
        x: Math.max(10, Math.min(window.innerWidth - 330, dragRef.current.initialX + dx)),
        y: Math.max(10, Math.min(window.innerHeight - 200, dragRef.current.initialY + dy)),
      });
    };

    const handleMouseUp = () => setIsDragging(false);

    if (isDragging) {
      window.addEventListener('mousemove', handleMouseMove);
      window.addEventListener('mouseup', handleMouseUp);
    }
    return () => {
      window.removeEventListener('mousemove', handleMouseMove);
      window.removeEventListener('mouseup', handleMouseUp);
    };
  }, [isDragging]);

  // Document Picture-in-Picture (Нативное плавающее окно без видеоплеера)
  const toggleDocumentPiP = async () => {
    if ('documentPictureInPicture' in window) {
      try {
        if (pipWindowRef.current) {
          pipWindowRef.current.close();
          pipWindowRef.current = null;
          return;
        }

        const pipWindow = await window.documentPictureInPicture.requestWindow({
          width: 280,
          height: 160,
        });

        pipWindowRef.current = pipWindow;

        // Копируем стили из основного окна
        [...document.styleSheets].forEach((styleSheet) => {
          try {
            const cssRules = [...styleSheet.cssRules].map((rule) => rule.cssText).join('');
            const style = document.createElement('style');
            style.textContent = cssRules;
            pipWindow.document.head.appendChild(style);
          } catch (e) {
            const link = document.createElement('link');
            link.rel = 'stylesheet';
            link.href = styleSheet.href;
            pipWindow.document.head.appendChild(link);
          }
        });

        pipWindow.addEventListener('pagehide', () => {
          pipWindowRef.current = null;
        });
      } catch (err) {
        console.error('Failed to open Document PiP:', err);
      }
    } else {
      alert('Document Picture-in-Picture поддерживается в Chrome / Edge v116+');
    }
  };

  // Обновление DOM в нативном плавающем окне
  useEffect(() => {
    if (pipWindowRef.current && activeTimer) {
      const doc = pipWindowRef.current.document;
      doc.body.className = 'bg-slate-900 text-white font-sans p-4 m-0 flex flex-col justify-between h-full select-none';
      doc.body.innerHTML = `
        <div style="display:flex; justify-content:space-between; align-items:center;">
          <span style="font-size:12px; font-weight:bold; color:${isPaused ? '#f59e0b' : '#10b981'}; text-transform:uppercase;">
            ${isPaused ? 'Paused' : 'Live Session'}
          </span>
          <span style="font-size:12px; color:#94a3b8; max-width:140px; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;">
            ${activeTimer.description || 'Dev Session'}
          </span>
        </div>
        <div style="text-align:center; font-family:monospace; font-size:32px; font-weight:bold; margin:10px 0;">
          ${formatTime(elapsedSeconds)}
        </div>
        <div style="display:flex; gap:8px;">
          <button id="pip-toggle" style="flex:1; padding:6px; background:${isPaused ? '#4f46e5' : '#1e293b'}; border:none; border-radius:8px; color:white; font-size:12px; cursor:pointer;">
            ${isPaused ? 'Resume' : 'Pause'}
          </button>
          <button id="pip-stop" style="flex:1; padding:6px; background:rgba(239,68,68,0.2); border:1px solid rgba(239,68,68,0.4); border-radius:8px; color:#fca5a5; font-size:12px; cursor:pointer;">
            Stop
          </button>
        </div>
      `;

      doc.getElementById('pip-toggle')?.addEventListener('click', togglePause);
      doc.getElementById('pip-stop')?.addEventListener('click', () => {
        stopAndSaveTimer();
        pipWindowRef.current?.close();
      });
    }
  }, [elapsedSeconds, isPaused, activeTimer]);

  // Слушатель автообновления списка сессий при сохранении
  useEffect(() => {
    const handleUpdate = () => {
      if (typeof window !== 'undefined') {
        window.dispatchEvent(new CustomEvent('refresh_entries'));
      }
    };
    window.addEventListener('devos_timer_updated', handleUpdate);
    return () => window.removeEventListener('devos_timer_updated', handleUpdate);
  }, []);

  if (!activeTimer) return null;

  return (
    <div
      style={{ left: `${position.x}px`, top: `${position.y}px` }}
      className="fixed z-50 transition-shadow duration-200 select-none"
    >
      {isMinimized ? (
        <div className="flex items-center gap-3 bg-slate-900/95 border border-indigo-500/50 backdrop-blur-xl px-4 py-2.5 rounded-full shadow-2xl text-white">
          <div onMouseDown={handleMouseDown} className="cursor-move p-1 text-gray-400 hover:text-white">
            <Move className="w-3.5 h-3.5" />
          </div>
          <span className={`w-2.5 h-2.5 rounded-full ${isPaused ? 'bg-amber-400' : 'bg-emerald-400 animate-pulse'}`} />
          <span className="font-mono text-sm font-bold">{formatTime(elapsedSeconds)}</span>
          <button onClick={togglePause} className="p-1 hover:text-indigo-400 transition-colors">
            {isPaused ? <Play className="w-4 h-4 fill-current" /> : <Pause className="w-4 h-4 fill-current" />}
          </button>
          <button onClick={stopAndSaveTimer} className="p-1 hover:text-red-400 transition-colors">
            <Square className="w-4 h-4 fill-current" />
          </button>
          <button onClick={() => setIsMinimized(false)} className="p-1 text-gray-400 hover:text-white transition-colors">
            <Maximize2 className="w-4 h-4" />
          </button>
        </div>
      ) : (
        <div className="w-80 bg-slate-900/95 border border-indigo-500/40 backdrop-blur-xl p-4 rounded-2xl shadow-2xl text-white space-y-3">
          <div
            onMouseDown={handleMouseDown}
            className="flex items-center justify-between border-b border-slate-800 pb-2 cursor-move"
          >
            <div className="flex items-center gap-2">
              <Move className="w-3.5 h-3.5 text-gray-500" />
              <span className={`w-2.5 h-2.5 rounded-full ${isPaused ? 'bg-amber-400' : 'bg-emerald-400 animate-pulse'}`} />
              <span className="text-xs font-semibold uppercase text-indigo-400 tracking-wider">
                {isPaused ? 'Paused' : 'Live Session'}
              </span>
            </div>
            <div className="flex items-center gap-1">
              <button 
                onClick={toggleDocumentPiP} 
                title="Pop out window on top of OS" 
                className="text-gray-400 hover:text-indigo-400 p-1 transition-colors"
              >
                <ExternalLink className="w-3.5 h-3.5" />
              </button>
              <button onClick={() => setIsMinimized(true)} className="text-gray-400 hover:text-white p-1 transition-colors">
                <Minimize2 className="w-3.5 h-3.5" />
              </button>
            </div>
          </div>

          <div>
            <h4 className="font-semibold text-sm truncate">{activeTimer.description || 'Dev Session'}</h4>
          </div>

          <div className="text-center py-1">
            <span className="font-mono text-3xl font-bold tracking-tight">{formatTime(elapsedSeconds)}</span>
          </div>

          <div className="flex items-center gap-2 pt-1">
            <button
              onClick={togglePause}
              className={`flex-1 py-2 rounded-xl text-xs font-semibold flex items-center justify-center gap-1.5 transition-colors ${
                isPaused ? 'bg-indigo-600 hover:bg-indigo-500' : 'bg-slate-800 hover:bg-slate-700'
              }`}
            >
              {isPaused ? <Play className="w-3.5 h-3.5 fill-current" /> : <Pause className="w-3.5 h-3.5 fill-current" />}
              {isPaused ? 'Resume' : 'Pause'}
            </button>
            <button
              onClick={stopAndSaveTimer}
              className="flex-1 py-2 bg-red-500/20 hover:bg-red-500/30 text-red-300 border border-red-500/30 rounded-xl text-xs font-semibold flex items-center justify-center gap-1.5 transition-colors"
            >
              <Square className="w-3.5 h-3.5 fill-current" /> Stop
            </button>
          </div>
        </div>
      )}
    </div>
  );
}