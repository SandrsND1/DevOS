// ============================================================
// DevOS API Contract
// ============================================================
// IMPORTANT:
// Numeric values MUST match C# enums on the backend.
// Do not change enum values unless the backend is changed too.
// ============================================================


// ==================== TASK STATUS ====================

export const TaskStatus = Object.freeze({
  TODO: 0,
  IN_PROGRESS: 1,
  BLOCKED: 2,
  COMPLETED: 3,
  CANCELLED: 4,
});

export const TaskStatusMeta = Object.freeze({
  [TaskStatus.TODO]: {
    label: 'To Do',
    shortLabel: 'Todo',
    color: 'gray',
    className: 'bg-gray-500/15 text-gray-300 border-gray-500/20',
  },

  [TaskStatus.IN_PROGRESS]: {
    label: 'In Progress',
    shortLabel: 'In Progress',
    color: 'blue',
    className: 'bg-blue-500/15 text-blue-400 border-blue-500/20',
  },

  [TaskStatus.BLOCKED]: {
    label: 'Blocked',
    shortLabel: 'Blocked',
    color: 'red',
    className: 'bg-red-500/15 text-red-400 border-red-500/20',
  },

  [TaskStatus.COMPLETED]: {
    label: 'Completed',
    shortLabel: 'Done',
    color: 'green',
    className: 'bg-green-500/15 text-green-400 border-green-500/20',
  },

  [TaskStatus.CANCELLED]: {
    label: 'Cancelled',
    shortLabel: 'Cancelled',
    color: 'zinc',
    className: 'bg-zinc-500/15 text-zinc-400 border-zinc-500/20',
  },
});

export const TASK_STATUSES = [
  {
    value: TaskStatus.TODO,
    label: 'To Do',
  },
  {
    value: TaskStatus.IN_PROGRESS,
    label: 'In Progress',
  },
  {
    value: TaskStatus.BLOCKED,
    label: 'Blocked',
  },
  {
    value: TaskStatus.COMPLETED,
    label: 'Completed',
  },
  {
    value: TaskStatus.CANCELLED,
    label: 'Cancelled',
  },
];

export const getTaskStatusMeta = (status) => {
  return (
    TaskStatusMeta[Number(status)] || {
      label: 'Unknown',
      shortLabel: 'Unknown',
      color: 'gray',
      className: 'bg-gray-500/15 text-gray-400 border-gray-500/20',
    }
  );
};


// ==================== TASK PRIORITY ====================

export const TaskPriority = Object.freeze({
  LOW: 0,
  MEDIUM: 1,
  HIGH: 2,
  CRITICAL: 3,
});

export const TaskPriorityMeta = Object.freeze({
  [TaskPriority.LOW]: {
    label: 'Low',
    color: 'gray',
    className: 'bg-gray-500/15 text-gray-400 border-gray-500/20',
  },

  [TaskPriority.MEDIUM]: {
    label: 'Medium',
    color: 'yellow',
    className: 'bg-yellow-500/15 text-yellow-400 border-yellow-500/20',
  },

  [TaskPriority.HIGH]: {
    label: 'High',
    color: 'orange',
    className: 'bg-orange-500/15 text-orange-400 border-orange-500/20',
  },

  [TaskPriority.CRITICAL]: {
    label: 'Critical',
    color: 'red',
    className: 'bg-red-500/15 text-red-400 border-red-500/20',
  },
});

export const TASK_PRIORITIES = [
  {
    value: TaskPriority.LOW,
    label: 'Low',
  },
  {
    value: TaskPriority.MEDIUM,
    label: 'Medium',
  },
  {
    value: TaskPriority.HIGH,
    label: 'High',
  },
  {
    value: TaskPriority.CRITICAL,
    label: 'Critical',
  },
];

export const getTaskPriorityMeta = (priority) => {
  return (
    TaskPriorityMeta[Number(priority)] || {
      label: 'Medium',
      color: 'yellow',
      className: 'bg-yellow-500/15 text-yellow-400 border-yellow-500/20',
    }
  );
};


// ==================== PROJECT STATUS ====================

export const ProjectStatus = Object.freeze({
  ACTIVE: 0,
  ARCHIVED: 1,
  COMPLETED: 2,
});

export const ProjectStatusMeta = Object.freeze({
  [ProjectStatus.ACTIVE]: {
    label: 'Active',
    color: 'green',
    className: 'bg-green-500/15 text-green-400 border-green-500/20',
  },

  [ProjectStatus.ARCHIVED]: {
    label: 'Archived',
    color: 'gray',
    className: 'bg-gray-500/15 text-gray-400 border-gray-500/20',
  },

  [ProjectStatus.COMPLETED]: {
    label: 'Completed',
    color: 'blue',
    className: 'bg-blue-500/15 text-blue-400 border-blue-500/20',
  },
});

export const PROJECT_STATUSES = [
  {
    value: ProjectStatus.ACTIVE,
    label: 'Active',
  },
  {
    value: ProjectStatus.ARCHIVED,
    label: 'Archived',
  },
  {
    value: ProjectStatus.COMPLETED,
    label: 'Completed',
  },
];

export const getProjectStatusMeta = (status) => {
  return (
    ProjectStatusMeta[Number(status)] || {
      label: 'Unknown',
      color: 'gray',
      className: 'bg-gray-500/15 text-gray-400 border-gray-500/20',
    }
  );
};


// ==================== HELPERS ====================

export const isTaskCompleted = (status) => {
  return Number(status) === TaskStatus.COMPLETED;
};

export const isTaskActive = (status) => {
  const numericStatus = Number(status);

  return (
    numericStatus === TaskStatus.TODO ||
    numericStatus === TaskStatus.IN_PROGRESS
  );
};

export const isTaskBlocked = (status) => {
  return Number(status) === TaskStatus.BLOCKED;
};

export const isTaskCancelled = (status) => {
  return Number(status) === TaskStatus.CANCELLED;
};

export const isProjectActive = (status) => {
  return Number(status) === ProjectStatus.ACTIVE;
};

export const isProjectCompleted = (status) => {
  return Number(status) === ProjectStatus.COMPLETED;
};

export const isProjectArchived = (status) => {
  return Number(status) === ProjectStatus.ARCHIVED;
};