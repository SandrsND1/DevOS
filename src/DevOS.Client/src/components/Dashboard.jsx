import React, { useEffect, useMemo, useState } from 'react';
import {
  Activity,
  ArrowRight,
  CheckCircle2,
  Clock3,
  FolderKanban,
  ListTodo,
  PlayCircle,
  Timer,
  TrendingUp,
  AlertCircle,
  Circle,
} from 'lucide-react';
import { Link } from 'react-router-dom';
import ApiService from '../services/ApiService';
import { TaskStatus, ProjectStatus } from '../services/apiContract';

function Dashboard() {
  const [stats, setStats] = useState({
    totalProjects: 0,
    totalTasks: 0,
    timeSpentMinutes: 0,
    completedTasks: 0,
    completionRate: 0,
  });

  const [projects, setProjects] = useState([]);
  const [tasks, setTasks] = useState([]);
  const [timeEntries, setTimeEntries] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      setError('');

      const data = await ApiService.getDashboardData();

      setStats({
        totalProjects: data.totalProjects || 0,
        totalTasks: data.totalTasks || 0,
        timeSpentMinutes: data.timeSpentMinutes || 0,
        completedTasks: data.completedTasks || 0,
        completionRate: parseFloat(data.completionRate) || 0,
      });

      setProjects(data.projects || []);
      setTasks(data.tasks || []);
      setTimeEntries(data.timeEntries || []);
    } catch (err) {
      console.error('Failed to fetch dashboard data:', err);
      setError('Unable to load dashboard data.');
    } finally {
      setLoading(false);
    }
  };

  const taskStats = useMemo(() => {
    return {
      todo: tasks.filter((task) => task.status === TaskStatus.TODO).length,
      inProgress: tasks.filter(
        (task) => task.status === TaskStatus.IN_PROGRESS
      ).length,
      blocked: tasks.filter(
        (task) => task.status === TaskStatus.BLOCKED
      ).length,
      completed: tasks.filter(
        (task) => task.status === TaskStatus.COMPLETED
      ).length,
      cancelled: tasks.filter(
        (task) => task.status === TaskStatus.CANCELLED
      ).length,
    };
  }, [tasks]);

  const formatDuration = (minutes) => {
    if (!minutes || minutes <= 0) return '0m';
    const hours = Math.floor(minutes / 60);
    const remainingMinutes = minutes % 60;
    if (hours === 0) return `${remainingMinutes}m`;
    if (remainingMinutes === 0) return `${hours}h`;
    return `${hours}h ${remainingMinutes}m`;
  };

  const formatDate = (date) => {
    if (!date) return 'No deadline';
    const parsed = new Date(date);
    if (Number.isNaN(parsed.getTime())) return 'No deadline';
    return parsed.toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const getProjectStatus = (status) => {
    switch (status) {
      case ProjectStatus.COMPLETED:
        return {
          label: 'Completed',
          className: 'text-emerald-400 bg-emerald-400/10 border-emerald-400/20',
        };
      case ProjectStatus.ARCHIVED:
        return {
          label: 'Archived',
          className: 'text-gray-400 bg-gray-400/10 border-gray-400/20',
        };
      default:
        return {
          label: 'Active',
          className: 'text-blue-400 bg-blue-400/10 border-blue-400/20',
        };
    }
  };

  const getTaskStatus = (status) => {
    switch (status) {
      case TaskStatus.COMPLETED:
        return {
          label: 'Completed',
          icon: CheckCircle2,
          className: 'text-emerald-400',
        };
      case TaskStatus.IN_PROGRESS:
        return {
          label: 'In Progress',
          icon: PlayCircle,
          className: 'text-blue-400',
        };
      case TaskStatus.BLOCKED:
        return {
          label: 'Blocked',
          icon: AlertCircle,
          className: 'text-red-400',
        };
      case TaskStatus.CANCELLED:
        return {
          label: 'Cancelled',
          icon: Circle,
          className: 'text-gray-500',
        };
      default:
        return {
          label: 'Todo',
          icon: Circle,
          className: 'text-gray-500',
        };
    }
  };

  const recentTasks = useMemo(() => {
    return [...tasks]
      .sort((a, b) => {
        const aDate = new Date(a.updatedAt || a.createdAt || 0).getTime();
        const bDate = new Date(b.updatedAt || b.createdAt || 0).getTime();
        return bDate - aDate;
      })
      .slice(0, 6);
  }, [tasks]);

  const recentProjects = useMemo(() => projects.slice(0, 4), [projects]);

  const averageTimePerTask =
    stats.totalTasks > 0
      ? Math.round(stats.timeSpentMinutes / stats.totalTasks)
      : 0;

  if (loading) {
    return (
      <div className="max-w-[1440px] mx-auto p-4 sm:p-6 lg:p-8">
        <div className="animate-pulse space-y-6">
          <div className="h-8 w-48 rounded-lg bg-devos-surface" />
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
            {[1, 2, 3, 4].map((item) => (
              <div key={item} className="h-32 rounded-xl bg-devos-surface border border-devos-border" />
            ))}
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-[1440px] mx-auto p-4 sm:p-6 lg:p-8">
        <div className="rounded-xl border border-red-500/20 bg-red-500/5 p-6">
          <div className="flex items-start gap-3">
            <AlertCircle className="w-5 h-5 text-red-400 mt-0.5" />
            <div>
              <h2 className="text-sm font-semibold text-white">Dashboard unavailable</h2>
              <p className="text-sm text-gray-500 mt-1">{error}</p>
              <button
                type="button"
                onClick={fetchDashboardData}
                className="mt-4 text-sm font-medium text-devos-primary hover:text-devos-secondary transition-colors"
              >
                Try again
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <main className="max-w-[1440px] mx-auto p-4 sm:p-6 lg:p-8 space-y-6">
      {/* Header */}
      <section className="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-4">
        <div>
          <p className="text-xs font-medium uppercase tracking-wider text-devos-primary mb-2">
            Workspace overview
          </p>
          <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-white">Dashboard</h1>
          <p className="text-sm text-gray-500 mt-1">
            Keep track of your projects, tasks and development time.
          </p>
        </div>

        <div className="flex items-center gap-2">
          <Link
            to="/tasks"
            className="inline-flex items-center gap-2 px-3.5 py-2 rounded-lg bg-devos-primary hover:bg-devos-secondary text-sm font-medium text-white transition-colors"
          >
            <ListTodo className="w-4 h-4" />
            View Tasks
          </Link>

          <Link
            to="/projects"
            className="hidden sm:inline-flex items-center gap-2 px-3.5 py-2 rounded-lg border border-devos-border bg-devos-surface/50 hover:bg-devos-surface text-sm font-medium text-gray-300 transition-colors"
          >
            Projects
          </Link>
        </div>
      </section>

      {/* Main metrics */}
      <section className="grid grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4">
        <MetricCard
          label="Projects"
          value={stats.totalProjects}
          icon={FolderKanban}
          description="Total workspace projects"
          iconClass="text-blue-400"
          iconBackground="bg-blue-400/10"
        />

        <MetricCard
          label="Tasks"
          value={stats.totalTasks}
          icon={ListTodo}
          description={`${taskStats.inProgress} currently in progress`}
          iconClass="text-violet-400"
          iconBackground="bg-violet-400/10"
        />

        <MetricCard
          label="Time tracked"
          value={formatDuration(stats.timeSpentMinutes)}
          icon={Clock3}
          description={`${averageTimePerTask}m average per task`}
          iconClass="text-amber-400"
          iconBackground="bg-amber-400/10"
        />

        <MetricCard
          label="Completion"
          value={`${stats.completionRate.toFixed(1)}%`}
          icon={TrendingUp}
          description={`${stats.completedTasks} tasks completed`}
          iconClass="text-emerald-400"
          iconBackground="bg-emerald-400/10"
        />
      </section>

      {/* Productivity overview */}
      <section className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* Task progress */}
        <div className="lg:col-span-2 devos-card p-5 sm:p-6">
          <div className="flex items-start justify-between">
            <div>
              <div className="flex items-center gap-2">
                <div className="w-8 h-8 rounded-lg bg-devos-primary/10 flex items-center justify-center">
                  <Activity className="w-4 h-4 text-devos-primary" />
                </div>
                <h2 className="text-sm font-semibold text-white">Task progress</h2>
              </div>
              <p className="text-xs text-gray-500 mt-2">Current distribution of your tasks</p>
            </div>

            <Link to="/tasks" className="text-gray-500 hover:text-white transition-colors">
              <ArrowRight className="w-4 h-4" />
            </Link>
          </div>

          <div className="mt-8">
            <div className="flex items-end justify-between mb-3">
              <div>
                <span className="text-3xl font-bold text-white">{stats.completedTasks}</span>
                <span className="text-sm text-gray-500 ml-2">of {stats.totalTasks} completed</span>
              </div>
              <span className="text-sm font-medium text-emerald-400">
                {stats.completionRate.toFixed(0)}%
              </span>
            </div>

            <div className="h-2 rounded-full bg-devos-dark overflow-hidden">
              <div
                className="h-full rounded-full bg-devos-primary transition-all duration-500"
                style={{ width: `${Math.min(stats.completionRate, 100)}%` }}
              />
            </div>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mt-8">
            <TaskStatusCard label="Todo" value={taskStats.todo} dotClass="bg-gray-500" />
            <TaskStatusCard label="In Progress" value={taskStats.inProgress} dotClass="bg-blue-400" />
            <TaskStatusCard label="Blocked" value={taskStats.blocked} dotClass="bg-red-400" />
            <TaskStatusCard label="Completed" value={taskStats.completed} dotClass="bg-emerald-400" />
          </div>
        </div>

        {/* Time summary */}
        <div className="devos-card p-5 sm:p-6">
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 rounded-lg bg-amber-400/10 flex items-center justify-center">
              <Timer className="w-4 h-4 text-amber-400" />
            </div>
            <h2 className="text-sm font-semibold text-white">Time tracking</h2>
          </div>
          <p className="text-xs text-gray-500 mt-2">Development time recorded</p>

          <div className="mt-8">
            <p className="text-3xl font-bold text-white">{formatDuration(stats.timeSpentMinutes)}</p>
            <p className="text-xs text-gray-500 mt-1">across {timeEntries.length} sessions</p>
          </div>

          <div className="mt-8 space-y-3">
            <div className="flex items-center justify-between text-xs">
              <span className="text-gray-500">Average session</span>
              <span className="text-gray-300 font-medium">
                {timeEntries.length > 0
                  ? formatDuration(Math.round(stats.timeSpentMinutes / timeEntries.length))
                  : '0m'}
              </span>
            </div>

            <div className="flex items-center justify-between text-xs">
              <span className="text-gray-500">Average / task</span>
              <span className="text-gray-300 font-medium">{averageTimePerTask}m</span>
            </div>
          </div>

          <Link
            to="/time-tracking"
            className="flex items-center justify-between w-full mt-8 px-3 py-2.5 rounded-lg border border-devos-border bg-devos-dark/50 hover:bg-devos-dark text-xs font-medium text-gray-400 hover:text-white transition-colors"
          >
            Open time tracking
            <ArrowRight className="w-4 h-4" />
          </Link>
        </div>
      </section>

      {/* Projects + Recent tasks */}
      <section className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* Projects */}
        <div className="devos-card overflow-hidden">
          <div className="flex items-center justify-between p-5 border-b border-devos-border">
            <div>
              <h2 className="text-sm font-semibold text-white">Recent projects</h2>
              <p className="text-xs text-gray-500 mt-1">Your current workspace</p>
            </div>
            <Link to="/projects" className="text-xs font-medium text-gray-500 hover:text-white transition-colors">
              View all
            </Link>
          </div>

          {recentProjects.length === 0 ? (
            <EmptyState
              icon={FolderKanban}
              title="No projects yet"
              description="Create your first project to get started."
              actionLabel="Create project"
              actionTo="/projects"
            />
          ) : (
            <div className="divide-y divide-devos-border">
              {recentProjects.map((project) => {
                const status = getProjectStatus(project.status);
                return (
                  <Link
                    key={project.id}
                    to="/projects"
                    className="flex items-center gap-3 px-5 py-4 hover:bg-devos-surface/60 transition-colors cursor-pointer group"
                  >
                    <div className="w-9 h-9 rounded-lg bg-devos-primary/10 flex items-center justify-center shrink-0">
                      <FolderKanban className="w-4 h-4 text-devos-primary" />
                    </div>

                    <div className="min-w-0 flex-1">
                      <h3 className="text-sm font-medium text-gray-200 truncate group-hover:text-devos-primary transition-colors">
                        {project.name}
                      </h3>

                      <div className="flex items-center gap-2 mt-1">
                        <span className={`inline-flex items-center px-1.5 py-0.5 rounded-md border text-[10px] font-medium ${status.className}`}>
                          {status.label}
                        </span>

                        {project.deadline && (
                          <span className="text-[10px] text-gray-600">
                            Due {formatDate(project.deadline)}
                          </span>
                        )}
                      </div>
                    </div>
                  </Link>
                );
              })}
            </div>
          )}
        </div>

        {/* Recent tasks */}
        <div className="devos-card overflow-hidden">
          <div className="flex items-center justify-between p-5 border-b border-devos-border">
            <div>
              <h2 className="text-sm font-semibold text-white">Recent tasks</h2>
              <p className="text-xs text-gray-500 mt-1">Latest activity across your projects</p>
            </div>
            <Link to="/tasks" className="text-xs font-medium text-gray-500 hover:text-white transition-colors">
              View all
            </Link>
          </div>

          {recentTasks.length === 0 ? (
            <EmptyState
              icon={ListTodo}
              title="No tasks yet"
              description="Create a task to start tracking your work."
              actionLabel="Open tasks"
              actionTo="/tasks"
            />
          ) : (
            <div className="divide-y divide-devos-border">
              {recentTasks.map((task) => {
                const status = getTaskStatus(task.status);
                const StatusIcon = status.icon;

                return (
                  <Link
                    key={task.id}
                    to="/tasks"
                    className="flex items-center gap-3 px-5 py-4 hover:bg-devos-surface/60 transition-colors cursor-pointer group"
                  >
                    <StatusIcon className={`w-4 h-4 shrink-0 ${status.className}`} />

                    <div className="min-w-0 flex-1">
                      <h3
                        className={`text-sm font-medium truncate group-hover:text-devos-primary transition-colors ${
                          task.status === TaskStatus.COMPLETED ? 'text-gray-500 line-through' : 'text-gray-200'
                        }`}
                      >
                        {task.title}
                      </h3>

                      <p className="text-[10px] text-gray-600 mt-1">{status.label}</p>
                    </div>

                    {task.deadline && (
                      <span className="text-[10px] text-gray-600 shrink-0">
                        {formatDate(task.deadline)}
                      </span>
                    )}
                  </Link>
                );
              })}
            </div>
          )}
        </div>
      </section>

      {/* Bottom summary */}
      <section className="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <SummaryCard
          icon={CheckCircle2}
          label="Completed tasks"
          value={stats.completedTasks}
          description="Successfully finished"
          className="text-emerald-400"
        />

        <SummaryCard
          icon={AlertCircle}
          label="Remaining tasks"
          value={Math.max(stats.totalTasks - stats.completedTasks, 0)}
          description="Still requiring attention"
          className="text-orange-400"
        />

        <SummaryCard
          icon={Clock3}
          label="Average task time"
          value={formatDuration(averageTimePerTask)}
          description="Based on tracked time"
          className="text-blue-400"
        />
      </section>
    </main>
  );
}

