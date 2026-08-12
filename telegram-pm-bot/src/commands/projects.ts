import type { CommandHandler } from './types.js'
import * as projectService from '../services/projectService.js'

export const projectsCommand: CommandHandler = async (ctx) => {
  const projects = projectService.listProjects(15)
  if (projects.length === 0) {
    await ctx.reply('No projects yet. Send /new <description> to start one.')
    return
  }

  const lines = projects.map((p) => `${p.status === 'completed' ? '✅' : p.status === 'failed' ? '❌' : '•'} ${p.name} — ${p.status}`)
  await ctx.reply(`Recent projects:\n${lines.join('\n')}`)
}
