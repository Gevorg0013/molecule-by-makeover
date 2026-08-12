import * as projectRepository from '../database/repositories/projectRepository.js'
import type { Project, ProjectStatus } from '../database/types.js'

export function createProject(name: string, description: string): Project {
  return projectRepository.createProject(name, description)
}

export function getProject(id: string): Project | undefined {
  return projectRepository.getProject(id)
}

export function getLatestProject(): Project | undefined {
  return projectRepository.getLatestProject()
}

export function listProjects(limit = 20): Project[] {
  return projectRepository.listProjects(limit)
}

export function setStatus(id: string, status: ProjectStatus): void {
  projectRepository.updateProjectStatus(id, status)
}
