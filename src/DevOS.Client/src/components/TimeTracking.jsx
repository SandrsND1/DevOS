import React, { useState, useEffect, useMemo } from 'react';
import { Clock, Timer, Calendar, Plus, X, Target, Play, Trash2, Edit2 } from 'lucide-react';
import ApiService from '../services/ApiService';
import { useTimer } from '../context/TimerContext';

export default function TimeTracking() {
  const [entries, setEntries] = useState([]);
  const [projects, setProjects] = useState([]);
  const [selectedProjectId, setSelectedProjectId] = useState(null);
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);

  const [showModal, setShowModal] = useState(false);
  const [editingEntry, setEditingEntry] = useState(null);

  const { startTimer } = useTimer();

  const [formData, setFormData] = useState({ 
    description: '', 
    taskId: '', 
    minutes: '30',
    date: new Date().toISOString().split('T')[0]
  });

  useEffect(() => {
    loadProjects();
  }, []);

  useEffect(() => {
    if (selectedProjectId) {
      loadData(selectedProjectId);
    }
  }, [selectedProjectId]);

  useEffect(() => {
    const handleRefresh = () => {
      if (selectedProjectId) loadData(selectedProjectId);
    };
    window.addEventListener('refresh_entries', handleRefresh);
    return () => window.removeEventListener('refresh_entries', handleRefresh);
  }, [selectedProjectId]);

  const loadProjects = async () => {
    try {
      const data = await ApiService.getProjects();
      setProjects(data);
      if (data.length > 0) setSelectedProjectId(data[0].id);
    } catch (e) {
      console.error('Failed to load projects:', e);
    } finally {
      setLoading(false);
    }
  };

  const loadData = async (projectId) => {
    setLoading(true);
    try {
      const [entriesData, tasksData] = await Promise.all([
        ApiService.getTimeEntries(projectId),
        ApiService.getTasks(projectId),
      ]);
      setEntries(entriesData || []);
      setTasks(tasksData.items || tasksData || []);
    } catch (e) {
      console.error('Failed to load time tracking data:', e);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenCreateModal = () => {
    setEditingEntry(null);
    setFormData({
      description: '',
      taskId: '',
      minutes: '30',
      date: new Date().toISOString().split('T')[0]
    });
    setShowModal(true);
  };

  const handleOpenEditModal = (entry) => {
    setEditingEntry(entry);
    const start = new Date(entry.startedAt);
    const end = new Date(entry.endedAt);
    const diffMins = !isNaN(start) && !isNaN(end) ? Math.max(1, Math.round((end - start) / 60000)) : 30;

    setFormData({
      description: entry.description || '',
      taskId: entry.taskId || '',
      minutes: String(diffMins),
      date: !isNaN(start) ? start.toISOString().split('T')[0] : new Date().toISOString().split('T')[0]
    });
    setShowModal(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!selectedProjectId) return;
    
    try {
      const startDate = new Date(`${formData.date}T09:00:00`);
      const mins = parseInt(formData.minutes || '30', 10);
      const endedAt = new Date(startDate.getTime() + mins * 60000);

      const payload = {
        description: formData.description || 'Dev Session',
        taskId: formData.taskId || null,
        startedAt: startDate.toISOString(),
        endedAt: endedAt.toISOString()
      };

      if (editingEntry) {
        await ApiService.updateTimeEntry(selectedProjectId, editingEntry.id, payload);
      } else {
        await ApiService.createTimeEntry(selectedProjectId, payload);
      }

      setShowModal(false);
      loadData(selectedProjectId);
    } catch (e) {
      console.error('Failed to save time entry:', e);
    }
  };

  const handleDelete = async (entryId) => {
    if (!selectedProjectId || !window.confirm('Delete this time entry?')) return;
    try {
      await ApiService.deleteTimeEntry(selectedProjectId, entryId);
      loadData(selectedProjectId);
    } catch (e) {
      console.error('Failed to delete entry:', e);
    }
  };

  const calculateDuration = (startedAt, endedAt) => {
    if (!startedAt || !endedAt) return '0s';
    const start = new Date(startedAt);
    const end = new Date(endedAt);
    if (isNaN(start.getTime()) || isNaN(end.getTime())) return '0s';

    const diffSecs = Math.round((end - start) / 1000);
    if (diffSecs < 60) {
      return `${Math.max(1, diffSecs)}s`;
    }
    const diffMins = Math.floor(diffSecs / 60);
    if (diffMins >= 60) {
      return `${Math.floor(diffMins / 60)}h ${diffMins % 60}m`;
    }
    return `${diffMins}m`;
  };

  const stats = useMemo(() => {
    const totalMinutes = entries.reduce((acc, entry) => {
      const start = new Date(entry.startedAt);
      const end = new Date(entry.endedAt);
      if (!isNaN(start.getTime()) && !isNaN(end.getTime())) {
        return acc + Math.round((end - start) / (1000 * 60));
      }
      return acc;
    }, 0);

    return {
      totalHours: (totalMinutes / 60).toFixed(1),
      totalMinutes,
      averageSession: entries.length > 0 ? Math.round(totalMinutes / entries.length) : 0
    };
  }, [entries]);

  const groupedEntries = useMemo(() => {
    const groups = {};
    entries.forEach(entry => {
      const date = new Date(entry.startedAt).toLocaleDateString('en-US', {
        month: 'long',
        day: 'numeric',
        year: 'numeric'
      });
      if (!groups[date]) groups[date] = [];
      groups[date].push(entry);
    });
    return groups;
  }, [entries]);

  return (
    <div className="p-8 max-w-7xl mx-auto space-y-6">
      {/* Header */}
      <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-white tracking-tight flex items-center gap-3">
            <Clock className="w-8 h-8 text-indigo-400" /> Time Tracking
          </h1>
          <p className="text-gray-400 mt-1">Log and monitor developer productivity hours</p>
        </div>
        
        <div className="flex items-center gap-3">
          {projects.length > 0 && (
            <select
              value={selectedProjectId || ''}
              onChange={(e) => setSelectedProjectId(e.target.value)}
              className="px-4 py-2.5 bg-slate-800 border border-slate-700 rounded-xl text-white min-w-[200px]"
            >
              {projects.map((p) => (
                <option key={p.id} value={p.id} className="bg-slate-900 text-white">
                  {p.name}
                </option>
              ))}
            </select>
          )}

          <button
            onClick={handleOpenCreateModal}
            disabled={!selectedProjectId}
            className="px-5 py-2.5 bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-500 hover:to-purple-500 text-white font-medium rounded-xl shadow-lg shadow-indigo-500/25 transition-all flex items-center gap-2 disabled:opacity-50"
          >
            <Plus className="w-4 h-4" /> Log Time
          </button>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div className="bg-gradient-to-br from-indigo-600 to-purple-600 rounded-2xl p-6 text-white shadow-lg shadow-indigo-500/25">
          <div className="flex items-center justify-between mb-2">
            <Timer className="w-6 h-6 opacity-80" />
            <span className="text-xs font-semibold uppercase tracking-wider opacity-80">Total Time</span>
          </div>
          <p className="text-3xl font-bold">{stats.totalHours}h</p>
          <p className="text-sm opacity-80 mt-1">{stats.totalMinutes} minutes total</p>
        </div>

        <div className="bg-slate-800/50 border border-slate-700 rounded-2xl p-6 text-white">
          <div className="flex items-center justify-between mb-2">
            <Target className="w-6 h-6 text-purple-400" />
            <span className="text-xs font-semibold uppercase tracking-wider text-gray-400">Avg Session</span>
          </div>
          <p className="text-3xl font-bold">{stats.averageSession}m</p>
          <p className="text-sm text-gray-400 mt-1">{entries.length} sessions total</p>
        </div>
      </div>

      {/* Entries List */}
      {loading ? (
        <div className="flex items-center justify-center py-20">
          <div className="inline-block w-12 h-12 border-4 border-indigo-500 border-t-transparent rounded-full animate-spin mb-4" />
        </div>
      ) : entries.length === 0 ? (
        <div className="bg-slate-800/50 border border-slate-700 rounded-2xl p-12 text-center">
          <Clock className="w-10 h-10 text-indigo-400 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-white mb-2">No time entries yet</h3>
          <p className="text-gray-400 mb-6">Start logging your development time to see insights here.</p>
          <button
            onClick={handleOpenCreateModal}
            className="inline-flex items-center gap-2 px-5 py-2.5 bg-indigo-600 hover:bg-indigo-500 text-white font-medium rounded-xl transition-colors"
          >
            <Plus className="w-4 h-4" /> Log Your First Entry
          </button>
        </div>
      ) : (
        <div className="space-y-6">
          {Object.entries(groupedEntries).map(([date, dateEntries]) => (
            <div key={date}>
              <div className="flex items-center gap-3 mb-3">
                <Calendar className="w-4 h-4 text-gray-500" />
                <h3 className="text-sm font-semibold text-gray-400">{date}</h3>
                <div className="flex-1 h-px bg-slate-700" />
                <span className="text-xs text-gray-500">
                  {dateEntries.length} {dateEntries.length === 1 ? 'entry' : 'entries'}
                </span>
              </div>
              
              <div className="grid gap-3">
                {dateEntries.map((entry) => (
                  <div
                    key={entry.id || Math.random()}
                    className="bg-slate-800/50 border border-slate-700 rounded-xl p-5 flex justify-between items-center hover:border-indigo-500/50 transition-all group"
                  >
                    <div className="flex items-center space-x-4">
                      {/* Нажатие Play передает id карточки, чтобы ОБНОВИТЬ её при остановке */}
                      <button
                        onClick={() => startTimer(
                          entry.description || 'Dev Session', 
                          selectedProjectId, 
                          entry.taskId,
                          entry.id,
                          entry.startedAt
                        )}
                        className="p-3 bg-indigo-500/10 text-indigo-400 hover:bg-indigo-600 hover:text-white rounded-xl transition-colors cursor-pointer"
                        title="Resume & update this time entry"
                      >
                        <Play className="w-5 h-5 fill-current" />
                      </button>

                      <div>
                        <h3 className="font-semibold text-white">{entry.description || 'Work Session'}</h3>
                        <p className="text-xs text-gray-400 mt-1">
                          {entry.startedAt ? new Date(entry.startedAt).toLocaleTimeString([], {
                            hour: '2-digit',
                            minute: '2-digit'
                          }) : ''}
                        </p>
                      </div>
                    </div>

                    <div className="flex items-center gap-3">
                      <span className="px-3 py-1 bg-indigo-500/20 text-indigo-300 font-semibold rounded-lg text-sm border border-indigo-500/30">
                        {calculateDuration(entry.startedAt, entry.endedAt)}
                      </span>

                      <button
                        onClick={() => handleOpenEditModal(entry)}
                        className="p-2 text-gray-400 hover:text-indigo-400 transition-colors"
                        title="Edit Entry"
                      >
                        <Edit2 className="w-4 h-4" />
                      </button>

                      <button
                        onClick={() => handleDelete(entry.id)}
                        className="p-2 text-gray-400 hover:text-red-400 transition-colors"
                        title="Delete Entry"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Modal for Create/Edit */}
      {showModal && (
        <div className="fixed inset-0 bg-black/70 backdrop-blur-sm flex items-center justify-center p-4 z-50">
          <div className="bg-slate-900 border border-slate-700 rounded-2xl p-6 w-full max-w-md shadow-2xl space-y-4">
            <div className="flex items-center justify-between">
              <h2 className="text-xl font-bold text-white flex items-center gap-2">
                <Clock className="w-5 h-5 text-indigo-400" />
                {editingEntry ? 'Edit Entry' : 'Log Time Entry'}
              </h2>
              <button onClick={() => setShowModal(false)} className="text-gray-400 hover:text-white">
                <X className="w-5 h-5" />
              </button>
            </div>
            
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-300 mb-2">Description</label>
                <input
                  type="text"
                  required
                  placeholder="What were you working on?"
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  className="w-full px-4 py-2.5 bg-slate-800 border border-slate-700 rounded-xl text-white focus:outline-none focus:border-indigo-500"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">Date</label>
                  <input
                    type="date"
                    required
                    value={formData.date}
                    onChange={(e) => setFormData({ ...formData, date: e.target.value })}
                    className="w-full px-4 py-2.5 bg-slate-800 border border-slate-700 rounded-xl text-white"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">Duration (Min)</label>
                  <input
                    type="number"
                    required
                    min="1"
                    max="480"
                    value={formData.minutes}
                    onChange={(e) => setFormData({ ...formData, minutes: e.target.value })}
                    className="w-full px-4 py-2.5 bg-slate-800 border border-slate-700 rounded-xl text-white"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-300 mb-2">Link to Task</label>
                <select
                  value={formData.taskId}
                  onChange={(e) => setFormData({ ...formData, taskId: e.target.value })}
                  className="w-full px-4 py-2.5 bg-slate-800 border border-slate-700 rounded-xl text-white"
                >
                  <option value="" className="bg-slate-900 text-white">No Task Linked</option>
                  {tasks.map((t) => (
                    <option key={t.id} value={t.id} className="bg-slate-900 text-white">{t.title}</option>
                  ))}
                </select>
              </div>

              <div className="flex justify-end space-x-3 pt-2">
                <button
                  type="button"
                  onClick={() => setShowModal(false)}
                  className="px-4 py-2.5 border border-slate-700 text-gray-300 rounded-xl hover:bg-slate-800"
                >
                  Cancel
                </button>
                <button 
                  type="submit" 
                  className="px-5 py-2.5 bg-indigo-600 text-white rounded-xl hover:bg-indigo-500 shadow-lg shadow-indigo-500/25"
                >
                  {editingEntry ? 'Update Entry' : 'Save Entry'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}