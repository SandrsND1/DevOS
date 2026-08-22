import React, { useEffect, useMemo, useState } from 'react';
import {
  AlertCircle,
  Calendar,
  Check,
  CheckCircle2,
  CheckSquare,
  ChevronDown,
  Circle,
  Clock3,
  FolderKanban,
  MoreHorizontal,
  Pencil,
  Plus,
  Search,
  Square,
  Trash2,
  X,
  Zap,
} from 'lucide-react';
import ApiService from '../services/ApiService';
import { TaskPriority, TaskStatus } from '../services/apiContract';

const STATUS_CONFIG = {
  [TaskStatus.TODO]: {
    label: 'Todo',
    icon: Circle,
    className: 'text-gray-400 bg-gray-500/10 border-gray-500/20',
  },
  [TaskStatus.IN_PROGRESS]: {
    label: 'In Progress',
    icon: Clock3,
    className: 'text-blue-400 bg-blue-500/10 border-blue-500/20',
  },
  [TaskStatus.BLOCKED]: {
    label: 'Blocked',
    icon: AlertCircle,
    className: 'text-red-400 bg-red-500/10 border-red-500/20',
  },
  [TaskStatus.COMPLETED]: {
    label: 'Completed',
    icon: CheckCircle2,
    className: 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20',
  },
  [TaskStatus.CANCELLED]: {
    label: 'Cancelled',
    icon: Square,
    className: 'text-gray-500 bg-gray-500/10 border-gray-500/20',
  },
};

const PRIORITY_CONFIG = {
  [TaskPriority.LOW]: {
    label: 'Low',
    className: 'text-gray-400 bg-gray-500/10 border-gray-500/20',
  },
  [TaskPriority.MEDIUM]: {
    label: 'Medium',
    className: 'text-yellow-400 bg-yellow-500/10 border-yellow-500/20',
  },
  [TaskPriority.HIGH]: {
    label: 'High',
    className: 'text-orange-400 bg-orange-500/10 border-orange-500/20',
  },
  [TaskPriority.CRITICAL]: {
    label: 'Critical',
    className: 'text-red-400 bg-red-500/10 border-red-500/20',
  },
};

