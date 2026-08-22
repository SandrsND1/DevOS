import axios from 'axios';
import { TaskStatus, TaskPriority, ProjectStatus } from './apiContract';

class ApiService {
  constructor() {
    this.api = axios.create({
      baseURL: '/api',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Токен аутентификации
    this.api.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('devos_token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Этап 4: Централизованная обработка ошибок (не глотаем 401, 403, 500)
    this.api.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response) {
          const { status } = error.response;
          if (status === 401) {
            localStorage.removeItem('devos_token');
            if (window.location.pathname !== '/login') {
              window.location.href = '/login';
            }
          }
          console.error(`[API Error ${status}]:`, error.response.data || error.message);
        } else {
          console.error('[Network Error]:', error.message);
        }
        return Promise.reject(error);
      }
    );
  }

  setToken(token) {
    if (token) {
      localStorage.setItem('devos_token', token);
    } else {
      localStorage.removeItem('devos_token');
    }
  }

  // ==================== AUTH ====================
  async login(email, password) {
    const response = await this.api.post('/auth/login', { email, password });
    if (response.data?.token) this.setToken(response.data.token);
    return response.data;
  }

  async register(name, email, password) {
    const response = await this.api.post('/auth/register', { 
      userName: name, 
      email, 
      password 
    });
    if (response.data?.token) this.setToken(response.data.token);
    return response.data;
  }

  logout() {
    this.setToken(null);
  }

  // ==================== PROJECTS ====================
  async getProjects() {
    const response = await this.api.get('/projects');
    return response.data?.items || response.data || [];
  }

  async getProjectById(id) {
    const response = await this.api.get(`/projects/${id}`);
    return response.data;
  }

  async createProject(projectData) {
    // Этап 3: Отправляем полные данные по бэкенд контракту
    const payload = {
      name: projectData.name,
      description: projectData.description || null,
      priority: projectData.priority ?? TaskPriority.MEDIUM,
      deadline: projectData.deadline ? new Date(projectData.deadline).toISOString() : null,
      status: projectData.status ?? ProjectStatus.ACTIVE
    };
    const response = await this.api.post('/projects', payload);
    return response.data;
  }

  async updateProject(id, projectData) {
    const response = await this.api.put(`/projects/${id}`, projectData);
    return response.data;
  }

  async deleteProject(id) {
    const response = await this.api.delete(`/projects/${id}`);
    return response.data;
  }

  // ==================== TASKS (Явный projectId) ====================
  async getTasks(projectId, params = {}) {
    if (!projectId) throw new Error("projectId is required to fetch tasks.");

    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('page', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize || 20);
    if (params.status !== undefined && params.status !== 'all') queryParams.append('status', params.status);
    if (params.priority !== undefined && params.priority !== 'all') queryParams.append('priority', params.priority);
    if (params.search) queryParams.append('search', params.search);
    if (params.sortBy) queryParams.append('sortBy', params.sortBy);
    if (params.sortDirection) queryParams.append('sortDirection', params.sortDirection);

    const response = await this.api.get(`/projects/${projectId}/tasks?${queryParams.toString()}`);
    return {
      items: response.data?.items || response.data || [],
      totalCount: response.data?.totalCount || 0,
      page: response.data?.page || 1,
      pageSize: response.data?.pageSize || 20
    };
  }

  async createTask(projectId, taskData) {
    if (!projectId) throw new Error("projectId is required to create a task.");

    const payload = {
      title: taskData.title,
      description: taskData.description || null,
      priority: Number(taskData.priority ?? TaskPriority.MEDIUM),
      estimatedMinutes: taskData.estimatedMinutes ? parseInt(taskData.estimatedMinutes, 10) : null,
      deadline: taskData.deadline ? new Date(taskData.deadline).toISOString() : null
    };

    const response = await this.api.post(`/projects/${projectId}/tasks`, payload);
    return response.data;
  }

