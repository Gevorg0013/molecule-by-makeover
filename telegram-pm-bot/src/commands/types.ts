export interface CommandContext {
  args: string
  reply: (text: string) => Promise<void>
}

export type CommandHandler = (ctx: CommandContext) => Promise<void>
