import React, { useEffect, useMemo, useState } from 'react';
import {
  FolderKanban,
  Plus,
  Search,
  MoreVertical,
  Calendar,
  CheckCircle2,
  Clock3,
  Archive,
  Pencil,
  Trash2,
  X,
  ChevronDown,
  AlertCircle,
} from 'lucide-react';

import ApiService from '../services/ApiService';
import { ProjectStatus, TaskPriority } from '../services/apiContract';

function Projects() {
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [showModal, setShowModal] = useState(false);
  const [editingProject, setEditingProject] = useState(null);

  const [openMenuId, setOpenMenuId] = useState(null);

  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [sortBy, setSortBy] = useState('updated');

  const emptyProject = {
    name: '',
    description: '',
    priority: TaskPriority.MEDIUM,
    deadline: '',
    status: ProjectStatus.ACTIVE,
  };

  const [formData, setFormData] = useState(emptyProject);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    fetchProjects();
  }, []);

  useEffect(() => {
    const handleClickOutside = () => {
      setOpenMenuId(null);
    };

    if (openMenuId !== null) {
      document.addEventListener('click', handleClickOutside);
    }

    return () => {
      document.removeEventListener('click', handleClickOutside);
    };
  }, [openMenuId]);

  const fetchProjects = async () => {
    setLoading(true);
    setError('');

    try {
      const data = await ApiService.getProjects();
      setProjects(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error('Failed to fetch projects:', err);
      setError('Failed to load projects.');
    } finally {
      setLoading(false);
    }
  };

  const getStatusConfig = (status) => {
    switch (Number(status)) {
      case ProjectStatus.ACTIVE:
        return {
          label: 'Active',
          className: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
          icon: Clock3,
        };

      case ProjectStatus.COMPLETED:
        return {
          label: 'Completed',
          className: 'bg-blue-500/10 text-blue-400 border-blue-500/20',
          icon: CheckCircle2,
        };

      case ProjectStatus.ARCHIVED:
        return {
          label: 'Archived',
          className: 'bg-gray-500/10 text-gray-400 border-gray-500/20',
          icon: Archive,
        };

      default:
        return {
          label: 'Unknown',
          className: 'bg-gray-500/10 text-gray-400 border-gray-500/20',
          icon: AlertCircle,
        };
    }
  };

  const getPriorityConfig = (priority) => {
    switch (Number(priority)) {
      case TaskPriority.CRITICAL:
        return {
          label: 'Critical',
          className: 'text-red-400',
        };

      case TaskPriority.HIGH:
        return {
          label: 'High',
          className: 'text-orange-400',
        };

      case TaskPriority.MEDIUM:
        return {
          label: 'Medium',
          className: 'text-yellow-400',
        };

      default:
        return {
          label: 'Low',
          className: 'text-gray-400',
        };
    }
  };

  const filteredProjects = useMemo(() => {
    let result = [...projects];

    const normalizedSearch = search.trim().toLowerCase();

    if (normalizedSearch) {
      result = result.filter((project) => {
        const name = project.name?.toLowerCase() || '';
        const description = project.description?.toLowerCase() || '';

        return (
          name.includes(normalizedSearch) ||
          description.includes(normalizedSearch)
        );
      });
    }

    if (statusFilter !== 'all') {
      result = result.filter(
        (project) => Number(project.status) === Number(statusFilter)
      );
    }

    result.sort((a, b) => {
      switch (sortBy) {
        case 'name':
          return (a.name || '').localeCompare(b.name || '');

        case 'deadline': {
          const aDate = a.deadline
            ? new Date(a.deadline).getTime()
            : Number.MAX_SAFE_INTEGER;

          const bDate = b.deadline
            ? new Date(b.deadline).getTime()
            : Number.MAX_SAFE_INTEGER;

          return aDate - bDate;
        }

        case 'status':
          return Number(a.status ?? 0) - Number(b.status ?? 0);

        case 'updated':
        default: {
          const aDate = new Date(
            a.updatedAt || a.createdAt || 0
          ).getTime();

          const bDate = new Date(
            b.updatedAt || b.createdAt || 0
          ).getTime();

          return bDate - aDate;
        }
      }
    });

    return result;
  }, [projects, search, statusFilter, sortBy]);

  const projectStats = useMemo(() => {
    return {
      total: projects.length,
      active: projects.filter(
        (project) => Number(project.status) === ProjectStatus.ACTIVE
      ).length,
      completed: projects.filter(
        (project) => Number(project.status) === ProjectStatus.COMPLETED
      ).length,
      archived: projects.filter(
        (project) => Number(project.status) === ProjectStatus.ARCHIVED
      ).length,
    };
  }, [projects]);

  const formatDate = (date) => {
    if (!date) return null;

    const parsed = new Date(date);

    if (Number.isNaN(parsed.getTime())) {
      return null;
    }

    return parsed.toLocaleDateString(undefined, {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  };

  const getDeadlineState = (deadline, status) => {
    if (!deadline || Number(status) === ProjectStatus.COMPLETED) {
      return {
        label: formatDate(deadline),
        className: 'text-gray-500',
      };
    }

    const deadlineDate = new Date(deadline);

    if (Number.isNaN(deadlineDate.getTime())) {
      return {
        label: null,
        className: 'text-gray-500',
      };
    }

    const now = new Date();

    const diff = deadlineDate.getTime() - now.getTime();
    const days = Math.ceil(diff / (1000 * 60 * 60 * 24));

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

    if (days <= 3) {
      return {
        label: `${days}d left`,
        className: 'text-orange-400',
      };
    }

    return {
      label: formatDate(deadline),
      className: 'text-gray-400',
    };
  };

  const openCreateModal = () => {
    setEditingProject(null);
    setFormData(emptyProject);
    setShowModal(true);
  };

  const openEditModal = (project) => {
    setEditingProject(project);

    setFormData({
      name: project.name || '',
      description: project.description || '',
      priority: Number(project.priority ?? TaskPriority.MEDIUM),
      deadline: project.deadline
        ? new Date(project.deadline).toISOString().slice(0, 10)
        : '',
      status: Number(project.status ?? ProjectStatus.ACTIVE),
    });

    setOpenMenuId(null);
    setShowModal(true);
  };

  const closeModal = () => {
    if (saving) return;

    setShowModal(false);
    setEditingProject(null);
    setFormData(emptyProject);
  };

  const handleSubmit = async (event) => {
    event.preventDefault();

    if (!formData.name.trim()) {
      return;
    }

    setSaving(true);
    setError('');

    try {
      const payload = {
        name: formData.name.trim(),
        description: formData.description.trim() || null,
        priority: Number(formData.priority),
        deadline: formData.deadline
          ? new Date(formData.deadline).toISOString()
          : null,
        status: Number(formData.status),
      };

      if (editingProject) {
        const updated = await ApiService.updateProject(
          editingProject.id,
          payload
        );

        setProjects((current) =>
          current.map((project) =>
            project.id === editingProject.id
              ? updated
              : project
          )
        );
      } else {
        const created = await ApiService.createProject(payload);

        setProjects((current) => [...current, created]);
      }

      closeModal();
    } catch (err) {
      console.error('Failed to save project:', err);
      setError(
        editingProject
          ? 'Failed to update project.'
          : 'Failed to create project.'
      );
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (project) => {
    setOpenMenuId(null);

    const confirmed = window.confirm(
      `Delete project "${project.name}"?\n\nThis action cannot be undone.`
    );

    if (!confirmed) return;

    try {
      await ApiService.deleteProject(project.id);

      setProjects((current) =>
        current.filter((item) => item.id !== project.id)
      );
    } catch (err) {
      console.error('Failed to delete project:', err);
      setError('Failed to delete project.');
    }
  };

  const renderLoading = () => (
    <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      {Array.from({ length: 6 }).map((_, index) => (
        <div
          key={index}
          className="bg-devos-surface border border-devos-border rounded-xl p-5 animate-pulse"
        >
          <div className="flex justify-between">
            <div className="w-10 h-10 rounded-lg bg-devos-border" />
            <div className="w-6 h-6 rounded bg-devos-border" />
          </div>

          <div className="mt-5 h-5 w-2/3 rounded bg-devos-border" />
          <div className="mt-3 h-3 w-full rounded bg-devos-border" />
          <div className="mt-2 h-3 w-4/5 rounded bg-devos-border" />

          <div className="mt-6 flex justify-between">
            <div className="h-6 w-20 rounded bg-devos-border" />
            <div className="h-4 w-24 rounded bg-devos-border" />
          </div>
        </div>
      ))}
    </div>
  );

  const renderEmpty = () => (
    <div className="bg-devos-surface border border-devos-border rounded-xl">
      <div className="flex flex-col items-center justify-center py-16 px-6 text-center">
        <div className="w-14 h-14 rounded-xl bg-devos-primary/10 border border-devos-primary/20 flex items-center justify-center mb-5">
          <FolderKanban className="w-7 h-7 text-devos-primary" />
        </div>

        <h2 className="text-lg font-semibold text-white">
          {search || statusFilter !== 'all'
            ? 'No projects found'
            : 'No projects yet'}
        </h2>

        <p className="text-sm text-gray-500 mt-2 max-w-md">
          {search || statusFilter !== 'all'
            ? 'Try changing your search or filters.'
            : 'Create your first project to start organizing your development work.'}
        </p>

        {!search && statusFilter === 'all' && (
          <button
            type="button"
            onClick={openCreateModal}
            className="devos-button-primary mt-6 inline-flex items-center gap-2"
          >
            <Plus className="w-4 h-4" />
            Create Project
          </button>
        )}
      </div>
    </div>
  );

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">

      {/* Header */}
      <div className="flex flex-col lg:flex-row lg:items-end lg:justify-between gap-5 mb-7">
        <div>
          <div className="flex items-center gap-2 text-xs text-gray-500 mb-2">
            <FolderKanban className="w-3.5 h-3.5" />
            Workspace
            <span>/</span>
            Projects
          </div>

          <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-white">
            Projects
          </h1>

          <p className="text-sm text-gray-500 mt-1">
            Manage your development projects and track their progress.
          </p>
        </div>

        <button
          type="button"
          onClick={openCreateModal}
          className="devos-button-primary inline-flex items-center justify-center gap-2 self-start lg:self-auto"
        >
          <Plus className="w-4 h-4" />
          New Project
        </button>
      </div>

      {/* Error */}
      {error && (
        <div className="mb-5 flex items-center gap-3 rounded-lg border border-red-500/20 bg-red-500/10 px-4 py-3 text-sm text-red-400">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{error}</span>

          <button
            type="button"
            onClick={() => setError('')}
            className="ml-auto text-red-400/70 hover:text-red-300"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {/* Overview */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 mb-6">
        <div className="bg-devos-surface border border-devos-border rounded-xl px-4 py-3">
          <p className="text-xs text-gray-500">Total</p>
          <p className="text-xl font-semibold text-white mt-1">
            {projectStats.total}
          </p>
        </div>

        <div className="bg-devos-surface border border-devos-border rounded-xl px-4 py-3">
          <p className="text-xs text-gray-500">Active</p>
          <p className="text-xl font-semibold text-emerald-400 mt-1">
            {projectStats.active}
          </p>
        </div>

        <div className="bg-devos-surface border border-devos-border rounded-xl px-4 py-3">
          <p className="text-xs text-gray-500">Completed</p>
          <p className="text-xl font-semibold text-blue-400 mt-1">
            {projectStats.completed}
          </p>
        </div>

        <div className="bg-devos-surface border border-devos-border rounded-xl px-4 py-3">
          <p className="text-xs text-gray-500">Archived</p>
          <p className="text-xl font-semibold text-gray-400 mt-1">
            {projectStats.archived}
          </p>
        </div>
      </div>

      {/* Toolbar */}
      <div className="bg-devos-surface border border-devos-border rounded-xl p-3 mb-5">
        <div className="flex flex-col md:flex-row gap-3">

          {/* Search */}
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />

            <input
              type="text"
              placeholder="Search projects..."
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              className="w-full h-10 pl-9 pr-4 bg-devos-dark border border-devos-border rounded-lg text-sm text-gray-200 placeholder:text-gray-600 focus:outline-none focus:border-devos-primary/60 focus:ring-1 focus:ring-devos-primary/30 transition-all"
            />
          </div>

          {/* Status */}
          <div className="relative">
            <select
              value={statusFilter}
              onChange={(event) => setStatusFilter(event.target.value)}
              className="appearance-none h-10 w-full md:w-40 pl-3 pr-9 bg-devos-dark border border-devos-border rounded-lg text-sm text-gray-300 focus:outline-none focus:border-devos-primary/60"
            >
              <option value="all">All statuses</option>
              <option value={ProjectStatus.ACTIVE}>Active</option>
              <option value={ProjectStatus.COMPLETED}>Completed</option>
              <option value={ProjectStatus.ARCHIVED}>Archived</option>
            </select>

            <ChevronDown className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />
          </div>

          {/* Sort */}
          <div className="relative">
            <select
              value={sortBy}
              onChange={(event) => setSortBy(event.target.value)}
              className="appearance-none h-10 w-full md:w-40 pl-3 pr-9 bg-devos-dark border border-devos-border rounded-lg text-sm text-gray-300 focus:outline-none focus:border-devos-primary/60"
            >
              <option value="updated">Recently updated</option>
              <option value="name">Name</option>
              <option value="deadline">Deadline</option>
              <option value="status">Status</option>
            </select>

            <ChevronDown className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />
          </div>
        </div>
      </div>

      {/* Content */}
      {loading ? (
        renderLoading()
      ) : filteredProjects.length === 0 ? (
        renderEmpty()
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
          {filteredProjects.map((project) => {
            const status = getStatusConfig(project.status);
            const StatusIcon = status.icon;

            const priority = getPriorityConfig(project.priority);
            const deadline = getDeadlineState(
              project.deadline,
              project.status
            );

            return (
              <div
                key={project.id}
                className="
                  group
                  relative
                  bg-devos-surface
                  border border-devos-border
                  rounded-xl
                  p-5
                  transition-all duration-200
                  hover:border-devos-primary/30
                  hover:-translate-y-0.5
                "
              >
                {/* Card top */}
                <div className="flex items-start justify-between gap-4">

                  <div className="w-10 h-10 rounded-lg bg-devos-primary/10 border border-devos-primary/20 flex items-center justify-center shrink-0">
                    <FolderKanban className="w-5 h-5 text-devos-primary" />
                  </div>

                  <div className="relative">
                    <button
                      type="button"
                      onClick={(event) => {
                        event.stopPropagation();
                        setOpenMenuId(
                          openMenuId === project.id
                            ? null
                            : project.id
                        );
                      }}
                      className="
                        w-8 h-8
                        flex items-center justify-center
                        rounded-lg
                        text-gray-500
                        hover:text-gray-200
                        hover:bg-devos-dark
                        border border-transparent
                        hover:border-devos-border
                        transition-colors
                      "
                      aria-label="Project actions"
                    >
                      <MoreVertical className="w-4 h-4" />
                    </button>

                    {openMenuId === project.id && (
                      <div
                        onClick={(event) => event.stopPropagation()}
                        className="
                          absolute right-0 top-9 z-20
                          w-40
                          p-1
                          rounded-lg
                          bg-devos-surface
                          border border-devos-border
                          shadow-2xl
                        "
                      >
                        <button
                          type="button"
                          onClick={() => openEditModal(project)}
                          className="
                            w-full flex items-center gap-2
                            px-3 py-2
                            rounded-md
                            text-sm text-gray-300
                            hover:bg-devos-dark
                            hover:text-white
                            text-left
                          "
                        >
                          <Pencil className="w-3.5 h-3.5" />
                          Edit
                        </button>

                        <button
                          type="button"
                          onClick={() => handleDelete(project)}
                          className="
                            w-full flex items-center gap-2
                            px-3 py-2
                            rounded-md
                            text-sm text-red-400
                            hover:bg-red-500/10
                            text-left
                          "
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                          Delete
                        </button>
                      </div>
                    )}
                  </div>
                </div>

                {/* Title */}
                <div className="mt-5">
                  <h2 className="text-base font-semibold text-white truncate">
                    {project.name || 'Untitled Project'}
                  </h2>

                  <p className="text-sm text-gray-500 mt-1.5 line-clamp-2 min-h-[40px]">
                    {project.description || 'No description provided.'}
                  </p>
                </div>

                {/* Meta */}
                <div className="mt-5 flex flex-wrap items-center gap-2">
                  <span
                    className={`
                      inline-flex items-center gap-1.5
                      px-2.5 py-1
                      rounded-md
                      border
                      text-xs font-medium
                      ${status.className}
                    `}
                  >
                    <StatusIcon className="w-3.5 h-3.5" />
                    {status.label}
                  </span>

                  <span
                    className={`text-xs font-medium ${priority.className}`}
                  >
                    {priority.label} priority
                  </span>
                </div>

                {/* Footer */}
                <div className="mt-5 pt-4 border-t border-devos-border flex items-center justify-between gap-3">
                  <div className="flex items-center gap-2 text-xs">
                    <Calendar className="w-3.5 h-3.5 text-gray-600" />

                    <span className={deadline.className}>
                      {deadline.label || 'No deadline'}
                    </span>
                  </div>

                  {project.taskCount !== undefined && (
                    <div className="flex items-center gap-1.5 text-xs text-gray-500">
                      <CheckCircle2 className="w-3.5 h-3.5" />
                      {project.taskCount} tasks
                    </div>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Modal */}
      {showModal && (
        <div
          className="
            fixed inset-0 z-[100]
            flex items-center justify-center
            p-4
            bg-black/70
            backdrop-blur-sm
          "
          onMouseDown={(event) => {
            if (event.target === event.currentTarget) {
              closeModal();
            }
          }}
        >
          <div
            className="
              w-full max-w-lg
              bg-devos-surface
              border border-devos-border
              rounded-2xl
              shadow-2xl
              overflow-hidden
            "
          >
            {/* Modal header */}
            <div className="flex items-center justify-between px-6 py-5 border-b border-devos-border">
              <div>
                <h2 className="text-lg font-semibold text-white">
                  {editingProject ? 'Edit Project' : 'New Project'}
                </h2>

                <p className="text-xs text-gray-500 mt-1">
                  {editingProject
                    ? 'Update project information.'
                    : 'Create a workspace for your development work.'}
                </p>
              </div>

              <button
                type="button"
                onClick={closeModal}
                disabled={saving}
                className="
                  w-8 h-8
                  flex items-center justify-center
                  rounded-lg
                  text-gray-500
                  hover:text-white
                  hover:bg-devos-dark
                  transition-colors
                "
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            {/* Modal body */}
            <form onSubmit={handleSubmit}>
              <div className="p-6 space-y-5">

                {/* Name */}
                <div>
                  <label className="block text-xs font-medium text-gray-400 mb-2">
                    Project name
                  </label>

                  <input
                    type="text"
                    value={formData.name}
                    onChange={(event) =>
                      setFormData({
                        ...formData,
                        name: event.target.value,
                      })
                    }
                    placeholder="e.g. DevOS"
                    className="devos-input"
                    required
                    autoFocus
                  />
                </div>

                {/* Description */}
                <div>
                  <label className="block text-xs font-medium text-gray-400 mb-2">
                    Description
                  </label>

                  <textarea
                    value={formData.description}
                    onChange={(event) =>
                      setFormData({
                        ...formData,
                        description: event.target.value,
                      })
                    }
                    placeholder="What are you building?"
                    rows={4}
                    className="devos-input resize-none"
                  />
                </div>

                {/* Grid */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">

                  {/* Priority */}
                  <div>
                    <label className="block text-xs font-medium text-gray-400 mb-2">
                      Priority
                    </label>

                    <select
                      value={formData.priority}
                      onChange={(event) =>
                        setFormData({
                          ...formData,
                          priority: Number(event.target.value),
                        })
                      }
                      className="devos-input"
                    >
                      <option value={TaskPriority.LOW}>Low</option>
                      <option value={TaskPriority.MEDIUM}>Medium</option>
                      <option value={TaskPriority.HIGH}>High</option>
                      <option value={TaskPriority.CRITICAL}>
                        Critical
                      </option>
                    </select>
                  </div>

                  {/* Deadline */}
                  <div>
                    <label className="block text-xs font-medium text-gray-400 mb-2">
                      Deadline
                    </label>

                    <input
                      type="date"
                      value={formData.deadline}
                      onChange={(event) =>
                        setFormData({
                          ...formData,
                          deadline: event.target.value,
                        })
                      }
                      className="devos-input"
                    />
                  </div>
                </div>

                {/* Status */}
                {editingProject && (
                  <div>
                    <label className="block text-xs font-medium text-gray-400 mb-2">
                      Status
                    </label>

                    <select
                      value={formData.status}
                      onChange={(event) =>
                        setFormData({
                          ...formData,
                          status: Number(event.target.value),
                        })
                      }
                      className="devos-input"
                    >
                      <option value={ProjectStatus.ACTIVE}>
                        Active
                      </option>
                      <option value={ProjectStatus.COMPLETED}>
                        Completed
                      </option>
                      <option value={ProjectStatus.ARCHIVED}>
                        Archived
                      </option>
                    </select>
                  </div>
                )}
              </div>

              {/* Modal footer */}
              <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-devos-border bg-devos-dark/30">
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
                  disabled={saving || !formData.name.trim()}
                  className="
                    devos-button-primary
                    min-w-[120px]
                    disabled:opacity-50
                    disabled:cursor-not-allowed
                  "
                >
                  {saving
                    ? 'Saving...'
                    : editingProject
                      ? 'Save Changes'
                      : 'Create Project'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default Projects;