function MetricCard({ label, value, icon: Icon, description, iconClass, iconBackground }) {
  return (
    <div className="devos-card p-4 sm:p-5">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="text-xs font-medium text-gray-500">{label}</p>
          <p className="text-2xl sm:text-3xl font-bold text-white mt-2 tracking-tight">{value}</p>
        </div>

        <div className={`w-9 h-9 rounded-lg flex items-center justify-center shrink-0 ${iconBackground}`}>
          <Icon className={`w-4 h-4 ${iconClass}`} />
        </div>
      </div>
      <p className="text-[11px] text-gray-600 mt-3 truncate">{description}</p>
    </div>
  );
}

function TaskStatusCard({ label, value, dotClass }) {
  return (
    <div className="rounded-lg border border-devos-border bg-devos-dark/40 px-3 py-3">
      <div className="flex items-center gap-2">
        <span className={`w-1.5 h-1.5 rounded-full ${dotClass}`} />
        <span className="text-xs text-gray-500">{label}</span>
      </div>
      <p className="text-lg font-semibold text-white mt-2">{value}</p>
    </div>
  );
}

function SummaryCard({ icon: Icon, label, value, description, className }) {
  return (
    <div className="devos-card p-4">
      <div className="flex items-center gap-3">
        <div className={`w-9 h-9 rounded-lg bg-devos-dark border border-devos-border flex items-center justify-center ${className}`}>
          <Icon className="w-4 h-4" />
        </div>

        <div>
          <p className="text-xs text-gray-500">{label}</p>
          <div className="flex items-baseline gap-2 mt-0.5">
            <span className="text-lg font-semibold text-white">{value}</span>
            <span className="text-[10px] text-gray-600">{description}</span>
          </div>
        </div>
      </div>
    </div>
  );
}

function EmptyState({ icon: Icon, title, description, actionLabel, actionTo }) {
  return (
    <div className="px-5 py-12 text-center">
      <div className="w-10 h-10 mx-auto rounded-xl bg-devos-dark border border-devos-border flex items-center justify-center">
        <Icon className="w-4 h-4 text-gray-600" />
      </div>

      <h3 className="text-sm font-medium text-gray-300 mt-3">{title}</h3>
      <p className="text-xs text-gray-600 mt-1 max-w-xs mx-auto">{description}</p>

      <Link
        to={actionTo}
        className="inline-flex items-center gap-1.5 mt-4 text-xs font-medium text-devos-primary hover:text-devos-secondary transition-colors"
      >
        {actionLabel}
        <ArrowRight className="w-3.5 h-3.5" />
      </Link>
    </div>
  );
}

export default Dashboard;