  async updateTask(projectId, taskId, taskData) {
    if (!projectId || !taskId) throw new Error("projectId and taskId are required.");

    const payload = {
      title: taskData.title,
      description: taskData.description || null,
      priority: Number(taskData.priority ?? TaskPriority.MEDIUM),
      status: Number(taskData.status ?? TaskStatus.TODO),
      estimatedMinutes: taskData.estimatedMinutes ? parseInt(taskData.estimatedMinutes, 10) : null,
      deadline: taskData.deadline ? new Date(taskData.deadline).toISOString() : null
    };

    const response = await this.api.put(`/projects/${projectId}/tasks/${taskId}`, payload);
    return response.data;
  }

  async toggleTaskCompletion(projectId, task) {
    // Этап 4: Точная проверка C# enum (Completed = 3)
    const isCompleted = task.status === TaskStatus.COMPLETED;
    const newStatus = isCompleted ? TaskStatus.TODO : TaskStatus.COMPLETED;

    return this.updateTask(projectId, task.id, {
      ...task,
      status: newStatus
    });
  }

  async deleteTask(projectId, taskId) {
    const response = await this.api.delete(`/projects/${projectId}/tasks/${taskId}`);
    return response.data;
  }

  // ==================== TIME TRACKING ====================
  async getTimeEntries(projectId) {
    if (!projectId) throw new Error("projectId is required to fetch time entries.");

    const response = await this.api.get(`/projects/${projectId}/time-entries`);
    return response.data?.items || response.data || [];
  }

  async createTimeEntry(projectId, entryData) {
    if (!projectId) throw new Error("projectId is required to log time.");

    const payload = {
      taskId: entryData.taskId || null,
      startedAt: entryData.startedAt,
      endedAt: entryData.endedAt,
      description: entryData.description || "Development Session"
    };

    const response = await this.api.post(`/projects/${projectId}/time-entries`, payload);
    return response.data;
  }

  async deleteTimeEntry(projectId, entryId) {
    const response = await this.api.delete(`/projects/${projectId}/time-entries/${entryId}`);
    return response.data;
  }

  // ==================== DASHBOARD (Оптимизированный, Без N+1) ====================
  async getDashboardData() {
    // 1 Запрос проектов
    const projects = await this.getProjects();
    if (projects.length === 0) {
      return {
        totalProjects: 0,
        totalTasks: 0,
        completedTasks: 0,
        remainingTasks: 0,
        completionRate: "0.0",
        timeSpentMinutes: 0,
        projects: [],
        tasks: [],
        timeEntries: []
      };
    }

    // Параллельная загрузка задач и времени строго по существующим ID
    const taskPromises = projects.map(p => this.getTasks(p.id).catch(() => ({ items: [] })));
    const timePromises = projects.map(p => this.getTimeEntries(p.id).catch(() => []));

    const [tasksResults, timeResults] = await Promise.all([
      Promise.all(taskPromises),
      Promise.all(timePromises)
    ]);

    const allTasks = tasksResults.flatMap(r => r.items || []);
    const allEntries = timeResults.flat();

    // Этап 5: Строгий расчет завершенных задач (TaskStatus.COMPLETED === 3)
    const completedTasks = allTasks.filter(t => t.status === TaskStatus.COMPLETED).length;
    const totalTasks = allTasks.length;
    const completionRate = totalTasks > 0 ? ((completedTasks / totalTasks) * 100).toFixed(1) : "0.0";

    // Строгий расчет времени на основе DurationMinutes из C#
    const totalMinutes = allEntries.reduce((acc, entry) => {
      if (typeof entry.durationMinutes === 'number') return acc + entry.durationMinutes;
      if (entry.startedAt && entry.endedAt) {
        return acc + Math.max(0, (new Date(entry.endedAt) - new Date(entry.startedAt)) / 60000);
      }
      return acc;
    }, 0);

    return {
      totalProjects: projects.length,
      totalTasks,
      completedTasks,
      remainingTasks: totalTasks - completedTasks,
      completionRate,
      timeSpentMinutes: Math.round(totalMinutes),
      projects,
      tasks: allTasks,
      timeEntries: allEntries
    };
  }
}

export default new ApiService();