function Tasks() {
  const [tasks, setTasks] = useState([]);
  const [projects, setProjects] = useState([]);
  const [selectedProjectId, setSelectedProjectId] = useState(null);

  const [loading, setLoading] = useState(true);
  const [tasksLoading, setTasksLoading] = useState(false);

  const [showModal, setShowModal] = useState(false);
  const [editingTask, setEditingTask] = useState(null);

  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [priorityFilter, setPriorityFilter] = useState('all');
  const [sortBy, setSortBy] = useState('priority');

  const [openMenu, setOpenMenu] = useState(null);
  const [saving, setSaving] = useState(false);
  const [deletingId, setDeletingId] = useState(null);

  const [error, setError] = useState('');

  const emptyTask = {
    title: '',
    description: '',
    priority: TaskPriority.MEDIUM,
    status: TaskStatus.TODO,
    estimatedMinutes: '',
    deadline: '',
  };

  const [taskForm, setTaskForm] = useState(emptyTask);

  useEffect(() => {
    fetchProjects();
  }, []);

  useEffect(() => {
    if (!selectedProjectId) return;

    fetchTasks(selectedProjectId);
  }, [selectedProjectId]);

  useEffect(() => {
    const handleOutsideClick = () => {
      setOpenMenu(null);
    };

    if (openMenu !== null) {
      document.addEventListener('click', handleOutsideClick);
    }

    return () => {
      document.removeEventListener('click', handleOutsideClick);
    };
  }, [openMenu]);

  const fetchProjects = async () => {
    try {
      setLoading(true);
      setError('');

      const data = await ApiService.getProjects();
      const projectList = Array.isArray(data) ? data : [];

      setProjects(projectList);

      if (projectList.length > 0) {
        setSelectedProjectId(projectList[0].id);
      }
    } catch (err) {
      console.error('Failed to fetch projects:', err);
      setError('Failed to load projects.');
    } finally {
      setLoading(false);
    }
  };

  const fetchTasks = async (projectId) => {
    try {
      setTasksLoading(true);
      setError('');

      const data = await ApiService.getTasks(projectId);

      setTasks(data.items || []);
    } catch (err) {
      console.error('Failed to fetch tasks:', err);
      setTasks([]);
      setError('Failed to load tasks.');
    } finally {
      setTasksLoading(false);
    }
  };

  const selectedProject = projects.find(
    (project) => String(project.id) === String(selectedProjectId)
  );

  const taskStats = useMemo(() => {
    return {
      total: tasks.length,
      todo: tasks.filter(
        (task) => Number(task.status) === TaskStatus.TODO
      ).length,
      inProgress: tasks.filter(
        (task) => Number(task.status) === TaskStatus.IN_PROGRESS
      ).length,
      completed: tasks.filter(
        (task) => Number(task.status) === TaskStatus.COMPLETED
      ).length,
      blocked: tasks.filter(
        (task) => Number(task.status) === TaskStatus.BLOCKED
      ).length,
    };
  }, [tasks]);

  const filteredTasks = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    const result = tasks.filter((task) => {
      const matchesSearch =
        !normalizedSearch ||
        task.title?.toLowerCase().includes(normalizedSearch) ||
        task.description?.toLowerCase().includes(normalizedSearch);

      const matchesStatus =
        statusFilter === 'all' ||
        Number(task.status) === Number(statusFilter);

      const matchesPriority =
        priorityFilter === 'all' ||
        Number(task.priority) === Number(priorityFilter);

      return matchesSearch && matchesStatus && matchesPriority;
    });

    return [...result].sort((a, b) => {
      if (sortBy === 'priority') {
        return Number(b.priority ?? 0) - Number(a.priority ?? 0);
      }

      if (sortBy === 'deadline') {
        if (!a.deadline) return 1;
        if (!b.deadline) return -1;

        return new Date(a.deadline) - new Date(b.deadline);
      }

      if (sortBy === 'title') {
        return (a.title || '').localeCompare(b.title || '');
      }

      if (sortBy === 'status') {
        return Number(a.status ?? 0) - Number(b.status ?? 0);
      }

      return (
        new Date(b.updatedAt || b.createdAt || 0) -
        new Date(a.updatedAt || a.createdAt || 0)
      );
    });
  }, [
    tasks,
    search,
    statusFilter,
    priorityFilter,
    sortBy,
  ]);

  const openCreateModal = () => {
    setEditingTask(null);
    setTaskForm(emptyTask);
    setError('');
    setShowModal(true);
  };

  const openEditModal = (task) => {
    setEditingTask(task);

    setTaskForm({
      title: task.title || '',
      description: task.description || '',
      priority: Number(task.priority ?? TaskPriority.MEDIUM),
      status: Number(task.status ?? TaskStatus.TODO),
      estimatedMinutes: task.estimatedMinutes ?? '',
      deadline: task.deadline
        ? new Date(task.deadline).toISOString().split('T')[0]
        : '',
    });

    setError('');
    setOpenMenu(null);
    setShowModal(true);
  };

  const closeModal = () => {
    if (saving) return;

    setShowModal(false);
    setEditingTask(null);
    setTaskForm(emptyTask);
    setError('');
  };

  const handleSubmit = async (event) => {
    event.preventDefault();

    if (!selectedProjectId) {
      setError('Please select a project.');
      return;
    }

    if (!taskForm.title.trim()) {
      setError('Task title is required.');
      return;
    }

    try {
      setSaving(true);
      setError('');

      const payload = {
        title: taskForm.title.trim(),
        description: taskForm.description.trim(),
        priority: Number(taskForm.priority),
        status: Number(taskForm.status),
        estimatedMinutes: taskForm.estimatedMinutes
          ? Number(taskForm.estimatedMinutes)
          : null,
        deadline: taskForm.deadline || null,
      };

      if (editingTask) {
        const updated = await ApiService.updateTask(
          selectedProjectId,
          editingTask.id,
          payload
        );

        setTasks((current) =>
          current.map((task) =>
            task.id === editingTask.id ? updated : task
          )
        );
      } else {
        const created = await ApiService.createTask(
          selectedProjectId,
          payload
        );

        setTasks((current) => [...current, created]);
      }

      closeModal();
    } catch (err) {
      console.error('Failed to save task:', err);

      setError(
        err?.response?.data?.message ||
          'Failed to save task. Please try again.'
      );
    } finally {
      setSaving(false);
    }
  };

  const handleToggleTask = async (task) => {
    const nextStatus =
      Number(task.status) === TaskStatus.COMPLETED
        ? TaskStatus.TODO
        : TaskStatus.COMPLETED;

    try {
      const updatedTask = await ApiService.updateTask(
        selectedProjectId,
        task.id,
        {
          ...task,
          status: nextStatus,
        }
      );

      setTasks((current) =>
        current.map((item) =>
          item.id === task.id ? updatedTask : item
        )
      );
    } catch (err) {
      console.error('Failed to update task:', err);
      setError('Failed to update task.');
    }
  };

  const handleDelete = async (task) => {
    const confirmed = window.confirm(
      `Delete "${task.title}"? This action cannot be undone.`
    );

    if (!confirmed) return;

    try {
      setDeletingId(task.id);
      setOpenMenu(null);

      await ApiService.deleteTask(
        selectedProjectId,
        task.id
      );

      setTasks((current) =>
        current.filter((item) => item.id !== task.id)
      );
    } catch (err) {
      console.error('Failed to delete task:', err);
      setError('Failed to delete task.');
    } finally {
      setDeletingId(null);
    }
  };

  const formatDeadline = (deadline) => {
    if (!deadline) return null;

    const date = new Date(deadline);
    const now = new Date();

    const difference = date.getTime() - now.getTime();
    const days = Math.ceil(
      difference / (1000 * 60 * 60 * 24)
    );

    if (days < 0) {
      return {
        label: 'Overdue',
        className: 'text-red-400',
      };
    }

    if (days === 0) {
      return {
        label: 'Due today',
        className: 'text-orange-400',
      };
    }

    if (days === 1) {
      return {
        label: 'Due tomorrow',
        className: 'text-orange-400',
      };
    }

    if (days <= 3) {
      return {
        label: `Due in ${days}d`,
        className: 'text-orange-400',
      };
    }

    return {
      label: date.toLocaleDateString(undefined, {
        day: 'numeric',
        month: 'short',
      }),
      className: 'text-gray-500',
    };
  };

  const formatDuration = (minutes) => {
    if (!minutes || minutes <= 0) return null;

    if (minutes < 60) {
      return `${minutes}m`;
    }

    const hours = Math.floor(minutes / 60);
    const remainingMinutes = minutes % 60;

    if (!remainingMinutes) {
      return `${hours}h`;
    }

    return `${hours}h ${remainingMinutes}m`;
  };

  const getStatusConfig = (status) => {
    return (
      STATUS_CONFIG[Number(status)] ||
      STATUS_CONFIG[TaskStatus.TODO]
    );
  };

  const getPriorityConfig = (priority) => {
    return (
      PRIORITY_CONFIG[Number(priority)] ||
      PRIORITY_CONFIG[TaskPriority.MEDIUM]
    );
  };

  const clearFilters = () => {
    setSearch('');
    setStatusFilter('all');
    setPriorityFilter('all');
  };

  if (loading) {
    return (
      <div className="max-w-7xl mx-auto px-6 py-8">
        <div className="devos-card min-h-[360px] flex flex-col items-center justify-center">
          <div className="w-8 h-8 rounded-full border-2 border-devos-border border-t-devos-primary animate-spin mb-4" />
          <p className="text-sm text-gray-400">
            Loading tasks...
          </p>
        </div>
      </div>
    );
  }

  if (projects.length === 0) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 py-8">
        <div className="devos-card min-h-[420px] flex flex-col items-center justify-center text-center px-6">
          <div className="w-14 h-14 rounded-2xl bg-devos-primary/10 border border-devos-primary/20 flex items-center justify-center mb-5">
            <CheckSquare className="w-7 h-7 text-devos-primary" />
          </div>

          <h1 className="text-xl font-semibold text-white mb-2">
            No projects available
          </h1>

          <p className="max-w-md text-sm text-gray-400">
            Create a project first. Tasks belong to a specific
            project in DevOS.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 py-6 sm:py-8">
      {/* Header */}
      <div className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between mb-7">
        <div>
          <div className="flex items-center gap-3 mb-2">
            <div className="w-10 h-10 rounded-xl bg-devos-primary/10 border border-devos-primary/20 flex items-center justify-center">
              <CheckSquare className="w-5 h-5 text-devos-primary" />
            </div>

            <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-white">
              Tasks
            </h1>
          </div>

          <p className="text-sm text-gray-400">
            Organize and track your development work.
          </p>
        </div>

        <button
          onClick={openCreateModal}
          className="devos-button-primary inline-flex items-center justify-center gap-2 self-start lg:self-auto"
        >
          <Plus className="w-4 h-4" />
          New Task
        </button>
      </div>

      {/* Project selector */}
      <div className="devos-card p-4 mb-4">
        <div className="flex flex-col sm:flex-row sm:items-center gap-3">
          <div className="flex items-center gap-2 text-sm text-gray-400">
            <FolderKanban className="w-4 h-4 text-gray-500" />
            Project
          </div>

          <div className="relative flex-1">
            <select
              value={selectedProjectId || ''}
              onChange={(event) =>
                setSelectedProjectId(event.target.value)
              }
              className="appearance-none w-full h-10 pl-3 pr-9 bg-devos-dark border border-devos-border rounded-lg text-sm text-white focus:outline-none focus:border-devos-primary/60 cursor-pointer"
            >
              {projects.map((project) => (
                <option
                  key={project.id}
                  value={project.id}
                  className="bg-gray-900 text-white"
                >
                  {project.name}
                </option>
              ))}
            </select>

            <ChevronDown className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />
          </div>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mb-4">
        <div className="devos-card p-4">
          <p className="text-xs uppercase tracking-wide text-gray-500">
            Total
          </p>
          <p className="text-2xl font-semibold text-white mt-1">
            {taskStats.total}
          </p>
        </div>

        <div className="devos-card p-4">
          <p className="text-xs uppercase tracking-wide text-gray-500">
            Todo
          </p>
          <p className="text-2xl font-semibold text-gray-300 mt-1">
            {taskStats.todo}
          </p>
        </div>

        <div className="devos-card p-4">
          <p className="text-xs uppercase tracking-wide text-gray-500">
            In Progress
          </p>
          <p className="text-2xl font-semibold text-blue-400 mt-1">
            {taskStats.inProgress}
          </p>
        </div>

        <div className="devos-card p-4">
          <p className="text-xs uppercase tracking-wide text-gray-500">
            Completed
          </p>
          <p className="text-2xl font-semibold text-emerald-400 mt-1">
            {taskStats.completed}
          </p>
        </div>
      </div>

      {/* Toolbar */}
      <div className="devos-card p-3 mb-5">
        <div className="flex flex-col xl:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />

            <input
              type="text"
              value={search}
              onChange={(event) =>
                setSearch(event.target.value)
              }
              placeholder="Search tasks..."
              className="w-full h-10 pl-10 pr-4 bg-devos-dark border border-devos-border rounded-lg text-sm text-white placeholder:text-gray-600 focus:outline-none focus:border-devos-primary/60"
            />
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
            <div className="relative">
              <select
                value={statusFilter}
                onChange={(event) =>
                  setStatusFilter(event.target.value)
                }
                className="appearance-none w-full h-10 pl-3 pr-8 bg-devos-dark border border-devos-border rounded-lg text-sm text-gray-300 focus:outline-none focus:border-devos-primary/60 cursor-pointer"
              >
                <option value="all" className="bg-gray-900 text-white">All status</option>
                <option value={TaskStatus.TODO} className="bg-gray-900 text-white">Todo</option>
                <option value={TaskStatus.IN_PROGRESS} className="bg-gray-900 text-white">
                  In Progress
                </option>
                <option value={TaskStatus.BLOCKED} className="bg-gray-900 text-white">
                  Blocked
                </option>
                <option value={TaskStatus.COMPLETED} className="bg-gray-900 text-white">
                  Completed
                </option>
                <option value={TaskStatus.CANCELLED} className="bg-gray-900 text-white">
                  Cancelled
                </option>
              </select>

              <ChevronDown className="pointer-events-none absolute right-2.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />
            </div>

            <div className="relative">
              <select
                value={priorityFilter}
                onChange={(event) =>
                  setPriorityFilter(event.target.value)
                }
                className="appearance-none w-full h-10 pl-3 pr-8 bg-devos-dark border border-devos-border rounded-lg text-sm text-gray-300 focus:outline-none focus:border-devos-primary/60 cursor-pointer"
              >
                <option value="all" className="bg-gray-900 text-white">All priority</option>
                <option value={TaskPriority.LOW} className="bg-gray-900 text-white">Low</option>
                <option value={TaskPriority.MEDIUM} className="bg-gray-900 text-white">
                  Medium
                </option>
                <option value={TaskPriority.HIGH} className="bg-gray-900 text-white">High</option>
                <option value={TaskPriority.CRITICAL} className="bg-gray-900 text-white">
                  Critical
                </option>
              </select>

              <ChevronDown className="pointer-events-none absolute right-2.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />
            </div>

            <div className="relative col-span-2 sm:col-span-1">
              <select
                value={sortBy}
                onChange={(event) =>
                  setSortBy(event.target.value)
                }
                className="appearance-none w-full h-10 pl-3 pr-8 bg-devos-dark border border-devos-border rounded-lg text-sm text-gray-300 focus:outline-none focus:border-devos-primary/60 cursor-pointer"
              >
                <option value="priority" className="bg-gray-900 text-white">
                  Priority
                </option>
                <option value="deadline" className="bg-gray-900 text-white">
                  Deadline
                </option>
                <option value="title" className="bg-gray-900 text-white">Title</option>
                <option value="status" className="bg-gray-900 text-white">Status</option>
                <option value="updated" className="bg-gray-900 text-white">
                  Recently updated
                </option>
              </select>

              <ChevronDown className="pointer-events-none absolute right-2.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />
            </div>
          </div>
        </div>
      </div>

      {/* Error */}
      {error && (
        <div className="mb-5 rounded-lg border border-red-500/20 bg-red-500/5 px-4 py-3">
          <p className="text-sm text-red-400">{error}</p>
        </div>
      )}

      {/* Tasks */}
      {tasksLoading ? (
        <div className="devos-card min-h-[320px] flex flex-col items-center justify-center">
          <div className="w-8 h-8 rounded-full border-2 border-devos-border border-t-devos-primary animate-spin mb-4" />
          <p className="text-sm text-gray-400">
            Loading tasks...
          </p>
        </div>
      ) : filteredTasks.length === 0 ? (
        <div className="devos-card min-h-[320px] flex flex-col items-center justify-center text-center px-6">
          {tasks.length === 0 ? (
            <>
              <div className="w-14 h-14 rounded-2xl bg-devos-primary/10 border border-devos-primary/20 flex items-center justify-center mb-5">
                <CheckSquare className="w-7 h-7 text-devos-primary" />
              </div>

              <h2 className="text-xl font-semibold text-white mb-2">
                No tasks yet
              </h2>

              <p className="max-w-md text-sm text-gray-400 mb-6">
                Add your first task to start organizing work
                in{' '}
                <span className="text-gray-300">
                  {selectedProject?.name}
                </span>
                .
              </p>

              <button
                onClick={openCreateModal}
                className="devos-button-primary inline-flex items-center gap-2"
              >
                <Plus className="w-4 h-4" />
                Create task
              </button>
            </>
          ) : (
            <>
              <Search className="w-8 h-8 text-gray-600 mb-4" />

              <h2 className="text-lg font-semibold text-white mb-2">
                No matching tasks
              </h2>

              <p className="text-sm text-gray-400 mb-5">
                Try changing your search or filters.
              </p>

              <button
                onClick={clearFilters}
                className="devos-button-secondary"
              >
                Clear filters
              </button>
            </>
          )}
        </div>
      ) : (
        <div className="space-y-2">
          {filteredTasks.map((task) => {
            const status = getStatusConfig(task.status);
            const priority = getPriorityConfig(task.priority);
            const StatusIcon = status.icon;
            const deadline = formatDeadline(task.deadline);
            const completed =
              Number(task.status) === TaskStatus.COMPLETED;
            const isDeleting = deletingId === task.id;

            return (
              <div
                key={task.id}
                className={`group devos-card p-4 transition-colors duration-200 ${
                  isDeleting
                    ? 'opacity-50 pointer-events-none'
                    : ''
                }`}
              >
                <div className="flex items-start gap-3">
                  {/* Complete */}
                  <button
                    onClick={() => handleToggleTask(task)}
                    disabled={task.status === TaskStatus.CANCELLED}
                    className={`mt-0.5 w-5 h-5 shrink-0 rounded-md border flex items-center justify-center transition-colors ${
                      completed
                        ? 'bg-emerald-500 border-emerald-500'
                        : 'border-gray-600 hover:border-devos-primary hover:bg-devos-primary/5'
                    } disabled:opacity-40 disabled:cursor-not-allowed`}
                    aria-label={
                      completed
                        ? 'Mark task as incomplete'
                        : 'Complete task'
                    }
                  >
                    {completed && (
                      <Check className="w-3.5 h-3.5 text-white" />
                    )}
                  </button>

                  {/* Main */}
                  <div className="flex-1 min-w-0">
                    <div className="flex flex-col lg:flex-row lg:items-start gap-2 lg:gap-4">
                      <div className="min-w-0 flex-1">
                        <h3
                          className={`font-medium leading-6 ${
                            completed
                              ? 'text-gray-500 line-through'
                              : 'text-white'
                          }`}
                        >
                          {task.title}
                        </h3>

                        {task.description && (
                          <p
                            className={`mt-1 text-sm line-clamp-2 ${
                              completed
                                ? 'text-gray-600'
                                : 'text-gray-500'
                            }`}
                          >
                            {task.description}
                          </p>
                        )}
                      </div>

                      {/* Badges */}
                      <div className="flex items-center gap-2 shrink-0">
                        <span
                          className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md border text-xs font-medium ${status.className}`}
                        >
                          <StatusIcon className="w-3.5 h-3.5" />
                          {status.label}
                        </span>

                        <span
                          className={`px-2.5 py-1 rounded-md border text-xs font-medium ${priority.className}`}
                        >
                          {priority.label}
                        </span>
                      </div>
                    </div>

                    {/* Metadata */}
                    <div className="flex flex-wrap items-center gap-x-4 gap-y-2 mt-3">
                      {deadline && (
                        <span
                          className={`inline-flex items-center gap-1.5 text-xs ${deadline.className}`}
                        >
                          <Calendar className="w-3.5 h-3.5" />
                          {deadline.label}
                        </span>
                      )}

                      {task.estimatedMinutes && (
                        <span className="inline-flex items-center gap-1.5 text-xs text-gray-500">
                          <Clock3 className="w-3.5 h-3.5" />
                          {formatDuration(
                            Number(task.estimatedMinutes)
                          )}
                        </span>
                      )}

                      {Number(task.priority) ===
                        TaskPriority.CRITICAL && (
                        <span className="inline-flex items-center gap-1.5 text-xs text-red-400">
                          <Zap className="w-3.5 h-3.5" />
                          Critical
                        </span>
                      )}
                    </div>
                  </div>

                  {/* Menu */}
                  <div className="relative shrink-0">
                    <button
                      onClick={(event) => {
                        event.stopPropagation();

                        setOpenMenu(
                          openMenu === task.id
                            ? null
                            : task.id
                        );
                      }}
                      className="w-8 h-8 rounded-lg flex items-center justify-center text-gray-600 hover:text-white hover:bg-devos-border/60 transition-colors"
                      aria-label={`Actions for ${task.title}`}
                    >
                      <MoreHorizontal className="w-5 h-5" />
                    </button>

                    {openMenu === task.id && (
                      <div
                        onClick={(event) =>
                          event.stopPropagation()
                        }
                        className="absolute right-0 top-9 z-20 w-40 p-1.5 rounded-lg bg-devos-surface border border-devos-border shadow-xl"
                      >
                        <button
                          onClick={() =>
                            openEditModal(task)
                          }
                          className="w-full flex items-center gap-2 px-3 py-2 rounded-md text-sm text-gray-300 hover:text-white hover:bg-devos-border/60 transition-colors"
                        >
                          <Pencil className="w-4 h-4" />
                          Edit
                        </button>

                        <button
                          onClick={() =>
                            handleDelete(task)
                          }
                          className="w-full flex items-center gap-2 px-3 py-2 rounded-md text-sm text-red-400 hover:text-red-300 hover:bg-red-500/10 transition-colors"
                        >
                          <Trash2 className="w-4 h-4" />
                          Delete
                        </button>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Create / Edit Modal */}
      {showModal && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/70 backdrop-blur-sm"
          onMouseDown={(event) => {
            if (event.target === event.currentTarget) {
              closeModal();
            }
          }}
        >
          <div className="w-full max-w-lg rounded-xl bg-devos-surface border border-devos-border shadow-2xl">
            {/* Header */}
            <div className="flex items-center justify-between px-6 py-5 border-b border-devos-border">
              <div>
                <h2 className="text-lg font-semibold text-white">
                  {editingTask
                    ? 'Edit Task'
                    : 'New Task'}
                </h2>

                <p className="text-sm text-gray-500 mt-1">
                  {editingTask
                    ? 'Update task details and status.'
                    : 'Add a task to your current project.'}
                </p>
              </div>

              <button
                onClick={closeModal}
                disabled={saving}
                className="w-8 h-8 rounded-lg flex items-center justify-center text-gray-500 hover:text-white hover:bg-devos-border transition-colors"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {/* Form */}
            <form onSubmit={handleSubmit}>
              <div className="p-6 space-y-5">
                {error && (
                  <div className="rounded-lg border border-red-500/20 bg-red-500/5 px-3 py-2.5">
                    <p className="text-sm text-red-400">
                      {error}
                    </p>
                  </div>
                )}

                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Title
                  </label>

                  <input
                    type="text"
                    value={taskForm.title}
                    onChange={(event) =>
                      setTaskForm((current) => ({
                        ...current,
                        title: event.target.value,
                      }))
                    }
                    placeholder="e.g. Implement authentication"
                    className="devos-input"
                    autoFocus
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Description
                    <span className="text-gray-600 ml-1">
                      (optional)
                    </span>
                  </label>

                  <textarea
                    value={taskForm.description}
                    onChange={(event) =>
                      setTaskForm((current) => ({
                        ...current,
                        description: event.target.value,
                      }))
                    }
                    placeholder="Describe what needs to be done..."
                    className="devos-input resize-none"
                    rows={4}
                  />
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Priority
                    </label>

                    <select
                      value={taskForm.priority}
                      onChange={(event) =>
                        setTaskForm((current) => ({
                          ...current,
                          priority: Number(
                            event.target.value
                          ),
                        }))
                      }
                      className="devos-input"
                    >
                      <option value={TaskPriority.LOW} className="bg-gray-900 text-white">
                        Low
                      </option>

                      <option value={TaskPriority.MEDIUM} className="bg-gray-900 text-white">
                        Medium
                      </option>

                      <option value={TaskPriority.HIGH} className="bg-gray-900 text-white">
                        High
                      </option>

                      <option value={TaskPriority.CRITICAL} className="bg-gray-900 text-white">
                        Critical
                      </option>
                    </select>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Status
                    </label>

                    <select
                      value={taskForm.status}
                      onChange={(event) =>
                        setTaskForm((current) => ({
                          ...current,
                          status: Number(
                            event.target.value
                          ),
                        }))
                      }
                      className="devos-input"
                    >
                      <option value={TaskStatus.TODO} className="bg-gray-900 text-white">
                        Todo
                      </option>

                      <option value={TaskStatus.IN_PROGRESS} className="bg-gray-900 text-white">
                        In Progress
                      </option>

                      <option value={TaskStatus.BLOCKED} className="bg-gray-900 text-white">
                        Blocked
                      </option>

                      <option value={TaskStatus.COMPLETED} className="bg-gray-900 text-white">
                        Completed
                      </option>

                      <option value={TaskStatus.CANCELLED} className="bg-gray-900 text-white">
                        Cancelled
                      </option>
                    </select>
                  </div>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Estimated time
                    </label>

                    <div className="relative">
                      <input
                        type="number"
                        min="1"
                        value={
                          taskForm.estimatedMinutes
                        }
                        onChange={(event) =>
                          setTaskForm((current) => ({
                            ...current,
                            estimatedMinutes:
                              event.target.value,
                          }))
                        }
                        placeholder="60"
                        className="devos-input pr-12"
                      />

                      <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-gray-600">
                        min
                      </span>
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Deadline
                    </label>

                    <input
                      type="date"
                      value={taskForm.deadline}
                      onChange={(event) =>
                        setTaskForm((current) => ({
                          ...current,
                          deadline: event.target.value,
                        }))
                      }
                      className="devos-input"
                    />
                  </div>
                </div>
              </div>

              {/* Footer */}
              <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-devos-border bg-devos-dark/20">
                <button
                  type="button"
                  onClick={closeModal}
                  disabled={saving}
                  className="devos-button-secondary"
                >
                  Cancel
                </button>

                <button
                  type="submit"
                  disabled={saving}
                  className="devos-button-primary inline-flex items-center gap-2 min-w-[110px] justify-center disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {saving ? (
                    <>
                      <span className="w-4 h-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />
                      Saving
                    </>
                  ) : editingTask ? (
                    <>
                      <Check className="w-4 h-4" />
                      Save changes
                    </>
                  ) : (
                    <>
                      <Plus className="w-4 h-4" />
                      Create
                    </>
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default Tasks;